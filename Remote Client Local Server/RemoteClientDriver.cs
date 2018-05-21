using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

using ASCOM.Utilities;
using ASCOM.DeviceInterface;

using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;

using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace ASCOM.Remote
{
    public static class RemoteClientDriver
    {
        #region Private variables and constants

        //Private constants
        private const string NOT_CONNECTED_MESSAGE = "ASCOM Remote Client Driver is not connected.";
        private const string RANK_VARIABLE_NAME = "Rank";
        private const string ARRAYTYPE_VARIABLE_NAME = "Type";

        private const string FIND_TYPE_AND_RANK_REGEX_PATTERN = @"^*""Type"":(?<" + ARRAYTYPE_VARIABLE_NAME + @">\d*),""Rank"":(?<" + RANK_VARIABLE_NAME + @">\d*)"; // Regex to extract array type and rank from the returned JSON imagarray and imagearrayvariant responses

        //Private variables
        private static TraceLoggerPlus TLLocalServer;
        private static bool traceState = true;
        private static bool debugTraceState = true;

        private static int uniqueClientNumber = 0; // Unique number that increments on each call to UniqueClientNumber
        private static int uniqueTransactionNumber = 0; // Unique number that increments on each call to TransactionNumber

        private readonly static object connectLockObject = new object();
        private static ConcurrentDictionary<long, bool> connectStates;

        #endregion

        #region Initialiser

        /// <summary>
        /// Static initialiser to set up the objects we need at run time
        /// </summary>
        static RemoteClientDriver()
        {
            try
            {
                TLLocalServer = new TraceLoggerPlus("", "RemoteClientLocalServer")
                {
                    Enabled = false
                }; // Trace state is set in ReadProfile, immediately after being read from the Profile

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                TLLocalServer.LogMessage("RemoteClient", "Initialising, Trace: " + traceState + ", Version: " + version.ToString());

                connectStates = new ConcurrentDictionary<long, bool>();

                TLLocalServer.LogMessage("RemoteClient", "Initialisation complete.");
            }
            catch (Exception ex)
            {
                TLLocalServer.LogMessageCrLf("RemoteClient", ex.ToString());
                MessageBox.Show(ex.ToString(), "Error initialising the RemoteClient Telescope", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Utility code

        /// <summary>
        /// Returns a unique client numnber to the calling instance
        /// </summary>
        public static int GetUniqueClientNumber()
        {
            Interlocked.Increment(ref uniqueClientNumber);
            return uniqueClientNumber;
        }

        /// <summary>
        /// Returns a unique client numnber to the calling instance
        /// </summary>
        public static int TransactionNumber()
        {
            Interlocked.Increment(ref uniqueTransactionNumber);
            return uniqueTransactionNumber;
        }

        /// <summary>
        /// Tests whether the hub is already conected
        /// </summary>
        /// <param name="clientNumber">Number of the client making the call</param>
        /// <returns>Boolean true if the hub is already connected</returns>
        public static bool IsHardwareConnected(TraceLoggerPlus TL)
        {
            if (debugTraceState) TL.LogMessage("IsHardwareConnected", "Number of connected devices: " + connectStates.Count + ", Returning: " + (connectStates.Count > 0).ToString());
            return connectStates.Count > 0;
        }

        /// <summary>
        /// Test whether a particular client is already connected
        /// </summary>
        /// <param name="clientNumber">Number of the calling client</param>
        /// <returns></returns>
        public static bool IsClientConnected(int clientNumber, RestClient client, TraceLoggerPlus TL)
        {
            TL.LogMessage(clientNumber, "IsClientConnected", "Number of connected devices: " + connectStates.Count + ", Returning: " + connectStates.ContainsKey(clientNumber).ToString());

            return connectStates.ContainsKey(clientNumber);
        }

        /// <summary>
        /// Returns the number of connected clients
        /// </summary>
        public static int ConnectionCount(TraceLoggerPlus TL)
        {
            TL.LogMessage("ConnectionCount", connectStates.Count.ToString());
            return connectStates.Count;
        }

        /// <summary>
        /// Test whether the we are connected, if not throw a NotConnectedException
        /// </summary>
        /// <param name="MethodName">Name of the calling method</param>
        static void CheckConnected(string MethodName, TraceLoggerPlus TL)
        {
            if (!IsHardwareConnected(TL)) throw new NotConnectedException(MethodName + " - " + NOT_CONNECTED_MESSAGE);
        }

        /// <summary>
        /// Return name of current method
        /// </summary>
        /// <returns>Name of current method</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        public static void SetClientTimeout(RestClient client, int serverResponseTimeout)
        {
            client.Timeout = serverResponseTimeout * 1000;
            client.ReadWriteTimeout = serverResponseTimeout * 1000;
        }

        public static void ConnectToRemoteServer(ref RestClient client, string ipAddressString, decimal portNumber, string serviceType, TraceLoggerPlus TL, int clientNumber, string deviceType, int serverResponseTimeout, string userName, string password)
        {
            TL.LogMessage(clientNumber, deviceType, "Connecting to device: " + ipAddressString + ":" + portNumber.ToString());

            string clientHostAddress = string.Format("{0}://{1}:{2}", serviceType, ipAddressString, portNumber.ToString());
            TL.LogMessage(clientNumber, deviceType, "Client host address: " + clientHostAddress);

            if (client != null)
            {
                client.ClearHandlers();
                client = null;
            }

            client = new RestClient(clientHostAddress)
            {
                PreAuthenticate = true
            };
            TL.LogMessage(clientNumber, deviceType, "Creating Authenticator");
            client.Authenticator = new HttpBasicAuthenticator(userName.Unencrypt(TL), password.Unencrypt(TL)); // Need to decrypt the user name and password so the correct values are sent to the remote server
            SetClientTimeout(client, serverResponseTimeout);
        }

        #endregion

        #region Profile management
        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        public static void ReadProfile(int clientNumber, TraceLoggerPlus TL, string deviceType, string driverProgID,
                                       ref bool traceState,
                                       ref bool debugTraceState,
                                       ref string ipAddressString,
                                       ref decimal portNumber,
                                       ref decimal remoteDeviceNumber,
                                       ref string serviceType,
                                       ref int establishConnectionTimeout,
                                       ref int standardServerResponseTimeout,
                                       ref int longServerResponseTimeout,
                                       ref string userName,
                                       ref string password,
                                       ref bool manageConnectLocally
                                       )
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = deviceType;

                // Initialise the logging trace state from the Profile
                traceState = Convert.ToBoolean(driverProfile.GetValue(driverProgID, SharedConstants.TRACE_LEVEL_PROFILENAME, string.Empty, SharedConstants.CLIENT_TRACE_LEVEL_DEFAULT), CultureInfo.InvariantCulture);
                TL.Enabled = traceState; // Set the logging state immediately after this has been retrieved from Profile

                // Initialise other variables from the Profile
                debugTraceState = Convert.ToBoolean(driverProfile.GetValue(driverProgID, SharedConstants.DEBUG_TRACE_PROFILENAME, string.Empty, SharedConstants.DEBUG_TRACE_DEFAULT), CultureInfo.InvariantCulture);
                ipAddressString = driverProfile.GetValue(driverProgID, SharedConstants.IPADDRESS_PROFILENAME, string.Empty, SharedConstants.IPADDRESS_DEFAULT);
                portNumber = Convert.ToDecimal(driverProfile.GetValue(driverProgID, SharedConstants.PORTNUMBER_PROFILENAME, string.Empty, SharedConstants.PORTNUMBER_DEFAULT), CultureInfo.InvariantCulture);
                remoteDeviceNumber = Convert.ToDecimal(driverProfile.GetValue(driverProgID, SharedConstants.REMOTE_DEVICE_NUMBER_PROFILENAME, string.Empty, SharedConstants.REMOTE_DEVICE_NUMBER_DEFAULT), CultureInfo.InvariantCulture);
                serviceType = driverProfile.GetValue(driverProgID, SharedConstants.SERVICE_TYPE_PROFILENAME, string.Empty, SharedConstants.SERVICE_TYPE_DEFAULT);
                establishConnectionTimeout = Convert.ToInt32(driverProfile.GetValue(driverProgID, SharedConstants.ESTABLISH_CONNECTION_TIMEOUT_PROFILENAME, string.Empty, SharedConstants.ESTABLISH_CONNECTION_TIMEOUT_DEFAULT), CultureInfo.InvariantCulture);
                standardServerResponseTimeout = Convert.ToInt32(driverProfile.GetValue(driverProgID, SharedConstants.STANDARD_SERVER_RESPONSE_TIMEOUT_PROFILENAME, string.Empty, SharedConstants.STANDARD_SERVER_RESPONSE_TIMEOUT_DEFAULT), CultureInfo.InvariantCulture);
                longServerResponseTimeout = Convert.ToInt32(driverProfile.GetValue(driverProgID, SharedConstants.LONG_SERVER_RESPONSE_TIMEOUT_PROFILENAME, string.Empty, SharedConstants.LONG_SERVER_RESPONSE_TIMEOUT_DEFAULT), CultureInfo.InvariantCulture);
                userName = driverProfile.GetValue(driverProgID, SharedConstants.USERNAME_PROFILENAME, string.Empty, SharedConstants.USERNAME_DEFAULT);
                password = driverProfile.GetValue(driverProgID, SharedConstants.PASSWORD_PROFILENAME, string.Empty, SharedConstants.PASSWORD_DEFAULT);
                manageConnectLocally = Convert.ToBoolean(driverProfile.GetValue(driverProgID, SharedConstants.MANAGE_CONNECT_LOCALLY_PROFILENAME, string.Empty, SharedConstants.MANAGE_CONNECT_LOCALLY_DEFAULT));

                TL.DebugTraceState = debugTraceState; // Save the debug state for use when needed wherever the tracelogger is used

                TL.LogMessage(clientNumber, "ReadProfile", string.Format("New values: Device IP: {0} {1}, Remote device number: {2}", ipAddressString, portNumber.ToString(), remoteDeviceNumber.ToString()));
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        public static void WriteProfile(int clientNumber, TraceLoggerPlus TL, string deviceType, string driverProgID,
                                        bool traceState,
                                        bool debugTraceState,
                                        string ipAddressString,
                                        decimal portNumber,
                                        decimal remoteDeviceNumber,
                                        string serviceType,
                                        int establishConnectionTimeout,
                                        int standardServerResponseTimeout,
                                        int longServerResponseTimeout,
                                        string userName,
                                        string password,
                                        bool manageConnectLocally
                                        )
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = deviceType;

                // Save the variable state to the Profile
                driverProfile.WriteValue(driverProgID, SharedConstants.TRACE_LEVEL_PROFILENAME, traceState.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.DEBUG_TRACE_PROFILENAME, debugTraceState.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.IPADDRESS_PROFILENAME, ipAddressString.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.PORTNUMBER_PROFILENAME, portNumber.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.REMOTE_DEVICE_NUMBER_PROFILENAME, remoteDeviceNumber.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.SERVICE_TYPE_PROFILENAME, serviceType.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.ESTABLISH_CONNECTION_TIMEOUT_PROFILENAME, establishConnectionTimeout.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.STANDARD_SERVER_RESPONSE_TIMEOUT_PROFILENAME, standardServerResponseTimeout.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.LONG_SERVER_RESPONSE_TIMEOUT_PROFILENAME, longServerResponseTimeout.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.USERNAME_PROFILENAME, userName.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.PASSWORD_PROFILENAME, password.ToString(CultureInfo.InvariantCulture));
                driverProfile.WriteValue(driverProgID, SharedConstants.MANAGE_CONNECT_LOCALLY_PROFILENAME, manageConnectLocally.ToString(CultureInfo.InvariantCulture));

                TL.DebugTraceState = debugTraceState; // Save the new debug state for use when needed wherever the tracelogger is used

                TL.LogMessage(clientNumber, "WriteProfile", string.Format("New values: Device IP: {0} {1}, Remote device number: {2}", ipAddressString, portNumber.ToString(), remoteDeviceNumber.ToString()));
            }
        }

        #endregion

        #region Remote access methods

        public static void CallMethodWithNoParameters(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static T GetValue<T>(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>();
            return SendToRemoteDriver<T>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }

        public static void SetBool(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, bool parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { method, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetInt(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, int parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { method, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetShort(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { method, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetDouble(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, double parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { method, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetDoubleWithShortParameter(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short index, double parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, index.ToString(CultureInfo.InvariantCulture) },
                { SharedConstants.VALUE_PARAMETER_NAME, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetBoolWithShortParameter(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short index, bool parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, index.ToString(CultureInfo.InvariantCulture) },
                { SharedConstants.STATE_PARAMETER_NAME, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static void SetStringWithShortParameter(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short index, string parmeterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, index.ToString(CultureInfo.InvariantCulture) },
                { SharedConstants.NAME_PARAMETER_NAME, parmeterValue.ToString(CultureInfo.InvariantCulture) }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, method, Parameters, Method.PUT);
        }

        public static string GetStringIndexedString(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, string parameterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.SENSORNAME_PARAMETER_NAME, parameterValue }
            };
            return SendToRemoteDriver<string>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }
        public static double GetStringIndexedDouble(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, string parameterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.SENSORNAME_PARAMETER_NAME, parameterValue }
            };
            return SendToRemoteDriver<double>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }

        public static double GetShortIndexedDouble(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short parameterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, parameterValue.ToString(CultureInfo.InvariantCulture) }
            };
            return SendToRemoteDriver<double>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }

        public static bool GetShortIndexedBool(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short parameterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, parameterValue.ToString(CultureInfo.InvariantCulture) }
            };
            return SendToRemoteDriver<bool>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }

        public static string GetShortIndexedString(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, short parameterValue)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ID_PARAMETER_NAME, parameterValue.ToString(CultureInfo.InvariantCulture) }
            };
            return SendToRemoteDriver<string>(clientNumber, client, URIBase, TL, method, Parameters, Method.GET);
        }

        public static T SendToRemoteDriver<T>(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string method, Dictionary<string, string> Parameters, Method HttpMethod)
        {
            try
            {
                const string LOG_FORMAT_STRING = "Client Txn ID: {0}, Server Txn ID: {1}, Value: {2}, Method: {3}";

                RestResponseBase restResponseBase = null; // This has to be the base class of the data type classes in order for exception and error responses to be handled generically
                RestRequest request = new RestRequest(URIBase + method, HttpMethod)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter(SharedConstants.CLIENTID_PARAMETER_NAME, clientNumber.ToString());
                int transaction = TransactionNumber();
                request.AddParameter(SharedConstants.CLIENTTRANSACTION_PARAMETER_NAME, transaction.ToString());

                foreach (KeyValuePair<string, string> parameter in Parameters) // Add any supplied parameters to the request
                {
                    request.AddParameter(parameter.Key, parameter.Value);
                }

                if (debugTraceState) TL.LogMessage(clientNumber, method, "Client Txn ID: " + transaction.ToString() + ", Sending command to remote server");
                IRestResponse response = client.Execute(request);
                string responseContent;
                if (response.Content.Length > 100) responseContent = response.Content.Substring(0, 100);
                else responseContent = response.Content;
                TL.LogMessage(clientNumber, method, string.Format("Response Status: '{0}', Response: {1}", response.StatusDescription, responseContent));

                if ((response.ResponseStatus == ResponseStatus.Completed) & (response.StatusCode == System.Net.HttpStatusCode.OK))
                {
                    // GENERAL MULTI-DEVICE TYPES
                    if (typeof(T) == typeof(bool))
                    {
                        BoolResponse boolResponse = JsonConvert.DeserializeObject<BoolResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, boolResponse.ClientTransactionID, boolResponse.ServerTransactionID, boolResponse.Value.ToString(), boolResponse.Method));
                        if (boolResponse.DriverException == null) return (T)((object)boolResponse.Value);
                        restResponseBase = (RestResponseBase)boolResponse;
                    }
                    if (typeof(T) == typeof(float))
                    {
                        // Handle float as double over the web, remembering to convert the returned value to float
                        DoubleResponse doubleResponse = JsonConvert.DeserializeObject<DoubleResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, doubleResponse.ClientTransactionID, doubleResponse.ServerTransactionID, doubleResponse.Value.ToString(), doubleResponse.Method));
                        float floatValue = (float)doubleResponse.Value;
                        if (doubleResponse.DriverException == null) return (T)((object)floatValue);
                        restResponseBase = (RestResponseBase)doubleResponse;
                    }
                    if (typeof(T) == typeof(double))
                    {
                        DoubleResponse doubleResponse = JsonConvert.DeserializeObject<DoubleResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, doubleResponse.ClientTransactionID, doubleResponse.ServerTransactionID, doubleResponse.Value.ToString(), doubleResponse.Method));
                        if (doubleResponse.DriverException == null) return (T)((object)doubleResponse.Value);
                        restResponseBase = (RestResponseBase)doubleResponse;
                    }
                    if (typeof(T) == typeof(string))
                    {
                        StringResponse stringResponse = JsonConvert.DeserializeObject<StringResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, stringResponse.ClientTransactionID, stringResponse.ServerTransactionID, stringResponse.Value.ToString(), stringResponse.Method));
                        if (stringResponse.DriverException == null) return (T)((object)stringResponse.Value);
                        restResponseBase = (RestResponseBase)stringResponse;
                    }
                    if (typeof(T) == typeof(string[]))
                    {
                        StringArrayResponse stringArrayResponse = JsonConvert.DeserializeObject<StringArrayResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, stringArrayResponse.ClientTransactionID, stringArrayResponse.ServerTransactionID, stringArrayResponse.Value.Count(), stringArrayResponse.Method));
                        if (stringArrayResponse.DriverException == null) return (T)((object)stringArrayResponse.Value);
                        restResponseBase = (RestResponseBase)stringArrayResponse;
                    }
                    if (typeof(T) == typeof(short))
                    {
                        ShortResponse shortResponse = JsonConvert.DeserializeObject<ShortResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, shortResponse.ClientTransactionID, shortResponse.ServerTransactionID, shortResponse.Value.ToString(), shortResponse.Method));
                        if (shortResponse.DriverException == null) return (T)((object)shortResponse.Value);
                        restResponseBase = (RestResponseBase)shortResponse;
                    }
                    if (typeof(T) == typeof(int))
                    {
                        IntResponse intResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, intResponse.ClientTransactionID, intResponse.ServerTransactionID, intResponse.Value.ToString(), intResponse.Method));
                        if (intResponse.DriverException == null) return (T)((object)intResponse.Value);
                        restResponseBase = (RestResponseBase)intResponse;
                    }
                    if (typeof(T) == typeof(int[]))
                    {
                        IntArray1DResponse intArrayResponse = JsonConvert.DeserializeObject<IntArray1DResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, intArrayResponse.ClientTransactionID, intArrayResponse.ServerTransactionID, intArrayResponse.Value.Count(), intArrayResponse.Method));
                        if (intArrayResponse.DriverException == null) return (T)((object)intArrayResponse.Value);
                        restResponseBase = (RestResponseBase)intArrayResponse;
                    }
                    if (typeof(T) == typeof(DateTime))
                    {
                        DateTimeResponse dateTimeResponse = JsonConvert.DeserializeObject<DateTimeResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, dateTimeResponse.ClientTransactionID, dateTimeResponse.ServerTransactionID, dateTimeResponse.Value.ToString(), dateTimeResponse.Method));
                        if (dateTimeResponse.DriverException == null) return (T)((object)dateTimeResponse.Value);
                        restResponseBase = (RestResponseBase)dateTimeResponse;
                    }
                    if (typeof(T) == typeof(List<string>)) // Used for ArrayLists of string
                    {
                        StringListResponse stringListResponse = JsonConvert.DeserializeObject<StringListResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, stringListResponse.ClientTransactionID, stringListResponse.ServerTransactionID, stringListResponse.Value.Count.ToString(), stringListResponse.Method));
                        if (stringListResponse.DriverException == null) return (T)((object)stringListResponse.Value);
                        restResponseBase = (RestResponseBase)stringListResponse;
                    }
                    if (typeof(T) == typeof(NoReturnValue)) // Used for Methods that have no response and Property Set members
                    {
                        MethodResponse deviceResponse = JsonConvert.DeserializeObject<MethodResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, deviceResponse.ClientTransactionID.ToString(), deviceResponse.ServerTransactionID.ToString(), "No response", deviceResponse.Method));
                        if (deviceResponse.DriverException == null) return (T)((object)new NoReturnValue());
                        restResponseBase = (RestResponseBase)deviceResponse;
                    }

                    // DEVICE SPECIFIC TYPES
                    if (typeof(T) == typeof(PierSide))
                    {
                        IntResponse pierSideResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, pierSideResponse.ClientTransactionID, pierSideResponse.ServerTransactionID, pierSideResponse.Value.ToString(), pierSideResponse.Method));
                        if (pierSideResponse.DriverException == null) return (T)((object)pierSideResponse.Value);
                        restResponseBase = (RestResponseBase)pierSideResponse;
                    }
                    if (typeof(T) == typeof(ITrackingRates))
                    {
                        TrackingRatesResponse trackingRatesResponse = JsonConvert.DeserializeObject<TrackingRatesResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format("Trackingrates Count: {0} - Txn ID: {1}, Method: {2}", trackingRatesResponse.Rates.Count.ToString(), trackingRatesResponse.ServerTransactionID.ToString(), trackingRatesResponse.Method));
                        List<DriveRates> rates = new List<DriveRates>();
                        DriveRates[] ratesArray = new DriveRates[trackingRatesResponse.Rates.Count];
                        int i = 0;
                        foreach (DriveRates rate in trackingRatesResponse.Rates)
                        {
                            TL.LogMessage(clientNumber, method, string.Format("Rate: {0}", rate.ToString()));
                            ratesArray[i] = rate;
                            i++;
                        }
                        TrackingRates trackingRates = new TrackingRates();
                        trackingRates.SetRates(ratesArray);
                        if (trackingRatesResponse.DriverException == null)
                        {
                            TL.LogMessage(clientNumber, method, string.Format("Returning {0} tracking rates to the client - now measured from trackingRates", trackingRates.Count));
                            return (T)((object)trackingRates);
                        }
                        TL.LogMessage(clientNumber, method, "trackingRatesResponse.DriverException is NOT NULL!");
                        restResponseBase = (RestResponseBase)trackingRatesResponse;
                    }
                    if (typeof(T) == typeof(EquatorialCoordinateType))
                    {
                        IntResponse equatorialCoordinateResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, equatorialCoordinateResponse.ClientTransactionID, equatorialCoordinateResponse.ServerTransactionID, equatorialCoordinateResponse.Value.ToString(), equatorialCoordinateResponse.Method));
                        if (equatorialCoordinateResponse.DriverException == null) return (T)((object)equatorialCoordinateResponse.Value);
                        restResponseBase = (RestResponseBase)equatorialCoordinateResponse;
                    }
                    if (typeof(T) == typeof(AlignmentModes))
                    {
                        IntResponse alignmentModesResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, alignmentModesResponse.ClientTransactionID, alignmentModesResponse.ServerTransactionID, alignmentModesResponse.Value.ToString(), alignmentModesResponse.Method));
                        if (alignmentModesResponse.DriverException == null) return (T)((object)alignmentModesResponse.Value);
                        restResponseBase = (RestResponseBase)alignmentModesResponse;
                    }
                    if (typeof(T) == typeof(DriveRates))
                    {
                        IntResponse driveRatesResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, driveRatesResponse.ClientTransactionID, driveRatesResponse.ServerTransactionID, driveRatesResponse.Value.ToString(), driveRatesResponse.Method));
                        if (driveRatesResponse.DriverException == null) return (T)((object)driveRatesResponse.Value);
                        restResponseBase = (RestResponseBase)driveRatesResponse;
                    }
                    if (typeof(T) == typeof(SensorType))
                    {
                        IntResponse sensorTypeResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, sensorTypeResponse.ClientTransactionID, sensorTypeResponse.ServerTransactionID, sensorTypeResponse.Value.ToString(), sensorTypeResponse.Method));
                        if (sensorTypeResponse.DriverException == null) return (T)((object)sensorTypeResponse.Value);
                        restResponseBase = (RestResponseBase)sensorTypeResponse;
                    }
                    if (typeof(T) == typeof(CameraStates))
                    {
                        IntResponse cameraStatesResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, cameraStatesResponse.ClientTransactionID, cameraStatesResponse.ServerTransactionID, cameraStatesResponse.Value.ToString(), cameraStatesResponse.Method));
                        if (cameraStatesResponse.DriverException == null) return (T)((object)cameraStatesResponse.Value);
                        restResponseBase = (RestResponseBase)cameraStatesResponse;
                    }
                    if (typeof(T) == typeof(ShutterState))
                    {
                        IntResponse domeShutterStateResponse = JsonConvert.DeserializeObject<IntResponse>(response.Content);
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, domeShutterStateResponse.ClientTransactionID, domeShutterStateResponse.ServerTransactionID, domeShutterStateResponse.Value.ToString(), domeShutterStateResponse.Method));
                        if (domeShutterStateResponse.DriverException == null) return (T)((object)domeShutterStateResponse.Value);
                        restResponseBase = (RestResponseBase)domeShutterStateResponse;
                    }
                    if (typeof(T) == typeof(IAxisRates))
                    {
                        AxisRatesResponse axisRatesResponse = JsonConvert.DeserializeObject<AxisRatesResponse>(response.Content);
                        AxisRates axisRates = new AxisRates((TelescopeAxes)(Convert.ToInt32(Parameters[SharedConstants.AXIS_PARAMETER_NAME])));
                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, axisRatesResponse.ClientTransactionID.ToString(), axisRatesResponse.ServerTransactionID.ToString(), axisRatesResponse.Value.Count.ToString(), axisRatesResponse.Method));
                        foreach (RateResponse rr in axisRatesResponse.Value)
                        {
                            axisRates.Add(rr.Minimum, rr.Maximum, TL);
                            TL.LogMessage(clientNumber, method, string.Format("Found rate: {0} - {1}", rr.Minimum, rr.Maximum));
                        }

                        if (axisRatesResponse.DriverException == null) return (T)((object)axisRates);
                        restResponseBase = (RestResponseBase)axisRatesResponse;
                    }
                    if (typeof(T) == typeof(Array)) // Used for Camera.ImageArray and Camera.ImageArrayVariant
                    {
                        // Parse the first 30 characters of the returned JSON to extract the array type and rank
                        TL.LogMessage(clientNumber, method, "Received type is Array");
                        string responseString = response.Content.Substring(0, 30);
                        TL.LogMessage(clientNumber, method, "Response SubString" + responseString);

                        TL.LogMessage(clientNumber, method, "Before Regex.Matches");
                        MatchCollection matches = Regex.Matches(responseString, FIND_TYPE_AND_RANK_REGEX_PATTERN, RegexOptions.IgnoreCase);
                        TL.LogMessage(clientNumber, method, "After Regex.Matches");
                        string arrayTypeString = matches[0].Groups[ARRAYTYPE_VARIABLE_NAME].Value;
                        string arrayRankString = matches[0].Groups[RANK_VARIABLE_NAME].Value;
                        TL.LogMessage(clientNumber, method, string.Format("Array Type String: '{0}', Array Rank String: '{1}', Response String(30 characters): '{2}'", arrayTypeString, arrayRankString, responseString));

                        SharedConstants.ImageArrayElementTypes arrayType = (SharedConstants.ImageArrayElementTypes)Enum.Parse(typeof(SharedConstants.ImageArrayElementTypes), arrayTypeString);
                        int arrayRank = Convert.ToInt32(arrayRankString);
                        TL.LogMessage(clientNumber, method, string.Format("String values - Type: {0}, Rank: {1}, Actual values - Type: {2}, Rank: {3}", arrayTypeString, arrayRankString, arrayType.ToString(), arrayRank.ToString()));

                        switch (arrayType) // Handle the different return types that may come from ImageArrayVariant
                        {
                            case SharedConstants.ImageArrayElementTypes.Int:
                                switch (arrayRank)
                                {
                                    case 2:
                                        IntArray2DResponse intArray2DResponse = JsonConvert.DeserializeObject<IntArray2DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, intArray2DResponse.ClientTransactionID, intArray2DResponse.ServerTransactionID, intArray2DResponse.Rank.ToString(), intArray2DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)intArray2DResponse.Type).ToString(), intArray2DResponse.Rank));
                                        if (intArray2DResponse.DriverException == null) return (T)((object)intArray2DResponse.Value);
                                        restResponseBase = (RestResponseBase)intArray2DResponse;
                                        break;

                                    case 3:
                                        IntArray3DResponse intArray3DResponse = JsonConvert.DeserializeObject<IntArray3DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, intArray3DResponse.ClientTransactionID, intArray3DResponse.ServerTransactionID, intArray3DResponse.Rank.ToString(), intArray3DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)intArray3DResponse.Type).ToString(), intArray3DResponse.Rank));
                                        if (intArray3DResponse.DriverException == null) return (T)((object)intArray3DResponse.Value);
                                        restResponseBase = (RestResponseBase)intArray3DResponse;
                                        break;

                                    default:
                                        throw new InvalidOperationException("Arrays of Rank " + arrayRank + " are not supported.");
                                }
                                break;

                            case SharedConstants.ImageArrayElementTypes.Short:
                                switch (arrayRank)
                                {
                                    case 2:
                                        ShortArray2DResponse shortArray2DResponse = JsonConvert.DeserializeObject<ShortArray2DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, shortArray2DResponse.ClientTransactionID, shortArray2DResponse.ServerTransactionID, shortArray2DResponse.Rank.ToString(), shortArray2DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)shortArray2DResponse.Type).ToString(), shortArray2DResponse.Rank));
                                        if (shortArray2DResponse.DriverException == null) return (T)((object)shortArray2DResponse.Value);
                                        restResponseBase = (RestResponseBase)shortArray2DResponse;
                                        break;

                                    case 3:
                                        ShortArray3DResponse shortArray3DResponse = JsonConvert.DeserializeObject<ShortArray3DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, shortArray3DResponse.ClientTransactionID, shortArray3DResponse.ServerTransactionID, shortArray3DResponse.Rank.ToString(), shortArray3DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)shortArray3DResponse.Type).ToString(), shortArray3DResponse.Rank));
                                        if (shortArray3DResponse.DriverException == null) return (T)((object)shortArray3DResponse.Value);
                                        restResponseBase = (RestResponseBase)shortArray3DResponse;
                                        break;

                                    default:
                                        throw new InvalidOperationException("Arrays of Rank " + arrayRank + " are not supported.");
                                }
                                break;

                            case SharedConstants.ImageArrayElementTypes.Double:
                                switch (arrayRank)
                                {
                                    case 2:
                                        DoubleArray2DResponse doubleArray2DResponse = JsonConvert.DeserializeObject<DoubleArray2DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, doubleArray2DResponse.ClientTransactionID, doubleArray2DResponse.ServerTransactionID, doubleArray2DResponse.Rank.ToString(), doubleArray2DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)doubleArray2DResponse.Type).ToString(), doubleArray2DResponse.Rank));
                                        if (doubleArray2DResponse.DriverException == null) return (T)((object)doubleArray2DResponse.Value);
                                        restResponseBase = (RestResponseBase)doubleArray2DResponse;
                                        break;

                                    case 3:
                                        DoubleArray3DResponse doubleArray3DResponse = JsonConvert.DeserializeObject<DoubleArray3DResponse>(response.Content);
                                        TL.LogMessage(clientNumber, method, string.Format(LOG_FORMAT_STRING, doubleArray3DResponse.ClientTransactionID, doubleArray3DResponse.ServerTransactionID, doubleArray3DResponse.Rank.ToString(), doubleArray3DResponse.Method));
                                        TL.LogMessage(clientNumber, method, string.Format("Returned array type is: {0} of Rank: {1}", ((SharedConstants.ImageArrayElementTypes)doubleArray3DResponse.Type).ToString(), doubleArray3DResponse.Rank));
                                        if (doubleArray3DResponse.DriverException == null) return (T)((object)doubleArray3DResponse.Value);
                                        restResponseBase = (RestResponseBase)doubleArray3DResponse;
                                        break;

                                    default:
                                        throw new InvalidOperationException("Arrays of Rank " + arrayRank + " are not supported.");
                                }
                                break;

                            default:
                                throw new InvalidOperationException("Image array element type" + arrayType + " is not supported.");

                        }
                    }

                    // Handle exceptions received from the driver by the remote server
                    if (restResponseBase.DriverException != null)
                    {
                        TL.LogMessageCrLf(clientNumber, method, string.Format("Exception Message: {0}, Exception Number: 0x{1}", restResponseBase.ErrorMessage, restResponseBase.ErrorNumber.ToString("X8")));
                        throw restResponseBase.DriverException;
                    }

                    // Internal error if an unsuported type is requested 
                    throw new InvalidOperationException("Type " + typeof(T).ToString() + " is not supported.");
                }
                else
                {
                    if (response.ErrorException != null)
                    {
                        TL.LogMessageCrLf(clientNumber, method, "RestClient exception: " + response.ErrorMessage + "\r\n " + response.ErrorException.ToString());
                        throw new ASCOM.DriverException(string.Format("Communications exception: {0} - {1}", response.ErrorMessage, response.ResponseStatus), response.ErrorException);
                    }
                    else
                    {
                        TL.LogMessage(clientNumber, method + " Error", string.Format("RestRequest response status: {0}, HTTP response code: {1}, HTTP response description: {2}", response.ResponseStatus.ToString(), response.StatusCode, response.StatusDescription));
                        throw new ASCOM.DriverException(method + " Error - Status: " + response.ResponseStatus + " " + response.StatusDescription);
                    }
                }
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf(clientNumber, method, typeof(T).Name + " " + ex.Message);
                if (TL.DebugTraceState) TL.LogMessageCrLf(clientNumber, method, "Exception: " + ex.ToString());
                throw;
            }
        }

        #endregion

        #region ASCOM Common members

        public static string Action(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string actionName, string actionParameters)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.ACTION_COMMAND_PARAMETER_NAME, actionName },
                { SharedConstants.ACTION_PARAMETERS_PARAMETER_NAME, actionParameters }
            };
            string remoteString = SendToRemoteDriver<string>(clientNumber, client, URIBase, TL, "Action", Parameters, Method.PUT);

            TL.LogMessage(clientNumber, "Action", "Response: " + remoteString);
            return remoteString;
        }

        public static void CommandBlind(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string command, bool raw)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.COMMAND_PARAMETER_NAME, command },
                { SharedConstants.RAW_PARAMETER_NAME, raw.ToString() }
            };
            SendToRemoteDriver<NoReturnValue>(clientNumber, client, URIBase, TL, "CommandBlind", Parameters, Method.PUT);
            TL.LogMessage(clientNumber, "CommandBlind", "Completed OK");
        }

        public static bool CommandBool(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string command, bool raw)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.COMMAND_PARAMETER_NAME, command },
                { SharedConstants.RAW_PARAMETER_NAME, raw.ToString() }
            };
            bool remoteBool = SendToRemoteDriver<bool>(clientNumber, client, URIBase, TL, "CommandBool", Parameters, Method.PUT);

            TL.LogMessage(clientNumber, "CommandBool", remoteBool.ToString());
            return remoteBool;
        }

        public static string CommandString(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL, string command, bool raw)
        {
            Dictionary<string, string> Parameters = new Dictionary<string, string>
            {
                { SharedConstants.COMMAND_PARAMETER_NAME, command },
                { SharedConstants.RAW_PARAMETER_NAME, raw.ToString() }
            };
            string remoteString = SendToRemoteDriver<string>(clientNumber, client, URIBase, TL, "CommandString", Parameters, Method.PUT);

            TL.LogMessage(clientNumber, "CommandString", remoteString);
            return remoteString;
        }

        public static void Connect(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            if (debugTraceState) TL.LogMessage(clientNumber, "Connect", "Acquiring connection lock");
            lock (connectLockObject) // Ensure that only one connection attempt can happen at a time
            {
                TL.LogMessage(clientNumber, "Connect", "Has connection lock");
                if (IsClientConnected(clientNumber, client, TL)) // If we are already connected then just log this 
                {
                    TL.LogMessage(clientNumber, "Connect", "Already connected, just incrementing connection count.");
                }
                else // We are not connected so connect now
                {
                    try
                    {
                        TL.LogMessage(clientNumber, "Connect", "Attempting to connect to devices");
                        bool notAlreadyPresent = connectStates.TryAdd(clientNumber, true);
                        TL.LogMessage(clientNumber, "Connect", "Successfully connected, AlreadyConnected: " + (!notAlreadyPresent).ToString() + ", number of connections: " + connectStates.Count);
                        if (ConnectionCount(TL) == 1) // This is the first successful connection
                        {
                            TL.LogMessage(clientNumber, "Connect", "This is the first connection so set Connected to True");
                            SetBool(clientNumber, client, URIBase, TL, "Connected", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        TL.LogMessageCrLf(clientNumber, "Connect", "Exception: " + ex.ToString());
                        throw;
                    }
                }
            }
        }

        public static string Description(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            return GetValue<string>(clientNumber, client, URIBase, TL, "Description");
        }

        public static void Disconnect(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            bool successfullyRemoved = connectStates.TryRemove(clientNumber, out bool lastValue);
            TL.LogMessage("Disconnect", "Set Connected to: False, Successfully removed: " + successfullyRemoved.ToString());

            if (ConnectionCount(TL) == 0) // The last connection has dropped so really disconnect
            {
                TL.LogMessage(clientNumber, "Disconnect", "This is the last connection so set Connected to False");
                SetBool(clientNumber, client, URIBase, TL, "Connected", false);
            }

        }

        public static string DriverInfo(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            return GetValue<string>(clientNumber, client, URIBase, TL, "DriverInfo");
        }

        public static string DriverVersion(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            string remoteString = GetValue<string>(clientNumber, client, URIBase, TL, "DriverVersion");
            TL.LogMessage(clientNumber, "DriverVersion", remoteString);
            return remoteString;
        }

        public static short InterfaceVersion(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            short interfaceVersion = GetValue<short>(clientNumber, client, URIBase, TL, "InterfaceVersion");
            TL.LogMessage(clientNumber, "InterfaceVersion", interfaceVersion.ToString());
            return interfaceVersion;
        }

        public static ArrayList SupportedActions(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            List<string> supportedActions = GetValue<List<string>>(clientNumber, client, URIBase, TL, "SupportedActions");
            TL.LogMessage(clientNumber, "SupportedActions", string.Format("Returning {0} actions", supportedActions.Count));

            ArrayList returnValues = new ArrayList();
            foreach (string action in supportedActions)
            {
                returnValues.Add(action);
                TL.LogMessage(clientNumber, "SupportedActions", string.Format("Returning action: {0}", action));
            }

            return returnValues;
        }

        #endregion

        #region Complex Camera Properties

        public static object ImageArrayVariant(int clientNumber, RestClient client, string URIBase, TraceLoggerPlus TL)
        {
            Array returnArray;
            string variantType = null;
            object[,] objectArray2D = new object[1, 1];
            object[,,] objectArray3D = new object[1, 1, 1];

            returnArray = GetValue<Array>(clientNumber, client, URIBase, TL, "ImageArrayVariant");

            variantType = returnArray.GetType().Name;
            TL.LogMessage(clientNumber, "ImageArrayVariant", string.Format("Received array of Rank {0} of Length {1} with element type {2} {3}", returnArray.Rank, returnArray.Length, returnArray.GetType().Name, returnArray.GetType().UnderlyingSystemType.Name));

            // convert to variant

            switch (returnArray.Rank)
            {
                case 2:
                    switch (variantType)
                    {
                        case "Int16[,]":
                            objectArray2D = new object[returnArray.GetLength(0), returnArray.GetLength(1)];
                            for (int i = 0; i < returnArray.GetLength(1); i++)
                            {
                                for (int j = 0; j < returnArray.GetLength(0); j++)
                                {
                                    objectArray2D[j, i] = ((short[,])returnArray)[j, i];
                                }
                            }
                            return objectArray2D;
                        case "Int32[,]":
                            objectArray2D = new object[returnArray.GetLength(0), returnArray.GetLength(1)];

                            for (int i = 0; i < returnArray.GetLength(1); i++)
                            {
                                for (int j = 0; j < returnArray.GetLength(0); j++)
                                {
                                    objectArray2D[j, i] = ((int[,])returnArray)[j, i];
                                }
                            }
                            return objectArray2D;
                        case "Double[,]":
                            objectArray2D = new object[returnArray.GetLength(0), returnArray.GetLength(1)];
                            for (int i = 0; i < returnArray.GetLength(1); i++)
                            {
                                for (int j = 0; j < returnArray.GetLength(0); j++)
                                {
                                    objectArray2D[j, i] = ((double[,])returnArray)[j, i];
                                }
                            }
                            return objectArray2D;
                        default:
                            throw new InvalidValueException("Remote Driver Camera.ImageArrayVariant: Unsupported return array rank from RemoteClientDriver.GetValue<Array>: " + returnArray.Rank);
                    }
                case 3:
                    switch (variantType)
                    {
                        case "Int16[,,]":
                            objectArray3D = new object[returnArray.GetLength(0), returnArray.GetLength(1), 3];
                            for (int i = 0; i < returnArray.GetLength(1); i++)
                            {
                                for (int j = 0; j < returnArray.GetLength(0); j++)
                                {
                                    for (int k = 0; k < 3; k++)
                                        objectArray3D[j, i, k] = ((short[,,])returnArray)[j, i, k];
                                }
                            }
                            return objectArray3D;

                        case "Int32[,,]":
                            objectArray3D = new object[returnArray.GetLength(0), returnArray.GetLength(1), returnArray.GetLength(2)];
                            for (int k = 0; k < returnArray.GetLength(2); k++)
                            {
                                for (int j = 0; j < returnArray.GetLength(1); j++)
                                {
                                    for (int i = 0; i < returnArray.GetLength(0); i++)
                                    {
                                        objectArray3D[i, j, k] = ((int[,,])returnArray)[i, j, k];
                                    }
                                }
                            }
                            return objectArray3D;

                        case "Double[,,]":
                            objectArray3D = new object[returnArray.GetLength(0), returnArray.GetLength(1), 3];
                            for (int i = 0; i < returnArray.GetLength(1); i++)
                            {
                                for (int j = 0; j < returnArray.GetLength(0); j++)
                                {
                                    for (int k = 0; k < 3; k++)
                                        objectArray3D[j, i, k] = ((double[,,])returnArray)[j, i, k];
                                }
                            }
                            return objectArray3D;
                        default:
                            throw new InvalidValueException("Remote Driver Camera.ImageArrayVariant: Unsupported return array rank from RemoteClientDriver.GetValue<Array>: " + returnArray.Rank);
                    }

                default:
                    throw new InvalidValueException("Remote Driver Camera.ImageArrayVariant: Unsupported return array rank from RemoteClientDriver.GetValue<Array>: " + returnArray.Rank);
            }
        }

        #endregion

    }
}
