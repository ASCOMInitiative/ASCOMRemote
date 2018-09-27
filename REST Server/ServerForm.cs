using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Drawing;

using Newtonsoft.Json;
using System.Web;
using System.Text.RegularExpressions;

namespace ASCOM.Remote
{

    public partial class ServerForm : Form
    {
        #region Constants

        private const string SERVER_TRACELOGGER_NAME = "RemoteAccessServer";
        private const string ACCESSLOG_TRACELOGGER_NAME = "ServerAccessLog";

        //Relative path of the SetNetworkPermissions exe from C:\Program Files (or x86 flavour on a 64bit OS). This must match the location where the installer puts the exe!
        private const string SET_NETWORK_PERMISSIONS_EXE_PATH = @"\ASCOM\Remote\ASCOM.SetNetworkPermissions.exe";

        private const int MAX_ERRORS_BEFORE_CLOSE = 10; // Maximum number of async listen errors before the application permanently shuts down
        private const int SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH = 500; // Maximum length of a message that can be logged to the screen - required to prevent huge messages from locking up the system while the text box attempts to processes them
        private const int SCREEN_LOG_MAXIMUM_LENGTH = 50000; // Maximum length of the screen log - The screen log will be pruned to ensure it never exceeds this length, which would start to degrade performance

        private const string CORRECT_API_FORMAT_STRING = "<br>Required format is: <b>" +
                            "<font color=\"red\">API/V</font>" +
                            "<font color=\"blue\">x</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">DeviceType</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">y</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">Method</font>" +
                            "</b> where x is the one based API version number and y is the zero based number of the device.";
        private const string CORRECT_SERVER_FORMAT_STRING = "<br>Required format is: <b>" +
                            "<font color=\"red\">SERVER/V</font>" +
                            "<font color=\"blue\">x</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">CONFIGURATION | PROFILE</font>" +
                            "</b> where x is the one based API version number.";
        private const string GET_UNKNOWN_METHOD_MESSAGE = "GET - Unknown device method: ";
        private const string PUT_UNKNOWN_METHOD_MESSAGE = "PUT - Unknown device method: ";
        private const string MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE = "The management interface is not enabled, please enable it using the remote access server configuration dialogue";
        private const string API_INTERFACE_NOT_ENABLED_MESSAGE = "Access to the device server API is not currently available, please ask the owner to enable it.";
        private const char FORWARD_SLASH = '/'; // Forward slash character as a char value
        private const string X_FORWARDED_FOR = "X-Forwarded-For";

        // Position of each element within the client's requested URI 
        private const int URL_ELEMENT_API = 0; // For /api/ URIs
        private const int URL_ELEMENT_API_VERSION = 1;
        private const int URL_ELEMENT_DEVICE_TYPE = 2;
        private const int URL_ELEMENT_DEVICE_NUMBER = 3;
        private const int URL_ELEMENT_METHOD = 4;
        private const int URL_ELEMENT_SERVER_COMMAND = 2; // For /server/ type uris

        // Device server profile persistence constants
        internal const string SERVER_ACCESS_LOG_PROFILENAME = "Server Access Log Enabled"; public const bool SERVER_ACCESS_LOG_DEFAULT = true;
        internal const string SERVER_TRACE_LEVEL_PROFILENAME = "Server Trace Level"; public const bool SERVER_TRACE_LEVEL_DEFAULT = true;
        internal const string SERVER_DEBUG_TRACE_PROFILENAME = "Server Include Debug Trace"; public const bool SERVER_DEBUG_TRACE_DEFAULT = true;
        internal const string SERVER_IPADDRESS_PROFILENAME = "Server IP Address"; public const string SERVER_IPADDRESS_DEFAULT = SharedConstants.LOCALHOST_ADDRESS;
        internal const string SERVER_PORTNUMBER_PROFILENAME = "Server Port Number"; public const decimal SERVER_PORTNUMBER_DEFAULT = 11111;
        internal const string SERVER_AUTOCONNECT_PROFILENAME = "Server Auto Connect"; public const bool SERVER_AUTOCONNECT_DEFAULT = true;
        internal const string SCREEN_LOG_REQUESTS_PROFILENAME = "Log Requests To Screen"; public const bool SCREEN_LOG_REQUESTS_DEFAULT = true;
        internal const string SCREEN_LOG_RESPONSES_PROFILENAME = "Log Responses To Screen"; public const bool SCREEN_LOG_RESPONSES_DEFAULT = false;
        internal const string ALLOW_CONNECTED_SET_FALSE_PROFILENAME = "Allow Connected Set False"; public const bool ALLOW_CONNECTED_SET_FALSE_DEFAULT = true;
        internal const string ALLOW_CONNECTED_SET_TRUE_PROFILENAME = "Allow Connected Set True"; public const bool ALLOW_CONNECTED_SET_TRUE_DEFAULT = true;
        internal const string MANAGEMENT_INTERFACE_ENABLED_PROFILENAME = "Management Interface Enabled"; public const bool MANGEMENT_INTERFACE_ENABLED_DEFAULT = false;
        internal const string START_WITH_API_ENABLED_PROFILENAME = "Start WIth API Enabled"; public const bool START_WITH_API_ENABLED_DEFAULT = true;
        internal const string RUN_DRIVERS_ON_SEPARATE_THREADS_PROFILENAME = "Run Drivers On Separate Threads"; public const bool RUN_DRIVERS_ON_SEPARATE_THREADS_DEFAULT = true;

        //Device profile persistence constants
        internal const string DEVICE_SUBFOLDER_NAME = "Device";
        internal const string DEVICETYPE_PROFILENAME = "Device Type"; public const string DEVICETYPE_DEFAULT = SharedConstants.DEVICE_NOT_CONFIGURED;
        internal const string PROGID_PROFILENAME = "ProgID"; public const string PROGID_DEFAULT = SharedConstants.DEVICE_NOT_CONFIGURED;
        internal const string DESCRIPTION_PROFILENAME = "Description"; public const string DESCRIPTION_DEFAULT = "";
        internal const string DEVICENUMBER_PROFILENAME = "Device Number"; public const int DEVICENUMBER_DEFAULT = 0;

        // !!!!! This list must match the names of the ServedDevice instances on the SetupForm !!!!!
        internal static ConcurrentBag<string> ServerDeviceNames = new ConcurrentBag<string>() { "ServedDevice0", "ServedDevice1", "ServedDevice2", "ServedDevice3", "ServedDevice4", "ServedDevice5", "ServedDevice6", "ServedDevice7", "ServedDevice8", "ServedDevice9" };
        internal static List<string> ServerDeviceNumbers = new List<string>() { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        // Form size and resize constants
        internal const int FORM_MINIMUM_WIDTH = 400; // Server form minimum width
        internal const int FORM_MINIMUM_HEIGHT = 350; // Server form minimum height
        internal const int TITLE_OFFSET_FROM_TOP = 22; // Offset of the title from the top of the form
        internal const int TITLE_TRANSITION_POSITION_END = 900; // Form width above which the title position is always centred over the message list
        internal const int LOG_HEIGHT_OFFSET = 123; // Offset from the height of the form so that the log text box just fits within the form when resized
        internal const int CONTROL_OVERALL_HEIGHT = 103; // Overall height of all control groups
        internal const int CONTROL_SPACING_MAXIMUM = 45; // Maximum separation between control groups
        internal const int CONTROL_SPACE_WIDTH = 240; // Size of the free space, to the right of the log messages text box, that must be left clear for server controls
        internal const int CONTROL_LEFT_OFFSET = 202; // Offset from the width of the form to the start of a full sized control
        internal const int CONTROL_CENTRE_OFFSET = 32; // Offset from the CONTROL_LEFT_OFFSET to the start of a centred control

        // The server form displays controls in several groups:
        // Control Group 1 - Concurrent transactions counter
        // Control Group 2 - Log requests and responses check boxes
        // Control Group 3 - REST Server Status panel plus the Stop and Start buttons
        // Control Group 4 - Device Status panel plus the Disconnect and Connect buttons
        // Control Group 5 - Setup button
        // Control Group 6 - Exit button
        internal const int NUMBER_OF_CONTROL_GROUPS = 6; // Number of control groups 

        // Management interface constants
        internal const int MANGEMENT_RESTART_WAIT_TIME = 5000;

        // Run driver in separate thread constants
        internal const int DESTROY_DRIVER = int.MinValue;
        internal const int ASYNC_WAIT_LOOP_TIME = 20; // Number of milliseconds to wait for work before timing out and going round the wait loop to run Application.DoEvents()

        #endregion

        #region Application Global Variables

        // These are available anywhere in the Remote Device Server and have the same value in all threads

        internal static HttpListener httpListener;
        internal static TraceLoggerPlus AccessLog;
        internal readonly object counterLock = new object();
        internal readonly object managementCommandLock = new object();

        // Application Status variables
        internal static bool apiIsEnabled = false;
        internal static bool devicesAreConnected = false;

        internal static int serverTransactionIDCounter = 0; // Internal variable used to keep track of the current server transaction ID
        internal static int numberOfConcurrentTransactions = 0; // Internal variable to keep track of the number of concurrent transactions
        internal static int numberOfConsecutiveErrors = 0; // Counter to record the number of consecutive errors, this is reset to zero whenever a successful async listen occurs

        // Variable to hold the last log time so that old logs are closed and new logs are started when we move to a new day
        internal static DateTime LastTraceLogTime = DateTime.Now; // Initialise to now to ensure that the TraceLoggerPlus code works correctly
        internal static DateTime LastAccessLogTime = DateTime.Now; // Initialise to now to ensure that the TraceLoggerPlus code works correctly

        internal static ConcurrentDictionary<string, ConfiguredDevice> ConfiguredDevices;
        internal static ConcurrentDictionary<string, ActiveObject> ActiveObjects;

        // Configuration variables that can be changed through the Setup dialogue 
        internal static bool TraceState;
        internal static bool DebugTraceState;
        internal static string ServerIPAddressString;
        internal static decimal ServerPortNumber;
        internal static bool StartWithDevicesConnected;
        internal static bool AccessLogEnabled;
        internal static bool ScreenLogRequests;
        internal static bool ScreenLogResponses;
        internal static bool ManagementInterfaceEnabled;
        internal static bool StartWithApiEnabled;
        internal static bool RunDriversOnSeparateThreads;

        #endregion

        #region Thread Global Variables

        // These are global within a Remote Device Server thread but will be different between threads

        [ThreadStatic]
        internal static dynamic device; // Shortcut to the device referenced by the incoming URI
        [ThreadStatic]
        internal static bool allowConnectedSetFalse; // Shortcut to a flag indicating whether Connected can be set False
        [ThreadStatic]
        internal static bool allowConnectedSetTrue; // Shortcut to a flag indicating whether Connected can be set True

        #endregion

        #region Private Variables
        private static readonly object logLockObject = new object(); // Lock object to ensure that midnight log change overs happen smoothly
        private static TraceLoggerPlus TL; // Variable to hold the trace logger
        #endregion

        #region Delegates for Form callbacks

        private delegate void SetTextCallback(string text);
        private delegate void SetConcurrencyCallback();
        private delegate void DestroyDriverDelegate();
        private delegate void DriverCommandDelegate(RequestData requestData);
        private delegate void CreateInstanceDelegate(KeyValuePair<string, ConfiguredDevice> kvp);

        #endregion

        #region Initialisation, Form Load and Close

        public ServerForm()
        {
            try
            {
                InitializeComponent();
                TL = new TraceLoggerPlus("", SERVER_TRACELOGGER_NAME);
                ConfiguredDevices = new ConcurrentDictionary<string, ConfiguredDevice>();
                ActiveObjects = new ConcurrentDictionary<string, ActiveObject>();

                ReadProfile();

                TL.Enabled = TraceState; // Initialise with the trace state enabled or disabled as configured

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                LogMessage(0, 0, 0, "New", string.Format("Remote Server Version {0}, Started on {1}", version.ToString(), DateTime.Now.ToString("dddd d MMMM yyyy HH:mm:ss")));

                AccessLog = new TraceLoggerPlus("", ACCESSLOG_TRACELOGGER_NAME)
                {
                    Enabled = AccessLogEnabled
                };

                LogMessage(0, 0, 0, "New", "Setting screen log check boxes"); // Must be done before enabling event handlers!
                chkLogRequests.Checked = ScreenLogRequests;
                chkLogResponses.Checked = ScreenLogResponses;

                LogMessage(0, 0, 0, "New", "Enabling screen log check box event handlers");
                chkLogRequests.CheckedChanged += ChkLogRequestsAndResponses_CheckedChanged;
                chkLogResponses.CheckedChanged += ChkLogRequestsAndResponses_CheckedChanged;

                LogMessage(0, 0, 0, "New", "Form configuration and enabling form event handlers");
                this.MinimumSize = new Size(FORM_MINIMUM_WIDTH, FORM_MINIMUM_HEIGHT); // Set the form's minimum size
                this.FormClosed += Form1_FormClosed;
                this.Resize += ServerForm_Resize;
                ServerForm_Resize(this, new EventArgs()); // Move controls to their correct positions

                LogMessage(0, 0, 0, "New", "Assigning label parents to picture boxes");
                LbDriverStatus.Parent = PboxDriverStatus;
                LbDriverStatus.Location = new Point(0, 0);
                LblRESTStatus.Parent = PboxRESTStatus;
                LblRESTStatus.Location = new Point(0, 0);


                LogMessage(0, 0, 0, "New", "Initialisation complete");
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "New", ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        private void ServerForm_Load(object sender, EventArgs e)
        {
            this.Text = "ASCOM REST Server - v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (StartWithDevicesConnected)
            {
                ConnectDevices();
            }

            if (StartWithApiEnabled)
            {
                StartRESTServer();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Clear down the listener
            LogMessage(0, 0, 0, "FormClosed", string.Format("Stopping REST server on thread {0}", Thread.CurrentThread.ManagedThreadId));
            StopRESTServer();
            LogMessage(0, 0, 0, "FormClosed", string.Format("Stopped REST server on thread {0}", Thread.CurrentThread.ManagedThreadId));

            // Clear all of the current objects
            LogMessage(0, 0, 0, "FormClosed", string.Format("Disconnecting devices on thread {0}", Thread.CurrentThread.ManagedThreadId));
            DisconnectDevices();
            LogMessage(0, 0, 0, "FormClosed", string.Format("Disconnected devices on thread {0}", Thread.CurrentThread.ManagedThreadId));

            LogMessage(0, 0, 0, "FormClosed", string.Format("Calling Application.Exit on thread {0}", Thread.CurrentThread.ManagedThreadId));

            //Environment.Exit(0);
            Application.Exit();
            LogMessage(0, 0, 0, "FormClosed", string.Format("After Application.Exit on thread {0}", Thread.CurrentThread.ManagedThreadId));
            //Thread.Sleep(100);
            WaitFor(200);
            LogMessage(0, 0, 0, "FormClosed", string.Format("After Sleep on thread {0}", Thread.CurrentThread.ManagedThreadId));
        }

        #endregion

        #region Utility methods

        private int GetServerTransactionID()
        {
            // Increment the server transaction number in a thread safe way
            lock (counterLock)
            {
                // Ensure that the server transaction number wraps round gracefully when it gets to its maximum value
                if (serverTransactionIDCounter < int.MaxValue) serverTransactionIDCounter += 1;
                else serverTransactionIDCounter = 0; // We need to wrap round so reset the counter to 0
            }
            return serverTransactionIDCounter;
        }

        public IPAddress GetIPAddress()
        {
            IPHostEntry host;
            IPAddress localIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            bool found = false;
            foreach (IPAddress ip in host.AddressList)
            {
                if ((ip.AddressFamily == AddressFamily.InterNetwork) & !found)
                {
                    localIP = ip;
                    LogMessage(0, 0, 0, "GetIPAddress", "Found IP Address: " + localIP.ToString());
                    found = true;
                }
                else
                {
                    LogMessage(0, 0, 0, "GetIPAddress", "Ignored IP Address: " + ip.ToString());
                }
            }
            if (localIP == null) throw new Exception("Cannot find IP address of this device");

            LogMessage(0, 0, 0, "GetIPAddress", localIP.ToString());
            return localIP;
        }

        private void StartRESTServer()
        {
            try
            {

                // Create variables to hold the ASCOM device server operating URIs
                string apiOperatingUri = string.Format(@"http://{0}:{1}{2}", ServerIPAddressString, ServerPortNumber, SharedConstants.API_URL_BASE);
                string managementUri = string.Format(@"http://{0}:{1}{2}", ServerIPAddressString, ServerPortNumber, SharedConstants.MANAGEMENT_URL_BASE);
                LogMessage(0, 0, 0, "StartRESTServer", "Operating URI: " + apiOperatingUri);
                LogMessage(0, 0, 0, "StartRESTServer", "Management URI: " + managementUri);

                // Create the listener on the required URIs
                LogMessage(0, 0, 0, "StartRESTServer", "Stopping existing server");
                StopRESTServer();

                LogMessage(0, 0, 0, "StartRESTServer", "Creating listener");
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(apiOperatingUri); // Set up the listener on the api URI
                httpListener.Prefixes.Add(managementUri); // Set up the listener on the management URI

                // Start the listener and ask permission if required
                while (!httpListener.IsListening)
                {
                    try
                    {
                        LogMessage(0, 0, 0, "StartRESTServer", "Starting listener");
                        httpListener.Start();
                        LogMessage(0, 0, 0, "StartRESTServer", "Listener started");
                    }
                    catch (HttpListenerException ex) when (ex.ErrorCode == (int)WindowsErrorCodes.ERROR_ACCESS_DENIED) // User does not have an ACL permitting this address and port to be used so get permission
                    {
                        DialogResult dlgResult = MessageBox.Show(string.Format("You need to give permission to listen on URL: {0}, do you wish to do this? \r\n(Requires administrator privilege)", apiOperatingUri), "Access Denied", MessageBoxButtons.YesNo);
                        if (dlgResult == DialogResult.Yes) // Permission given so set the ACL using net-sh, which will ask for elevation if required
                        {
                            LogMessage(0, 0, 0, "StartRESTServer", "User gave permission to set port ACL");
                            LogMessage(0, 0, 0, "StartRESTServer", "Closing listener"); // Have to close listener before setting ACL
                            httpListener.Close();
                            httpListener = null;
                            LogMessage(0, 0, 0, "StartRESTServer", "Enabling URIs");

                            string apiNetshCommand = string.Format(@"http add urlacl url={0} user={1}\{2}""", apiOperatingUri, Environment.UserDomainName, Environment.UserName);
                            LogMessage(0, 0, 0, "StartRESTServer", string.Format("API NetSh command: {0}", apiNetshCommand));

                            string managementNetshCommand = string.Format(@"""http add urlacl url={0} user={1}\{2}""", managementUri, Environment.UserDomainName, Environment.UserName);
                            LogMessage(0, 0, 0, "StartRESTServer", string.Format("Management NetSh command: {0}", managementNetshCommand));

                            try
                            {
                                string setNetworkPermissionsPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + SET_NETWORK_PERMISSIONS_EXE_PATH;
                                LogMessage(0, 0, 0, "StartRESTServer", string.Format("SetNetworkPermissionspath: {0}", setNetworkPermissionsPath));

                                // Check that the SetNetworkPermissions exe exists
                                if (File.Exists(setNetworkPermissionsPath)) // SetNetworkPermissions exists
                                {
                                    string args = string.Format(@"--setapiuriacl {0} --setmanagementuriacl {1}", apiOperatingUri, managementUri);
                                    LogMessage(0, 0, 0, "StartRESTServer", string.Format("SetNetworkPermissions arguments: {0}", args));

                                    ProcessStartInfo psi = new ProcessStartInfo(setNetworkPermissionsPath, args)
                                    {
                                        Verb = "runas",
                                        CreateNoWindow = true,
                                        WindowStyle = ProcessWindowStyle.Hidden,
                                        UseShellExecute = true
                                    };
                                    LogMessage(0, 0, 0, "StartRESTServer", "Starting SetNetworkPermissions process");
                                    Process.Start(psi).WaitForExit();
                                    LogMessage(0, 0, 0, "StartRESTServer", "Completed SetNetworkPermissions process");
                                }
                                else // SetNetworkPermissions does not exist
                                {
                                    string errorMessage = string.Format("Cannot find SetNetworkPermissions program: {0} ", setNetworkPermissionsPath);
                                    LogToScreen(errorMessage);
                                    LogMessage(0, 0, 0, "StartRESTServer", errorMessage);
                                    return;
                                }
                            }
                            catch (Exception ex1)
                            {
                                LogToScreen("Exception while enabling the API and Management URIs: " + ex1.Message);
                                LogException(0, 0, 0, "StartRESTServer", ex1.ToString());
                            }

                            // Create a new listener instance and loop round to attempt to start it again
                            LogMessage(0, 0, 0, "StartRESTServer", "Creating listener");
                            httpListener = new HttpListener(); // Set up the listener so that Start can be attempted again at the top of the while loop
                            httpListener.Prefixes.Add(apiOperatingUri); // Set up the listener on the required URI
                            httpListener.Prefixes.Add(managementUri); // Set up the listener on the management URI
                        }
                        else
                        {
                            LogMessage(0, 0, 0, "StartRESTServer", "User did NOT give permission to set port ACL");
                            return; // Just exit and wait for user to do something
                        }
                    }
                }

                apiIsEnabled = true;
                PboxRESTStatus.BackColor = Color.Green;
                LblRESTStatus.Text = "REST Server Up";

                LogMessage(0, 0, 0, "StartRESTServer", "Starting wait for incoming request");
                IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(RestRequestReceivedHandler), httpListener);

                //LogToScreen("Server started successfully.");
                LogMessage(0, 0, 0, "StartRESTServer", "Server started successfully.");

            }
            catch (Exception ex)
            {
                LogToScreen("Exception while attempting to start the listener: " + ex.Message);
                LogException(0, 0, 0, "StartRESTServer", ex.ToString());
            }
        }

        private void StopRESTServer()
        {
            if (httpListener != null) // Close and dispose of the current listener, if there is one.
            {
                // Create variables to hold the ASCOM device server operating URIs
                string apiOperatingUri = string.Format(@"http://{0}:{1}{2}", ServerIPAddressString, ServerPortNumber, SharedConstants.API_URL_BASE);
                string managementUri = string.Format(@"http://{0}:{1}{2}", ServerIPAddressString, ServerPortNumber, SharedConstants.MANAGEMENT_URL_BASE);
                LogMessage(0, 0, 0, "StopRESTServer", "Operating URI: " + apiOperatingUri);
                LogMessage(0, 0, 0, "StopRESTServer", "Management URI: " + managementUri);

                LogMessage(0, 0, 0, "StopRESTServer", "Removing Prefixes");
                try { httpListener.Prefixes.Remove(apiOperatingUri); } catch { } // Set up the listener on the api URI
                try { httpListener.Prefixes.Remove(managementUri); } catch { }// Set up the listener on the management URI

                LogMessage(0, 0, 0, "StopRESTServer", "Stopping current REST server");
                try { httpListener.Stop(); } catch { }
                try { httpListener.Close(); } catch { }
                try { httpListener = null; } catch { }
            }

            apiIsEnabled = false;
            PboxRESTStatus.BackColor = Color.Red;
            LblRESTStatus.Text = "REST Server Down";
        }

        private void ConnectDevices()
        {
            try
            {
                DisconnectDevices(); // Shut down all the ASCOM device instances
                ActiveObjects.Clear();
                GC.Collect();

                // Create new ASCOM device instances
                foreach (KeyValuePair<string, ConfiguredDevice> configuredDevice in ConfiguredDevices)
                {
                    if ((configuredDevice.Value.ProgID != SharedConstants.DEVICE_NOT_CONFIGURED) & (configuredDevice.Value.ProgID.Trim(" ".ToCharArray()) != "")) // Only attempt to process devices with valid ProgIDs
                    {
                        try
                        {
                            if (RunDriversOnSeparateThreads)
                            {
                                LogMessage(0, 0, 0, "ConnectDevices", string.Format("Creating driver on separate thread. This is thread: {0}", Thread.CurrentThread.ManagedThreadId));
                                Thread driverThread = new Thread(DriverOnSeparateThread);
                                driverThread.SetApartmentState(ApartmentState.STA);
                                driverThread.DisableComObjectEagerCleanup();
                                driverThread.IsBackground = true;
                                driverThread.Start(configuredDevice);
                                LogMessage(0, 0, 0, "ConnectDevices", string.Format("Thread started successfully. This is thread: {0}", Thread.CurrentThread.ManagedThreadId));

                                string deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber);
                                ActiveObjects[deviceKey] = new ActiveObject();

                                do
                                {
                                    Thread.Sleep(50);
                                    Application.DoEvents();
                                } while (ActiveObjects[deviceKey].DriverHostForm == null);

                                LogMessage(0, 0, 0, "ConnectDevices", string.Format("Completed create driver delegate on thread {0}", Thread.CurrentThread.ManagedThreadId));
                            }
                            else
                            {
                                if (this.InvokeRequired)
                                {
                                    CreateInstanceDelegate createInstanceDelegate = new CreateInstanceDelegate(CreateInstance);
                                    this.Invoke(createInstanceDelegate, new object[] { configuredDevice }); // Force the driver to be created on the main UI thread if we are currently executing on a different thread
                                }
                                else CreateInstance(configuredDevice); // We are on the UI thread so just create the driver
                            }
                        }
                        catch (Exception ex)
                        {
                            LogToScreen(string.Format("Exception while attempting to create thread for device: {0} {1}", configuredDevice.Value.ProgID, ex.Message));
                            LogException(0, 0, 0, "ConnectDevices", ex.ToString());
                        }
                    }
                }

                devicesAreConnected = true;

                PboxDriverStatus.BackColor = Color.Green; // Turn the "Connected / Disconnected" colour box green
                LbDriverStatus.Text = "Drivers Connected";
            }
            catch (Exception ex)
            {
                LogToScreen("Exception while attempting to create devices: " + ex.Message);
                LogException(0, 0, 0, "ConnectDevices", ex.ToString());
            }
        }

        private void DriverOnSeparateThread(object arg)
        {
            KeyValuePair<string, ConfiguredDevice> configuredDevice = (KeyValuePair<string, ConfiguredDevice>)arg; // Convert the supplied argument to the correct type
            string deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber); // Recreate the device key to use in log messages

            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("About to create driver host form"));
            DriverHostForm driverHostForm = new DriverHostForm(configuredDevice, this); // Create the form
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Created driver host form"));
            driverHostForm.Show(); // Make it come into existence - it doesn't exist until its shown for some reason
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Shown driver host form"));
            driverHostForm.Hide(); // Hide the form from view
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Hidden driver host form"));

            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Starting driver host environment for {0} on thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));
            Application.Run();  // Start the message loop on this thread to bring the form to life
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Environment for driver host {0} shut down on thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));

            // Thread will finish at this point
        }

        internal void CreateInstance(KeyValuePair<string, ConfiguredDevice> configuredDevice)
        {
            try
            {
                string deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber);
                LogMessage(0, 0, 0, "CreateInstance", string.Format("Creating device: {0}, ProgID: {1}, Key: {2} on thread {3}", configuredDevice.Key, configuredDevice.Value.ProgID, deviceKey, Thread.CurrentThread.ManagedThreadId));

                dynamic deviceObject = Activator.CreateInstance(Type.GetTypeFromProgID(configuredDevice.Value.ProgID));
                LogMessage(0, 0, 0, "CreateInstance", string.Format("Device object is null: {0}", (deviceObject == null)));
                ActiveObjects[deviceKey] = new ActiveObject(deviceObject, configuredDevice.Value.AllowConnectedSetFalse, configuredDevice.Value.AllowConnectedSetTrue);
                LogMessage(0, 0, 0, "CreateInstance", string.Format("ActiveObjects.DeviceObject is null: {0}", (ActiveObjects[deviceKey].DeviceObject == null)));

                try
                {
                    LogMessage(0, 0, 0, "CreateInstance", "Connecting device");
                    deviceObject.Connected = true;
                }
                catch (Exception ex1)
                {
                    LogException(0, 0, 0, "CreateInstance", "Error connecting to device: \r\n" + ex1.ToString());
                }
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "CreateInstance", "Error creating device: \r\n" + ex.ToString());
            }
        }

        private void DisconnectDevices()
        {
            LogMessage(0, 0, 0, "DisconnectDevices", "Clearing devices");
            int RemainingObjectCount;

            PboxDriverStatus.BackColor = Color.Red; // Turn the "Connected / Disconnected" colour box red
            LbDriverStatus.Text = "Drivers Unloaded";
            devicesAreConnected = false;

            // Clear all of the current drivers
            foreach (KeyValuePair<string, ActiveObject> activeObject in ActiveObjects)
            {
                try
                {
                    if (RunDriversOnSeparateThreads)
                    {
                        DestroyDriverDelegate destroyDriverDelegate = new DestroyDriverDelegate(activeObject.Value.DriverHostForm.DestroyDriver);
                        LogMessage(0, 0, 0, "DisconnectDevices", string.Format("Starting invoke of driver delegate for device {0} on thread {1}", activeObject.Key, Thread.CurrentThread.ManagedThreadId));
                        activeObject.Value.DriverHostForm.Invoke(destroyDriverDelegate);
                        LogMessage(0, 0, 0, "DisconnectDevices", string.Format("Completed invoke of driver delegate for device {0} on thread {1}", activeObject.Key, Thread.CurrentThread.ManagedThreadId));
                    }
                    else
                    {
                        RemainingObjectCount = DestroyDriver(activeObject.Key);
                    }
                }
                catch (KeyNotFoundException) { } // Ignore key not found exceptions, they are expected for unconfigured devices

                catch (Exception ex)
                {
                    LogException(0, 0, 0, "DisconnectDevices", "  ReleaseComObject Exception: " + ex.ToString());
                }
            }

            ActiveObjects.Clear(); // Clear the list of active objects now that all active device instances have been destroyed

            GC.Collect(); // Reclaim memory and destroy all deleted objects
        }

        internal static int DestroyDriver(string DeviceKey)
        {
            int RemainingObjectCount;

            LogMessage(0, 0, 0, "DestroyDriver", string.Format("Destroying driver: {0}", DeviceKey));

            device = ActiveObjects[DeviceKey].DeviceObject;

            try { device.Connected = false; } catch { }// Don't throw exceptions from these
            try { device.Link = false; } catch { }
            try { device.Dispose(); } catch { }

            // Now destroy the device instance
            int LoopCount = 0;
            RemainingObjectCount = 0;

            if (device != null)
            {
                do
                {
                    LoopCount += 1;
                    try { RemainingObjectCount = Marshal.ReleaseComObject(device); } catch { } // Don't throw exceptions from this
                    LogMessage(0, 0, 0, "DestroyDriver", string.Format("Remaining {0} count: {1}, LoopCount: {2}", DeviceKey, RemainingObjectCount, LoopCount));
                }
                while ((RemainingObjectCount > 0) & (LoopCount != 20));
                device = null;
            }

            LogMessage(0, 0, 0, "DestroyDriver", string.Format("Destroyed driver: {0}", DeviceKey));

            return RemainingObjectCount;
        }

        internal void WaitFor(int Duration)
        {
            const int SLEEP_TIME = 20;
            int remainingDuration;

            remainingDuration = Duration;

            do
            {
                remainingDuration -= SLEEP_TIME;
                Thread.Sleep(SLEEP_TIME);
                Application.DoEvents();

            } while (remainingDuration > 0);
        }

        internal static void LogMessage(int clientID, int clientTransactionID, int serverTransactionID, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.LogMessage(clientID, clientTransactionID, serverTransactionID, Method, Message);
            }
        }

        internal static void LogException(int clientID, int clientTransactionID, int serverTransactionID, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.LogMessageCrLf(clientID, clientTransactionID, serverTransactionID, Method, Message);
            }
        }

        internal static void LogBlankLine(int clientID, int clientTransactionID, int serverTransactionID)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.BlankLine();
            }
        }

        private static void CheckWhetherNewLogRequired(int clientID, int clientTransactionID, int serverTransactionID)
        {
            if (TL.Enabled) // We are logging so we have to close the current log and start a new one if we have moved to a new day
            {
                DateTime now = DateTime.Now;
                if (LastTraceLogTime.DayOfYear != now.DayOfYear) // We have moved onto tomorrow so close the current log and start another
                {
                    TL.LogMessage(clientID, clientTransactionID, serverTransactionID, "EndOfDay", "Closing this log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));
                    TL.Enabled = false;
                    TL.Dispose();
                    TL = null;
                    TL = new TraceLoggerPlus("", SERVER_TRACELOGGER_NAME)
                    {
                        Enabled = true
                    };
                    TL.LogMessage(clientID, clientTransactionID, serverTransactionID, "StartOfDay", "Opening a new log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));
                }
                LastTraceLogTime = now; // Update the last log time so that it can be tested on the next logging call
            }
        }

        /// <summary>
        /// Display a message on the screen log
        /// </summary>
        /// <param name="screenMessage">Message to be displayed</param>
        /// <remarks>The log will be limited to a total length of SCREEN_LOG_MAXIMUM_LENGTH and the new message must be less than or equal to SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH characters in length</remarks>
        private void LogToScreen(string screenMessage)
        {
            // Invoke the code on the UI thread if required
            if (txtLog.InvokeRequired)
            {
                SetTextCallback logToScreenDelegate = new SetTextCallback(LogToScreen);
                this.Invoke(logToScreenDelegate, screenMessage);
            }
            else
            {
                // Limit the maximum number of characters in the screen log to maintain performance
                if (txtLog.TextLength > SCREEN_LOG_MAXIMUM_LENGTH) txtLog.Text = txtLog.Text.Substring(SCREEN_LOG_MAXIMUM_LENGTH / 3);

                // Limit the number of characters that can be added in one message to maintain performance
                if (screenMessage.Length > SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH) screenMessage = string.Format("{0} - Screen display truncated to {1} characters in order to maintain performance", screenMessage.Substring(0, SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH), SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH);

                txtLog.AppendText(screenMessage + "\r\n"); // Add the text to the screen log
                txtLog.SelectionStart = txtLog.Text.Length; // Move the text box focus to the newly added text
            }
        }

        private void IncrementConcurrencyCounter()
        {
            if (txtConcurrency.InvokeRequired)
            {
                SetConcurrencyCallback d = new SetConcurrencyCallback(IncrementConcurrencyCounter);
                this.Invoke(d, new object[] { });
            }
            else
            {
                Interlocked.Increment(ref numberOfConcurrentTransactions); // Increment the concurrent transaction counter in a thread safe manner
                txtConcurrency.Text = numberOfConcurrentTransactions.ToString();
            }
        }

        internal void DecrementConcurrencyCounter()
        {
            if (this.txtConcurrency.InvokeRequired)
            {
                SetConcurrencyCallback decrementConcurrencyCounterDelegate = new SetConcurrencyCallback(DecrementConcurrencyCounter);
                this.Invoke(decrementConcurrencyCounterDelegate, new object[] { });
            }
            else
            {
                Interlocked.Decrement(ref numberOfConcurrentTransactions); // Decrement the concurrent transaction counter in a thread safe manner
                txtConcurrency.Text = numberOfConcurrentTransactions.ToString();
            }
        }

        /// <summary>
        /// Clean the message string to remove illegal characters such as carriage return and line feed from the string so that it can be sent as HTTP text
        /// </summary>
        /// <param name="message">String message to be cleaned</param>
        /// <returns>Cleaned message string</returns>
        private string CleanMessage(string message)
        {
            //string cleanedMessage = message.Replace("\r", string.Empty);
            //cleanedMessage = cleanedMessage.Replace("\n", string.Empty);
            string cleanedMessage = Regex.Replace(message, @"[^\u0020-\u007E]", string.Empty);
            return cleanedMessage;
        }

        #endregion

        #region Profile management

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        public static void ReadProfile()
        {

            using (Configuration driverProfile = new Configuration())
            {

                // Initialise the logging trace state from the Profile
                TraceState = driverProfile.GetValue<bool>(SERVER_TRACE_LEVEL_PROFILENAME, string.Empty, SERVER_TRACE_LEVEL_DEFAULT);
                DebugTraceState = driverProfile.GetValue<bool>(SERVER_DEBUG_TRACE_PROFILENAME, string.Empty, SERVER_DEBUG_TRACE_DEFAULT);
                ServerIPAddressString = driverProfile.GetValue<string>(SERVER_IPADDRESS_PROFILENAME, string.Empty, SERVER_IPADDRESS_DEFAULT);
                ServerPortNumber = driverProfile.GetValue<decimal>(SERVER_PORTNUMBER_PROFILENAME, string.Empty, SERVER_PORTNUMBER_DEFAULT);
                StartWithDevicesConnected = driverProfile.GetValue<bool>(SERVER_AUTOCONNECT_PROFILENAME, string.Empty, SERVER_AUTOCONNECT_DEFAULT);
                AccessLogEnabled = driverProfile.GetValue<bool>(SERVER_ACCESS_LOG_PROFILENAME, string.Empty, SERVER_ACCESS_LOG_DEFAULT);
                ScreenLogRequests = driverProfile.GetValue<bool>(SCREEN_LOG_REQUESTS_PROFILENAME, string.Empty, SCREEN_LOG_REQUESTS_DEFAULT);
                ScreenLogResponses = driverProfile.GetValue<bool>(SCREEN_LOG_RESPONSES_PROFILENAME, string.Empty, SCREEN_LOG_RESPONSES_DEFAULT);
                ManagementInterfaceEnabled = driverProfile.GetValue<bool>(MANAGEMENT_INTERFACE_ENABLED_PROFILENAME, string.Empty, MANGEMENT_INTERFACE_ENABLED_DEFAULT);
                StartWithApiEnabled = driverProfile.GetValue<bool>(START_WITH_API_ENABLED_PROFILENAME, string.Empty, START_WITH_API_ENABLED_DEFAULT);
                RunDriversOnSeparateThreads = driverProfile.GetValue<bool>(RUN_DRIVERS_ON_SEPARATE_THREADS_PROFILENAME, string.Empty, RUN_DRIVERS_ON_SEPARATE_THREADS_DEFAULT);

                foreach (string deviceName in ServerDeviceNames)
                {
                    string deviceType = driverProfile.GetValue<string>(DEVICETYPE_PROFILENAME, deviceName, DEVICETYPE_DEFAULT);
                    string progID = driverProfile.GetValue<string>(PROGID_PROFILENAME, deviceName, PROGID_DEFAULT);
                    string description = driverProfile.GetValue<string>(DESCRIPTION_PROFILENAME, deviceName, DESCRIPTION_DEFAULT);
                    int deviceNumber = Convert.ToInt32(driverProfile.GetValue<int>(DEVICENUMBER_PROFILENAME, deviceName, DEVICENUMBER_DEFAULT));
                    bool allowConnectedSetFalse = Convert.ToBoolean(driverProfile.GetValue<bool>(ALLOW_CONNECTED_SET_FALSE_PROFILENAME, deviceName, ALLOW_CONNECTED_SET_FALSE_DEFAULT));
                    bool allowConnectedSetTrue = Convert.ToBoolean(driverProfile.GetValue<bool>(ALLOW_CONNECTED_SET_TRUE_PROFILENAME, deviceName, ALLOW_CONNECTED_SET_TRUE_DEFAULT));

                    ConfiguredDevices[deviceName] = new ConfiguredDevice(deviceType, progID, description, deviceNumber, allowConnectedSetFalse, allowConnectedSetTrue);
                }
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        public static void WriteProfile()
        {
            using (Configuration driverProfile = new Configuration())
            {
                // Save the variable state to the Profile
                driverProfile.SetValue<bool>(SERVER_TRACE_LEVEL_PROFILENAME, string.Empty, TraceState);
                driverProfile.SetValue<bool>(SERVER_DEBUG_TRACE_PROFILENAME, string.Empty, DebugTraceState);
                driverProfile.SetValue<string>(SERVER_IPADDRESS_PROFILENAME, string.Empty, ServerIPAddressString);
                driverProfile.SetValue<decimal>(SERVER_PORTNUMBER_PROFILENAME, string.Empty, ServerPortNumber);
                driverProfile.SetValue<bool>(SERVER_AUTOCONNECT_PROFILENAME, string.Empty, StartWithDevicesConnected);
                driverProfile.SetValue<bool>(SERVER_ACCESS_LOG_PROFILENAME, string.Empty, AccessLogEnabled);
                driverProfile.SetValue<bool>(SCREEN_LOG_REQUESTS_PROFILENAME, string.Empty, ScreenLogRequests);
                driverProfile.SetValue<bool>(SCREEN_LOG_RESPONSES_PROFILENAME, string.Empty, ScreenLogResponses);
                driverProfile.SetValue<bool>(MANAGEMENT_INTERFACE_ENABLED_PROFILENAME, string.Empty, ManagementInterfaceEnabled);
                driverProfile.SetValue<bool>(START_WITH_API_ENABLED_PROFILENAME, string.Empty, StartWithApiEnabled);
                driverProfile.SetValue<bool>(RUN_DRIVERS_ON_SEPARATE_THREADS_PROFILENAME, string.Empty, RunDriversOnSeparateThreads);

                foreach (string deviceName in ServerDeviceNames)
                {
                    driverProfile.SetValue<string>(DEVICETYPE_PROFILENAME, deviceName, ConfiguredDevices[deviceName].DeviceType.ToString());
                    driverProfile.SetValue<string>(PROGID_PROFILENAME, deviceName, ConfiguredDevices[deviceName].ProgID.ToString());
                    driverProfile.SetValue<string>(DESCRIPTION_PROFILENAME, deviceName, ConfiguredDevices[deviceName].Description.ToString());
                    driverProfile.SetValue<string>(DEVICENUMBER_PROFILENAME, deviceName, ConfiguredDevices[deviceName].DeviceNumber.ToString());
                    driverProfile.SetValue<bool>(ALLOW_CONNECTED_SET_FALSE_PROFILENAME, deviceName, ConfiguredDevices[deviceName].AllowConnectedSetFalse);
                    driverProfile.SetValue<bool>(ALLOW_CONNECTED_SET_TRUE_PROFILENAME, deviceName, ConfiguredDevices[deviceName].AllowConnectedSetTrue);
                }
            }
        }

        #endregion

        #region Form Event handlers

        private void BtnSetup_Click(object sender, EventArgs e)
        {
            bool apiEnabled; // Local variables to hold the current server state
            bool devicesConnected;

            LogMessage(0, 0, 0, "SetupButton", string.Format("Saving current server state", apiIsEnabled, devicesAreConnected));
            apiEnabled = apiIsEnabled; // Save current server state
            devicesConnected = devicesAreConnected;

            LogMessage(0, 0, 0, "SetupButton", string.Format("Stopping REST Server"));
            StopRESTServer(); // Shut down access while we use the Setup screen
            LogMessage(0, 0, 0, "SetupButton", string.Format("Disconnecting devices"));
            DisconnectDevices(); // Disconnect all devices so we can use their Setup screens if necessary

            LogMessage(0, 0, 0, "SetupButton", string.Format("Loading Setup form"));
            SetupForm frm = new SetupForm();
            DialogResult outcome = frm.ShowDialog();
            LogMessage(0, 0, 0, "SetupButton", string.Format("Setup dialogue outcome: {0}", outcome.ToString()));

            // Restore original server state
            if (devicesConnected)
            {
                LogMessage(0, 0, 0, "SetupButton", string.Format("Connecting devices"));
                ConnectDevices();
            }
            else
            {
                LogMessage(0, 0, 0, "SetupButton", string.Format("Not connecting devices because they weren't connected in the first place."));
            }

            if (apiEnabled)
            {
                LogMessage(0, 0, 0, "SetupButton", string.Format("Starting REST server"));
                StartRESTServer();
            }
            else
            {
                LogMessage(0, 0, 0, "SetupButton", string.Format("Not starting REST server because it wasn't running in the first place."));
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnConnectDevices_Click(object sender, EventArgs e)
        {
            if (!devicesAreConnected) ConnectDevices();
        }

        private void BtnDisconnectDevices_Click(object sender, EventArgs e)
        {
            if (devicesAreConnected) DisconnectDevices();
        }

        private void BtnStartRESTServer_Click(object sender, EventArgs e)
        {
            if (!apiIsEnabled) StartRESTServer();
        }

        private void BtnStopRESTServer_Click(object sender, EventArgs e)
        {
            if (apiIsEnabled) StopRESTServer();
        }

        private void ChkLogRequestsAndResponses_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (DebugTraceState) LogMessage(0, 0, 0, "ScreenLog_Changed", string.Format("Check box {0} has changed to {1}, saving new value", checkBox.Name, checkBox.Checked.ToString()));
            ScreenLogRequests = chkLogRequests.Checked;
            ScreenLogResponses = chkLogResponses.Checked;
            WriteProfile();
        }

        /// <summary>
        /// Lays out server form controls to appear visually pleasing at different sizes.
        /// </summary>
        /// <param name="sender">ServerForm instance</param>
        /// <param name="e">Event argument that contains no data</param>
        /// <remarks>Lays out controls in groups with equal spacing between groups. Supports full width controls and those that need to be centred on the control group median line</remarks>
        private void ServerForm_Resize(object sender, EventArgs e)
        {
            Form form = (Form)sender; // Get the supplied control as a Form

            if (WindowState != FormWindowState.Minimized) // No need to change anything if the window is minimised
            {
                int formWidth = form.Width;
                int controlLeftPosition = formWidth - CONTROL_LEFT_OFFSET;
                int controlCentrePosition = controlLeftPosition + CONTROL_CENTRE_OFFSET;

                // Place the Log messages text box - This must be set first in this method as other controls are located relative to it
                txtLog.Size = new Size(formWidth - CONTROL_SPACE_WIDTH, form.Height - LOG_HEIGHT_OFFSET);

                // Pre calculate some items
                int controlSpacing = (txtLog.Height - CONTROL_OVERALL_HEIGHT) / (NUMBER_OF_CONTROL_GROUPS - 1); // Calculate the vertical distance between controls and control groups. This allows the controls to move closer together when the window is small.
                if (controlSpacing > CONTROL_SPACING_MAXIMUM) controlSpacing = CONTROL_SPACING_MAXIMUM; // Limit the maximum spacing so that it doesn't get too large and become unsightly
                int controlsTop = txtLog.Top + (txtLog.Height / 2) - ((CONTROL_OVERALL_HEIGHT + 5 * controlSpacing) / 2); // Calculate the location of the top of the controls

                // Place the form title
                // Transition from Form.Width =900 to Form.Width = Title.Width + CONTROL_SPACE_WIDTH + Form.Left
                int titleLeftMessagesCentred = txtLog.Left + ((txtLog.Width - lblTitle.Width) / 2) - 20; // Centre the title over the log message text box
                int titleLeftFormCentred = ((form.Width - lblTitle.Width) / 2) - 8; // If the text box is narrower than the title, centre the title within the overall width of the form
                int titleLeft = 0;
                int titleTransitionPositionStart = lblTitle.Width + CONTROL_SPACE_WIDTH + txtLog.Left;

                if (form.Width < TITLE_TRANSITION_POSITION_END) // We may be in the transition region or smaller than this
                {
                    if (form.Width > titleTransitionPositionStart) // We are in the transition region
                    {
                        int transitionSize = titleLeftFormCentred - titleLeftMessagesCentred;
                        double transitionFraction = 1.0 - Convert.ToDouble(form.Width - titleTransitionPositionStart) / Convert.ToDouble(TITLE_TRANSITION_POSITION_END - titleTransitionPositionStart);
                        titleLeft = titleLeftMessagesCentred + (int)(transitionSize * transitionFraction);
                    }
                    else // Smaller than lower transition point, just go with form centred
                    {
                        titleLeft = titleLeftFormCentred;
                    }
                }
                else // Larger than the upper transition point so do with message centred
                {
                    titleLeft = titleLeftMessagesCentred;
                }
                lblTitle.Location = new Point(titleLeft, TITLE_OFFSET_FROM_TOP); // Set the title position

                // Control Group 1 - Concurrent transactions counter
                txtConcurrency.Location = new Point(controlCentrePosition - 3, controlsTop);
                lblConcurrentTransactions.Location = new Point(controlCentrePosition + 33, txtConcurrency.Top - 4);

                // Control Group 2 - Log requests and responses check boxes
                chkLogRequests.Location = new Point(controlCentrePosition + 4, txtConcurrency.Top + controlSpacing);
                chkLogResponses.Location = new Point(controlCentrePosition + 4, chkLogRequests.Top + +20);

                //Control Group 3 - REST Server Status, Stop and Start
                PboxRESTStatus.Location = new Point(controlLeftPosition, chkLogResponses.Top + controlSpacing - 5);
                BtnStopRESTServer.Location = new Point(controlLeftPosition - 1, PboxRESTStatus.Top + 30);
                BtnStartRESTServer.Location = new Point(controlLeftPosition + 84, PboxRESTStatus.Top + 30);

                // Control Group 4 - Device Status, Disconnect and Connect
                PboxDriverStatus.Location = new Point(controlLeftPosition, BtnStopRESTServer.Top + controlSpacing + 3);
                BtnDisconnectDevices.Location = new Point(controlLeftPosition - 1, PboxDriverStatus.Top + 30);
                BtnConnectDevices.Location = new Point(controlLeftPosition + 84, PboxDriverStatus.Top + 30);

                // Control Group 5 - Setup button
                BtnSetup.Location = new Point(controlCentrePosition, BtnDisconnectDevices.Top + controlSpacing + 2);

                // Control Group 6 - Exit button
                BtnExit.Location = new Point(controlCentrePosition, BtnSetup.Top + controlSpacing + 2);
            }
        }

        #endregion

        #region API request event handlers

        /// <summary>
        /// Called to handle an asynchronous request received event from the HttpListener
        /// </summary>
        /// <param name="result">IAsyncResult object that can be used to obtain the context object that contains the request and response objects</param>
        /// <remarks>This routine receives an asynchronous event from the HttpListener when an HTTP PUT or GET request is received by the server. 
        /// It retrieves the http context object for this event, sets up the next asynchronous call so that other events can be received while this one is processing 
        /// and then calls ProcessRequest to respond to the received request.
        /// 
        /// This method will be called by the HttpListener on the first free thread in the background pool,
        /// </remarks>
        protected void RestRequestReceivedHandler(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState; // Get the listener instance from which this particular callback has come, it is supplied as the state parameter on the BeginGetContext call
            HttpListenerContext context = null;

            // Get the result context from the client's call
            try
            {
                if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - Request received. Is thread pool thread: {1}. Is background thread: {2}.", Thread.CurrentThread.ManagedThreadId.ToString(), Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.IsBackground));
                context = listener.EndGetContext(result); // Get the context object
            }
            catch (NullReferenceException) // httpListener is null because we are closing down or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - EndGetContext - httpListener is null so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }
            catch (ObjectDisposedException) // httpListener is disposed because the REST server has been stopped or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - EndGetContext - httpListener is disposed so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }

            catch (Exception ex)
            {
                LogException(0, 0, 0, "WebRequestCallback", ex.ToString()); // Log the exception
                if (context != null) // We have a context object but also an exception so return the error message to the client with a 500 status code
                {
                    Return500Error(context.Request, context.Response, ex.Message, 0, 0, 0);
                    return;
                }
                else // No context object so not possible to return an error to the client, just log the error and increment the error counter
                {
                    LogException(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - EndGetContext exception: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString()));
                    Interlocked.Increment(ref numberOfConsecutiveErrors);
                }
            }

            // Set up the next call back            
            try
            {
                if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - Setting up new call back to wait for next request ", Thread.CurrentThread.ManagedThreadId.ToString()));
                httpListener.BeginGetContext(new AsyncCallback(RestRequestReceivedHandler), httpListener);
            }
            catch (NullReferenceException) // httpListener is null because we are closing down or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - BeginGetContext - httpListener is null so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "WebRequestCallback", ex.ToString()); // Log the exception
                if (context != null) // We have a context object but also an exception so return the error message to the client with a 500 status code
                {
                    Return500Error(context.Request, context.Response, ex.Message, 0, 0, 0);
                    return;
                }
                else // No context object so not possible to return an error to the client
                {
                    LogException(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - BeginGetContext exception: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString()));
                    Interlocked.Increment(ref numberOfConsecutiveErrors);
                    return;
                }
            }

            Interlocked.Exchange(ref numberOfConsecutiveErrors, 0); // Reset the consecutive errors counter to zero

            // Now process this request, any exceptions are handled by the ProcessRequest method itself
            if (DebugTraceState) LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - Processing received message.", Thread.CurrentThread.ManagedThreadId.ToString()));
            ProcessRestRequest(context);

            // Shut down the listener and close down device drivers if the maximum number of errors has been reached
            if (numberOfConsecutiveErrors == MAX_ERRORS_BEFORE_CLOSE)
            {
                LogMessage(0, 0, 0, "WebRequestCallback", string.Format("Thread {0} - Maximum number of errors ({1}) reached, closing server and device drivers.", Thread.CurrentThread.ManagedThreadId, MAX_ERRORS_BEFORE_CLOSE));

                // Clear down the listener
                StopRESTServer();

                // Clear the devices
                DisconnectDevices();
            }
        }

        /// <summary>
        /// Processes the request received by the server
        /// </summary>
        /// <param name="context">Context object that contains the request and response objects.</param>
        private void ProcessRestRequest(HttpListenerContext context)
        {
            // Local convenience variables to hold this transaction's information
            int clientID = 0;
            int clientTransactionID = 0;
            int serverTransactionID = 0;
            NameValueCollection suppliedParameters;
            HttpListenerRequest request;
            HttpListenerResponse response;

            try
            {
                IncrementConcurrencyCounter();

                suppliedParameters = new NameValueCollection(); // Create a collection to hold the supplied parameters
                request = context.Request;
                response = context.Response;
                serverTransactionID = GetServerTransactionID();

                response.Headers.Add(HttpResponseHeader.Server, "ASCOM Rest API Server -");

                // Create a single collection holding all supplied parameters including both those supplied as query variables in the url string and those contained within the form post (http PUT only).
                suppliedParameters.Add(request.QueryString); // Add the query string parameters to the collection

                if (request.HasEntityBody) // Now add any parameters supplied in the form POST
                {
                    string formParameters;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) // Extract the aggregated parameter string from the form within the request
                    {
                        formParameters = reader.ReadToEnd();
                    }

                    string[] rawParams = formParameters.Split('&'); // Parse the aggregated parameter string into an array of key / value pair strings

                    foreach (string param in rawParams) // Parse each key / value pair string into its key and value and add these to the parameters collection
                    {
                        string[] kvPair = param.Split('=');
                        string key = kvPair[0];
                        string value = HttpUtility.UrlDecode(kvPair[1]);
                        suppliedParameters.Add(key, value);
                    }
                }

                // Extract the caller's IP address into hostIPAddress
                string hostIPAddress;
                if (request.Headers[X_FORWARDED_FOR] != null) hostIPAddress = request.Headers[X_FORWARDED_FOR]; // The request was fronted by an Apache web server
                else hostIPAddress = request.UserHostAddress; // The request came straight to this application
                AccessLog.LogMessage("    " + hostIPAddress, string.Format("{0} {1}", request.HttpMethod, request.Url.AbsolutePath));
                if (ScreenLogRequests) LogToScreen(string.Format("{0} {1} {2}", hostIPAddress, request.HttpMethod, request.Url.AbsolutePath));

                // Extract the client ID number from the supplied URI / Form, if present
                string clientIDString = suppliedParameters[SharedConstants.CLIENTID_PARAMETER_NAME];
                if (clientIDString != null) // Some value was supplied for this parameter
                {
                    // Parse the integer value out or throw a 400 error if the value is not an integer
                    if (!int.TryParse(clientIDString, out clientID))
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}, Concurrent requests: {3}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString(), numberOfConcurrentTransactions));
                        Return400Error(response, "Client ID is not an integer: " + suppliedParameters[SharedConstants.CLIENTID_PARAMETER_NAME], clientID, clientTransactionID, serverTransactionID);
                        return;
                    }
                }

                // Extract the client transaction ID from the supplied URI / Form, if present
                string clientTransactionIDString = suppliedParameters[SharedConstants.CLIENTTRANSACTION_PARAMETER_NAME];
                if (clientTransactionIDString != null) // Some value was supplied for this parameter
                {
                    // Parse the integer value out or throw a 400 error if the value is not an integer
                    if (!int.TryParse(clientTransactionIDString, out clientTransactionID))
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));
                        Return400Error(response, "Client transaction ID is not an integer: " + suppliedParameters[SharedConstants.CLIENTTRANSACTION_PARAMETER_NAME], clientID, clientTransactionID, serverTransactionID);
                        return;
                    }
                }

                // Log the request 
                LogMessage(clientID, clientTransactionID, serverTransactionID, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));

                if (DebugTraceState) // List headers and detailed parameter list if in debug mode
                {
                    foreach (string key in request.Headers.AllKeys)
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "RequestReceived", string.Format("Header {0} = {1}", key, request.Headers[key]));
                    }
                    foreach (string key in suppliedParameters.AllKeys)
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "RequestReceived", string.Format("Parameter {0} = {1}", key, suppliedParameters[key]));
                    }
                }

                if (request.Url.AbsolutePath.Trim().ToLowerInvariant().StartsWith(SharedConstants.API_URL_BASE)) // Process requests whose URIs start with /api
                {
                    // Return a 403 error if the API is not enabled through the management console
                    if (!apiIsEnabled)
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));
                        Return403Error(response, API_INTERFACE_NOT_ENABLED_MESSAGE, clientID, clientTransactionID, serverTransactionID);
                        return;
                    }

                    // Split the supplied URI into its elements demarked by the / character and remove any leading / trailing space characters from each element
                    // Element [0] will be "api"
                    // Element [1] will be the API version (whole number prefixed by V e.g. V1)
                    // Element [2] will be device type
                    // Element [3] will be the device number within the device type collection
                    // Element [4] will be a Method
                    string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);

                    // Basic error checking - We must have received 5 elements, now in the elements array, in order to have received a valid API request so check this here:
                    if (elements.Length != 5)
                    {
                        Return400Error(response, "Incorrect API format - Received: " + request.Url.AbsolutePath + " Required format is: <b> " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                        return;
                    }
                    else // We have received the required 5 elements in the URI
                    {
                        for (int i = 0; i < elements.Length; i++) // Remove leading and trailing space characters from each element
                        {
                            elements[i] = elements[i].Trim();
                        }

                        switch (elements[URL_ELEMENT_API_VERSION].ToLowerInvariant()) // Process each API version as necessary (at 8/8/17 only V1 is implemented)
                        {
                            case SharedConstants.API_VERSION_V1: // OK so we have a V1 request
                                if ((ServerDeviceNumbers.Contains(elements[URL_ELEMENT_DEVICE_NUMBER].ToLowerInvariant())) & (elements[URL_ELEMENT_DEVICE_NUMBER].ToLowerInvariant() != "")) // OK so we have a valid device number
                                {
                                    string deviceKey = elements[URL_ELEMENT_DEVICE_TYPE].ToLowerInvariant() + @"/" + elements[URL_ELEMENT_DEVICE_NUMBER].ToLowerInvariant(); // Create a unique key from device type and device number

                                    // Ensure that the device exists
                                    if (ActiveObjects.ContainsKey(deviceKey)) // Device is configured
                                    {
                                        // Ensure that we only process one command at a time for this driver
                                        lock (ActiveObjects[deviceKey].CommandLock) // Proceed when we have a lock for this device
                                        {
                                            // Confirm that the device requested is available on this server and process the request
                                            if (RunDriversOnSeparateThreads)
                                            {
                                                LogMessage(clientID, clientTransactionID, serverTransactionID, "ProcessRequestAsync", string.Format("Sending driver command to {0} from thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));
                                                DriverCommandDelegate driverCommandDelegate = new DriverCommandDelegate(ActiveObjects[deviceKey].DriverHostForm.DriverCommand);
                                                ActiveObjects[deviceKey].DriverHostForm.Invoke(driverCommandDelegate, new RequestData(clientID, clientTransactionID, serverTransactionID, suppliedParameters, request, response, elements, deviceKey));
                                                LogMessage(clientID, clientTransactionID, serverTransactionID, "ProcessRequestAsync", string.Format("Completed driver command to {0} from thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));
                                            }
                                            else // Driver is running on the UI thread so just process the command
                                            {
                                                ProcessDriverCommand(clientID, clientTransactionID, serverTransactionID, suppliedParameters, request, response, elements, deviceKey);
                                            }
                                        }
                                    }
                                    else // Specified device is not configured so return an error message
                                    {
                                        Return400Error(response, string.Format("Device {0} is not configured on this server", deviceKey), clientID, clientTransactionID, serverTransactionID);
                                    }
                                }
                                else
                                {
                                    Return400Error(response, "Unsupported or invalid integer device number: " + elements[URL_ELEMENT_DEVICE_NUMBER] + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                } // End of valid device numbers

                                break; // End of valid api version numbers
                            default:
                                Return400Error(response, "Unsupported API version: " + elements[URL_ELEMENT_API_VERSION] + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                break;
                        }

                    }

                    LogBlankLine(clientID, clientTransactionID, serverTransactionID);
                }
                else if (request.Url.AbsolutePath.Trim().ToLowerInvariant().StartsWith(SharedConstants.MANAGEMENT_URL_BASE)) // Process server requests
                {
                    // Only permit processing if access has been granted through the setup dialogue
                    if (ManagementInterfaceEnabled)
                    {
                        // Split the supplied URI into its elements demarked by the / character and remove any leading / trailing space characters from each element
                        // Element [0] will be "server"
                        // Element [1] will be the API version (whole number prefixed by V e.g. V1)
                        // Element [2] will be the configuration command
                        string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);

                        // Basic error checking - We must have received 3 elements, now in the elements array, in order to have received a valid API request so check this here:
                        if (elements.Length != 3)
                        {
                            Return400Error(response, "Incorrect API format - Received: " + request.Url.AbsolutePath + " Required format is: <b> " + CORRECT_SERVER_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                            return;
                        }
                        else // We have received the required 3 elements in the URI
                        {
                            for (int i = 0; i < elements.Length; i++)
                            {
                                elements[i] = elements[i].Trim();// Remove leading and trailing space characters
                                LogMessage(clientID, clientTransactionID, serverTransactionID, "ManagmentCommand", string.Format("Received element {0} = {1}", i, elements[i]));
                            }

                            switch (elements[URL_ELEMENT_API_VERSION].ToLowerInvariant())
                            {
                                case SharedConstants.API_VERSION_V1: // OK so we have a V1 request
                                    try // Confirm that the command requested is available on this server
                                    {
                                        string commandName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(elements[URL_ELEMENT_SERVER_COMMAND].ToLowerInvariant()); // Capitalise the first letter of the command
                                        switch (request.HttpMethod.ToUpperInvariant())
                                        {
                                            case "GET": // Read methods
                                                switch (elements[URL_ELEMENT_SERVER_COMMAND].ToLowerInvariant())
                                                {
                                                    // BOOL Get Values
                                                    // MANGEMENT_API_ENABLED removed because it won't work if the management api is disabled
                                                    /*case SharedConstants.MANGEMENT_API_ENABLED:
                                                        BoolResponse responseClass = new BoolResponse(clientTransactionID, serverTransactionID, commandName, apiIsEnabled);
                                                        string boolResponseJson = JsonConvert.SerializeObject(responseClass);
                                                        SendResponseToClient(commandName, request, response, null, boolResponseJson, clientID, clientTransactionID, serverTransactionID);
                                                        break; */

                                                    // INT Get Values
                                                    case SharedConstants.MANGEMENT_CONCURRENT_CALLS:
                                                        // Returns the number of concurrent device calls. 
                                                        // This call will have incremented the concurrent call counter in its own right so the value returned by this call is one less than the numberOfConcurrentTransactions counter value,
                                                        // which will be the number of concurrent device calls.
                                                        IntResponse intResponseClass = new IntResponse(clientTransactionID, serverTransactionID, commandName, numberOfConcurrentTransactions - 1);
                                                        string intResponseJson = JsonConvert.SerializeObject(intResponseClass);
                                                        SendResponseToClient(commandName, request, response, null, intResponseJson, clientID, clientTransactionID, serverTransactionID);
                                                        break;

                                                    // STRING Get Values
                                                    case SharedConstants.MANGEMENT_PROFILE:
                                                        List<ProfileDevice> profileDevices = new List<ProfileDevice>();
                                                        try
                                                        {
                                                            using (Profile profile = new Profile())
                                                            {
                                                                ArrayList deviceTypes = profile.RegisteredDeviceTypes;
                                                                foreach (string deviceType in deviceTypes)
                                                                {
                                                                    ArrayList registeredDevices = profile.RegisteredDevices(deviceType);
                                                                    foreach (KeyValuePair kvp in registeredDevices)
                                                                    {
                                                                        profileDevices.Add(new ProfileDevice(deviceType, kvp.Key, kvp.Value));
                                                                    }
                                                                }
                                                            }
                                                            ProfileResponse profileResponse = new ProfileResponse(clientTransactionID, serverTransactionID, elements[URL_ELEMENT_SERVER_COMMAND], profileDevices)
                                                            {
                                                                DriverException = null
                                                            };
                                                            string profileResponseJson = JsonConvert.SerializeObject(profileResponse);
                                                            SendResponseToClient(commandName, request, response, null, profileResponseJson, clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Return500Error(request, response, "Unexpected exception in command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + ex.ToString(), clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        break;

                                                    case SharedConstants.MANGEMENT_CONFIGURATION:
                                                        ConfigurationResponse configurationResponse = new ConfigurationResponse(clientTransactionID, serverTransactionID, elements[URL_ELEMENT_SERVER_COMMAND], ConfiguredDevices)
                                                        {
                                                            DriverException = null
                                                        };
                                                        ;
                                                        string configurationResponseJson = JsonConvert.SerializeObject(configurationResponse);
                                                        SendResponseToClient(commandName, request, response, null, configurationResponseJson, clientID, clientTransactionID, serverTransactionID);
                                                        break;

                                                    default:
                                                        Return400Error(response, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + CORRECT_SERVER_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                        break;
                                                }
                                                break;
                                            case "PUT": // Write or action methods
                                                switch (elements[URL_ELEMENT_SERVER_COMMAND].ToLowerInvariant())
                                                {
                                                    // MANGEMENT_API_ENABLED removed because it won't work if the management api is disabled
                                                    /*case SharedConstants.MANGEMENT_API_ENABLED:

                                                        string newValueString = suppliedParameters[SharedConstants.MANGEMENT_API_ENABLED];
                                                        LogMessage(clientID, clientTransactionID, serverTransactionID, "PUT " + commandName, newValueString);
                                                        if (bool.TryParse(newValueString, out bool newValue))
                                                        {
                                                            apiIsEnabled = newValue;
                                                            ReturnNoResponse(commandName, request, response, null, clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        else
                                                        {
                                                            Return400Error(response, string.Format("Management {0} command - supplied value '{1}' can not be converted to boolean.", commandName, newValueString), clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        break; */

                                                    // Restart the server by closing current drivers and reloading them
                                                    case SharedConstants.MANGEMENT_RESTART:
                                                        LogMessage(clientID, clientTransactionID, serverTransactionID, "Management Restart", string.Format("Restarting server - getting management lock"));
                                                        lock (managementCommandLock) // Make sure that this command can only run one at a time!
                                                        {
                                                            bool originalAPiIsEnabledState = apiIsEnabled;
                                                            LogMessage(clientID, clientTransactionID, serverTransactionID, "Management Restart", string.Format("Got lock - API is currently enabled: {0}", apiIsEnabled));
                                                            // Shut off access to the device API
                                                            apiIsEnabled = false;
                                                            LogMessage(clientID, clientTransactionID, serverTransactionID, "Management Restart", string.Format("Unloading drivers - devices are connected: {0}", devicesAreConnected));
                                                            if (devicesAreConnected) DisconnectDevices();
                                                            LogMessage(clientID, clientTransactionID, serverTransactionID, "Management Restart", string.Format("Reloading drivers"));
                                                            WaitFor(MANGEMENT_RESTART_WAIT_TIME); // Wait for current device activity to complete
                                                            ConnectDevices();
                                                            apiIsEnabled = originalAPiIsEnabledState;
                                                            LogMessage(clientID, clientTransactionID, serverTransactionID, "Management Restart", string.Format("Restored API enabled state: {0}, command completed", apiIsEnabled));
                                                            ReturnNoResponse(commandName, request, response, null, clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        break;

                                                    case SharedConstants.MANGEMENT_CONFIGURATION:
                                                        lock (managementCommandLock) // Make sure that this command can only run one at a time!
                                                        {
                                                            Return400Error(response, "Command not implemented: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + CORRECT_SERVER_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                        }
                                                        break;

                                                    default:
                                                        Return400Error(response, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + CORRECT_SERVER_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                        break;
                                                }
                                                break;
                                            default:
                                                Return400Error(response, "Unsupported http verb: " + request.HttpMethod, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Return400Error(response, string.Format("Exception processing {0} command \r\n {1}", elements[URL_ELEMENT_SERVER_COMMAND], ex.ToString()), clientID, clientTransactionID, serverTransactionID);
                                    }

                                    break; // End of valid api version numbers
                                default:
                                    Return400Error(response, "Unsupported API version: " + elements[URL_ELEMENT_API_VERSION] + " " + CORRECT_SERVER_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                    break;
                            }
                        }
                    }
                    else // The management interface is not enabled so return an error
                    {
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "Management", MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE);
                        Return403Error(response, MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE, clientID, clientTransactionID, serverTransactionID);
                    }
                }
                else // A URI that did not start with /api/ or /configuration/ was requested
                {
                    LogMessage(clientID, clientTransactionID, serverTransactionID, "Request", string.Format("Non API call - {0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString()));
                    string returnMessage = "You have reached the <i><b>ASCOM Remote Device Server</b></i> API portal<p><b>Available devices:</b></p>";

                    foreach (KeyValuePair<string, ConfiguredDevice> device in ConfiguredDevices)
                    {
                        if (device.Value.ProgID != SharedConstants.DEVICE_NOT_CONFIGURED)
                        {
                            returnMessage += string.Format("{0} {1}: {2} ({3})<br>", device.Value.DeviceType, device.Value.DeviceNumber, device.Value.Description, device.Value.ProgID);
                        }
                    }
                    TransmitResponse("text/html; charset=utf-8", HttpStatusCode.OK, "200 OK", returnMessage, "Request", response, clientID, clientTransactionID, serverTransactionID);
                }
            }

            catch (Exception ex) // Something serious has gone wrong with the ASCOM Rest server itself so report this to the user
            {
                LogException(clientID, clientTransactionID, serverTransactionID, "Request", ex.ToString());
                Return500Error(context.Request, context.Response, "Internal server error: " + ex.ToString(), clientID, clientTransactionID, serverTransactionID);
            }
            finally
            {
                DecrementConcurrencyCounter(); // Decrement the concurrent transactions counter in a thread safe manner
            }
        } // End of ProcessRequest method

        internal void ProcessDriverCommand(int clientID, int clientTransactionID, int serverTransactionID, NameValueCollection suppliedParameters, HttpListenerRequest request, HttpListenerResponse response, string[] elements, string deviceKey)
        {
            try
            {
                // Create three shortcut variables that are local to this thread
                device = ActiveObjects[deviceKey].DeviceObject; // Try and access the device. If it does not exist in the active devices collection then a KeyNotFound exception is generated and handled below
                allowConnectedSetFalse = ActiveObjects[deviceKey].AllowConnectedSetFalse; // If we get here then the user has requested a device that does exist
                allowConnectedSetTrue = ActiveObjects[deviceKey].AllowConnectedSetTrue;

                switch (request.HttpMethod.ToUpperInvariant()) // Handle GET and PUT requests
                {
                    case "GET": // Read and return data methods
                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                        {
                            #region Common methods
                            // Common methods are indicated in ReturnXXX methods by having the device type parameter set to "*" rather than the name of one of the ASCOM device types
                            // STRING Get Values
                            case "description":
                            case "driverinfo":
                            case "driverversion":
                            case "name":
                                ReturnString("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;

                            case "supportedactions":
                                ReturnStringList("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;

                            // SHORT Get Values
                            case "interfaceversion":
                                ReturnShort("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;

                            // BOOL Get Values
                            case "connected":
                                ReturnBool("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            #endregion
                            default: // Not a common method so check for device specific methods
                                switch (elements[URL_ELEMENT_DEVICE_TYPE].ToLowerInvariant())
                                {
                                    case "telescope": // OK so we have a Telescope request
                                        #region Telescope
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            #region Properties
                                            // BOOL Get Values
                                            case "athome":
                                            case "atpark":
                                            case "canfindhome":
                                            case "canslew":
                                            case "cansync":
                                            case "canpark":
                                            case "canpulseguide":
                                            case "cansetdeclinationrate":
                                            case "cansetguiderates":
                                            case "cansetpark":
                                            case "cansetpierside":
                                            case "cansetrightascensionrate":
                                            case "cansettracking":
                                            case "canslewaltaz":
                                            case "canslewaltazasync":
                                            case "canslewasync":
                                            case "cansyncaltaz":
                                            case "canunpark":
                                            case "ispulseguiding":
                                            case "tracking":
                                            case "doesrefraction":
                                            case "slewing":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            // SHORT Get Values
                                            case "slewsettletime":
                                                ReturnShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //DOUBLE Get Values
                                            case "altitude":
                                            case "aperturearea":
                                            case "aperturediameter":
                                            case "azimuth":
                                            case "declination":
                                            case "declinationrate":
                                            case "focallength":
                                            case "guideratedeclination":
                                            case "guideraterightascension":
                                            case "rightascension":
                                            case "rightascensionrate":
                                            case "siteelevation":
                                            case "sitelatitude":
                                            case "sitelongitude":
                                            case "siderealtime":
                                            case "targetdeclination":
                                            case "targetrightascension":
                                                ReturnDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //DATETIME Get values
                                            case "utcdate":
                                                ReturnDateTime(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //ENUM TYPES Get values
                                            case "equatorialsystem":
                                                ReturnEquatorialSystem(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            case "alignmentmode":
                                                ReturnAlignmentMode(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            case "trackingrate":
                                                ReturnTrackingRate(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            case "sideofpier":
                                                ReturnSideofPier(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //TRACKINGRATES Get value
                                            case "trackingrates":
                                                ReturnTrackingRates(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            #endregion

                                            #region Methods
                                            // METHODS
                                            case "axisrates":
                                                ReturnAxisRates(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                            case "destinationsideofpier":
                                                ReturnDestinationSideOfPier(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                            case "canmoveaxis":
                                                ReturnCanMoveAxis(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "camera":
                                        #region Camera
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // SHORT Get Values
                                            case "binx":
                                            case "biny":
                                            case "maxbinx":
                                            case "maxbiny":
                                            case "bayeroffsetx":
                                            case "bayeroffsety":
                                            case "gain":
                                            case "gainmax":
                                            case "gainmin":
                                            case "percentcompleted":
                                            case "readoutmode":
                                                ReturnShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // INT Get Values
                                            case "cameraxsize":
                                            case "cameraysize":
                                            case "maxadu":
                                            case "numx":
                                            case "numy":
                                            case "startx":
                                            case "starty":
                                                ReturnInt(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // BOOL Get Values
                                            case "canabortexposure":
                                            case "canasymmetricbin":
                                            case "cangetcoolerpower":
                                            case "canpulseguide":
                                            case "cansetccdtemperature":
                                            case "canstopexposure":
                                            case "cooleron":
                                            case "hasshutter":
                                            case "imageready":
                                            case "ispulseguiding":
                                            case "canfastreadout":
                                            case "fastreadout":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // DOUBLE Get Values
                                            case "ccdtemperature":
                                            case "coolerpower":
                                            case "electronsperadu":
                                            case "fullwellcapacity":
                                            case "heatsinktemperature":
                                            case "lastexposureduration":
                                            case "pixelsizex":
                                            case "pixelsizey":
                                            case "setccdtemperature":
                                            case "exposuremax":
                                            case "exposuremin":
                                            case "exposureresolution":
                                                ReturnDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //STRING Get Values
                                            case "lastexposurestarttime":
                                            case "sensorname":
                                                ReturnString(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //CAMERSTATES Get Values
                                            case "camerastate":
                                                ReturnCameraStates(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //IMAGEARRAY Get Values
                                            case "imagearray":
                                            case "imagearrayvariant":
                                                ReturnImageArray(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //STRING LIST Get Values
                                            case "gains":
                                            case "readoutmodes":
                                                ReturnStringList(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //SENSORTYPE Get Values
                                            case "sensortype":
                                                ReturnSensorType(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "dome":
                                        #region Dome
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // BOOL Get Values
                                            case "athome":
                                            case "atpark":
                                            case "canfindhome":
                                            case "canpark":
                                            case "cansetaltitude":
                                            case "cansetazimuth":
                                            case "cansetpark":
                                            case "cansetshutter":
                                            case "canslave":
                                            case "cansyncazimuth":
                                            case "slaved":
                                            case "slewing":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // DOUBLE Get Values
                                            case "altitude":
                                            case "azimuth":
                                                ReturnDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            case "shutterstatus": //SensorState
                                                ReturnShutterStatus(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "filterwheel":
                                        #region Filter Wheel
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // INT ARRAY Get Values
                                            case "focusoffsets":
                                                ReturnIntArray(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            // SHORT Get Values
                                            case "position":
                                                ReturnShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //STRING ARRAY Get Values
                                            case "names":
                                                ReturnStringArray(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "focuser":
                                        #region Focuser
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            #region Focuser Properties
                                            // BOOL Get Values
                                            case "absolute":
                                            case "ismoving":
                                            case "tempcompavailable":
                                            case "tempcomp":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            // INT Get Values
                                            case "maxincrement":
                                            case "maxstep":
                                            case "position":
                                                ReturnInt(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //DOUBLE Get Values
                                            case "stepsize":
                                            case "temperature":
                                                ReturnDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "observingconditions":
                                        #region ObservingConditions
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // DOUBLE Get Values
                                            case "averageperiod":
                                            case "cloudcover":
                                            case "dewpoint":
                                            case "humidity":
                                            case "pressure":
                                            case "rainrate":
                                            case "skybrightness":
                                            case "skyquality":
                                            case "skytemperature":
                                            case "starfwhm":
                                            case "temperature":
                                            case "winddirection":
                                            case "windgust":
                                            case "windspeed":
                                            case "timesincelastupdate":
                                                ReturnDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // STRING Get Values
                                            case "sensordescription":
                                                ReturnString(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "rotator":
                                        #region Rotator
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // BOOL Get Values
                                            case "canreverse":
                                            case "ismoving":
                                            case "reverse":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //DOUBLE Get Values
                                            case "position":
                                            case "stepsize":
                                            case "targetposition":
                                                ReturnFloat(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "safetymonitor":
                                        #region SafetyMonitor
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // BOOL Get Values
                                            case "issafe":
                                                ReturnBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "switch":
                                        #region Switch
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // BOOL Get Values
                                            case "canwrite":
                                            case "getswitch":
                                                ReturnShortIndexedBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // SHORT Get Values
                                            case "maxswitch":
                                                ReturnShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // DOUBLE Get Values
                                            case "getswitchvalue":
                                            case "maxswitchvalue":
                                            case "minswitchvalue":
                                            case "switchstep":
                                                ReturnShortIndexedDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            // STRING Get Values
                                            case "getswitchdescription":
                                            case "getswitchname":
                                                ReturnShortIndexedString(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, GET_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion

                                    default:// End of valid device types
                                        Return400Error(response, "Unsupported Device Type: " + elements[URL_ELEMENT_DEVICE_TYPE] + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                        break;
                                }
                                break;
                        }
                        break;
                    case "PUT": // Write or action methods
                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                        {
                            #region Common methods
                            // Process common methods shared by all drivers
                            case "commandblind":
                                CallMethod("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            case "commandbool":
                                ReturnBool("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            case "commandstring":
                                ReturnString("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            case "action":
                                ReturnString("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            case "connected":
                                WriteBool("*", elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                break;
                            #endregion
                            default:
                                switch (elements[URL_ELEMENT_DEVICE_TYPE].ToLowerInvariant())
                                {
                                    case "telescope": // OK so we have a Telescope request
                                        #region Telescope
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            #region Telescope Properties
                                            //BOOL Set values
                                            case "tracking":
                                            case "doesrefraction":
                                                WriteBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //SHORT Set Values
                                            case "slewsettletime":
                                                WriteShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //DOUBLE Set values
                                            case "declinationrate":
                                            case "rightascensionrate":
                                            case "guideratedeclination":
                                            case "guideraterightascension":
                                            case "siteelevation":
                                            case "sitelatitude":
                                            case "sitelongitude":
                                            case "targetdeclination":
                                            case "targetrightascension":
                                                WriteDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //DATETIME Set values
                                            case "utcdate":
                                                WriteDateTime(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //ENUM TYPES Set values
                                            case "trackingrate":
                                                WriteTrackingRate(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            #endregion

                                            #region Telescope Methods
                                            // METHODS
                                            case "sideofpier":
                                            case "unpark":
                                            case "park":
                                            case "abortslew":
                                            case "findhome":
                                            case "setpark":
                                            case "slewtotarget":
                                            case "slewtotargetasync":
                                            case "synctotarget":
                                            case "slewtocoordinates":
                                            case "slewtocoordinatesasync":
                                            case "slewtoaltaz":
                                            case "slewtoaltazasync":
                                            case "synctoaltaz":
                                            case "synctocoordinates":
                                            case "moveaxis":
                                            case "pulseguide":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "camera":
                                        #region Camera
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // METHODS
                                            case "abortexposure":
                                            case "pulseguide":
                                            case "startexposure":
                                            case "stopexposure":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //SHORT Set values
                                            case "binx":
                                            case "biny":
                                            case "gain":
                                            case "readoutmode":
                                                WriteShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //INT Set values
                                            case "numx":
                                            case "numy":
                                            case "startx":
                                            case "starty":
                                                WriteInt(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //DOUBLE Set values
                                            case "setccdtemperature":
                                                WriteDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //BOOL Set values
                                            case "cooleron":
                                            case "fastreadout":
                                                WriteBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "dome":
                                        #region Dome
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // METHODS
                                            case "abortslew":
                                            case "closeshutter":
                                            case "findhome":
                                            case "openshutter":
                                            case "park":
                                            case "setpark":
                                            case "slewtoaltitude":
                                            case "slewtoazimuth":
                                            case "synctoazimuth":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //BOOL Set values
                                            case "slaved":
                                                WriteBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "filterwheel":
                                        #region Filter Wheel
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            //SHORT Set values
                                            case "position":
                                                WriteShort(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "focuser":
                                        #region Focuser
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            #region Focuser Properties
                                            //BOOL Set values
                                            case "tempcomp":
                                                WriteBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            #endregion

                                            #region Focuser Methods
                                            // METHODS
                                            case "halt":
                                            case "move":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "observingconditions":
                                        #region ObservingConditions
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // METHODS
                                            case "refresh":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //DOUBLE Set values
                                            case "averageperiod":
                                                WriteDouble(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "rotator":
                                        #region Rotator
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // METHODS
                                            case "halt":
                                            case "move":
                                            case "moveabsolute":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;
                                            //BOOL Set values
                                            case "reverse":
                                                WriteBool(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "switch":
                                        #region Switch
                                        switch (elements[URL_ELEMENT_METHOD].ToLowerInvariant())
                                        {
                                            // METHODS
                                            case "setswitchname":
                                            case "setswitch":
                                            case "setswitchvalue":
                                                CallMethod(elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_METHOD], request, response, suppliedParameters, clientID, clientTransactionID, serverTransactionID); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(response, PUT_UNKNOWN_METHOD_MESSAGE + elements[URL_ELEMENT_METHOD].ToLowerInvariant() + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion

                                    default:
                                        Return400Error(response, "Unsupported Device Type: " + elements[URL_ELEMENT_DEVICE_TYPE] + " " + CORRECT_API_FORMAT_STRING, clientID, clientTransactionID, serverTransactionID);
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        Return400Error(response, "Unsupported http verb: " + request.HttpMethod, clientID, clientTransactionID, serverTransactionID);
                        break;
                }
            }
            catch (KeyNotFoundException)
            {
                Return400Error(response, string.Format("The requested device \"{0} {1}\" does not exist on this server. Supplied URI: {2} {3}", elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_DEVICE_NUMBER], request.Url.AbsolutePath, CORRECT_API_FORMAT_STRING), clientID, clientTransactionID, serverTransactionID);
            }
        }

        #endregion

        #region API response methods

        private void Return400Error(HttpListenerResponse response, string message, int clientID, int clientTransactionID, int serverTransactionID)
        {
            LogMessage(clientID, clientTransactionID, serverTransactionID, "HTTP 400 Error", message);
            LogToScreen(string.Format("ERROR - ClientId: {0}, ClientTransactionID: {1} - {2}", clientID, clientTransactionID, message));

            TransmitResponse("text/html; charset=utf-8", HttpStatusCode.BadRequest, "400 " + CleanMessage(message), "400 " + CleanMessage(message), "Request", response, clientID, clientTransactionID, serverTransactionID);
        }
        private void Return403Error(HttpListenerResponse response, string message, int clientID, int clientTransactionID, int serverTransactionID)
        {
            LogMessage(clientID, clientTransactionID, serverTransactionID, "HTTP 403 Error", message);
            LogToScreen(string.Format("ERROR - ClientId: {0}, ClientTransactionID: {1} - {2}", clientID, clientTransactionID, message));

            TransmitResponse("text/html; charset=utf-8", HttpStatusCode.Forbidden, "403 " + CleanMessage(message), "403 " + CleanMessage(message), "Request", response, clientID, clientTransactionID, serverTransactionID);
        }
        internal void Return500Error(HttpListenerRequest request, HttpListenerResponse response, string errorMessage, int clientID, int clientTransactionID, int serverTransactionID)
        {
            LogMessage(clientID, clientTransactionID, serverTransactionID, "HTTP 500 Error", errorMessage);

            TransmitResponse("text/html; charset=utf-8", HttpStatusCode.InternalServerError, "500 " + CleanMessage(errorMessage), "500 " + CleanMessage(errorMessage), "Request", response, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnBool(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            bool deviceResponse = false;
            string command;
            bool raw;
            Exception exReturn = null;

            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    #region Common methods
                    case "*.connected":
                        deviceResponse = device.Connected; break;
                    #endregion

                    #region Telescope Methods
                    case "*.commandbool":
                        command = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RAW_PARAMETER_NAME);
                        deviceResponse = device.CommandBool(command, raw);
                        break;
                    case "telescope.athome":
                        deviceResponse = device.AtHome; break;
                    case "telescope.atpark":
                        deviceResponse = device.AtPark; break;
                    case "telescope.canfindhome":
                        deviceResponse = device.CanFindHome; break;
                    case "telescope.canslew":
                        deviceResponse = device.CanSlew; break;
                    case "telescope.cansync":
                        deviceResponse = device.CanSync; break;
                    case "telescope.canpark":
                        deviceResponse = device.CanPark; break;
                    case "telescope.canpulseguide":
                        deviceResponse = device.CanPulseGuide; break;
                    case "telescope.cansetdeclinationrate":
                        deviceResponse = device.CanSetDeclinationRate; break;
                    case "telescope.cansetguiderates":
                        deviceResponse = device.CanSetGuideRates; break;
                    case "telescope.cansetpark":
                        deviceResponse = device.CanSetPark; break;
                    case "telescope.cansetpierside":
                        deviceResponse = device.CanSetPierSide; break;
                    case "telescope.cansetrightascensionrate":
                        deviceResponse = device.CanSetRightAscensionRate; break;
                    case "telescope.cansettracking":
                        deviceResponse = device.CanSetTracking; break;
                    case "telescope.canslewaltaz":
                        deviceResponse = device.CanSlewAltAz; break;
                    case "telescope.canslewaltazasync":
                        deviceResponse = device.CanSlewAltAzAsync; break;
                    case "telescope.canslewasync":
                        deviceResponse = device.CanSlewAsync; break;
                    case "telescope.cansyncaltaz":
                        deviceResponse = device.CanSyncAltAz; break;
                    case "telescope.canunpark":
                        deviceResponse = device.CanUnpark; break;
                    case "telescope.ispulseguiding":
                        deviceResponse = device.IsPulseGuiding; break;
                    case "telescope.tracking":
                        deviceResponse = device.Tracking; break;
                    case "telescope.doesrefraction":
                        deviceResponse = device.DoesRefraction; break;
                    case "telescope.slewing":
                        deviceResponse = device.Slewing; break;
                    #endregion

                    #region Camera Methods

                    case "camera.canabortexposure":
                        deviceResponse = device.CanAbortExposure; break;
                    case "camera.canasymmetricbin":
                        deviceResponse = device.CanAsymmetricBin; break;
                    case "camera.cangetcoolerpower":
                        deviceResponse = device.CanGetCoolerPower; break;
                    case "camera.canpulseguide":
                        deviceResponse = device.CanPulseGuide; break;
                    case "camera.cansetccdtemperature":
                        deviceResponse = device.CanSetCCDTemperature; break;
                    case "camera.canstopexposure":
                        deviceResponse = device.CanStopExposure; break;
                    case "camera.cooleron":
                        deviceResponse = device.CoolerOn; break;
                    case "camera.hasshutter":
                        deviceResponse = device.HasShutter; break;
                    case "camera.imageready":
                        deviceResponse = device.ImageReady; break;
                    case "camera.ispulseguiding":
                        deviceResponse = device.IsPulseGuiding; break;
                    case "camera.canfastreadout":
                        deviceResponse = device.CanFastReadout; break;
                    case "camera.fastreadout":
                        deviceResponse = device.FastReadout; break;

                    #endregion

                    #region Dome Methods

                    case "dome.athome":
                        deviceResponse = device.AtHome; break;
                    case "dome.atpark":
                        deviceResponse = device.AtPark; break;
                    case "dome.canfindhome":
                        deviceResponse = device.CanFindHome; break;
                    case "dome.canpark":
                        deviceResponse = device.CanPark; break;
                    case "dome.cansetaltitude":
                        deviceResponse = device.CanSetAltitude; break;
                    case "dome.cansetazimuth":
                        deviceResponse = device.CanSetAzimuth; break;
                    case "dome.cansetpark":
                        deviceResponse = device.CanSetPark; break;
                    case "dome.cansetshutter":
                        deviceResponse = device.CanSetShutter; break;
                    case "dome.canslave":
                        deviceResponse = device.CanSlave; break;
                    case "dome.cansyncazimuth":
                        deviceResponse = device.CanSyncAzimuth; break;
                    case "dome.slaved":
                        deviceResponse = device.Slaved; break;
                    case "dome.slewing":
                        deviceResponse = device.Slewing; break;

                    #endregion

                    #region Focuser methods
                    case "focuser.absolute":
                        deviceResponse = device.Absolute; break;
                    case "focuser.ismoving":
                        deviceResponse = device.IsMoving; break;
                    case "focuser.tempcompavailable":
                        deviceResponse = device.TempCompAvailable; break;
                    case "focuser.tempcomp":
                        deviceResponse = device.TempComp; break;

                    #endregion

                    #region Rotator Methods

                    case "rotator.canreverse":
                        deviceResponse = device.CanReverse; break;
                    case "rotator.ismoving":
                        deviceResponse = device.IsMoving; break;
                    case "rotator.reverse":
                        deviceResponse = device.Reverse; break;

                    #endregion

                    #region Safetymonitor Methods

                    case "safetymonitor.issafe":
                        deviceResponse = device.IsSafe; break;

                    #endregion

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnBool", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnBool - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            BoolResponse responseClass = new BoolResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnShortIndexedBool(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            bool deviceResponse = false;
            Exception exReturn = null;


            try
            {
                short index = GetParameter<short>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);

                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    #region Switch Methods

                    case "switch.canwrite":
                        deviceResponse = device.CanWrite(index); break;
                    case "switch.getswitch":
                        deviceResponse = device.GetSwitch(index); break;

                    #endregion

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnShortIndexedBool", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnShortIndexedBool - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            BoolResponse responseClass = new BoolResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void WriteBool(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            bool boolValue = false;
            Exception exReturn = null;

            try
            {
                boolValue = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, method);
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // COMMON METHODS
                    case "*.connected":
                        if (boolValue == true) // We are being asked to set Connected to True
                        {
                            if (allowConnectedSetTrue)
                            {
                                LogMessage(clientID, clientTransactionID, serverTransactionID, "Connected Set True", "Changing Connected state because the \"Allow Connected Set True\" setting is true");
                                device.Connected = boolValue;
                            }
                            else
                            {
                                LogMessage(clientID, clientTransactionID, serverTransactionID, "Connected Set True", "Ignoring Connected state change because the \"Allow Connected Set True\" setting is false");
                            }
                        }
                        else // We are being asked to set Connected to False
                        {
                            if (allowConnectedSetFalse)
                            {
                                LogMessage(clientID, clientTransactionID, serverTransactionID, "Connected Set False", "Changing Connected state because the \"Allow Connected Set False\" setting is true");
                                device.Connected = boolValue;
                            }
                            else
                            {
                                LogMessage(clientID, clientTransactionID, serverTransactionID, "Connected Set False", "Ignoring Connected state change because the \"Allow Connected Set False\" setting is false");
                            }
                        }
                        break;
                    // TELESCOPE
                    case "telescope.tracking":
                        device.Tracking = boolValue; break;
                    case "telescope.doesrefraction":
                        device.DoesRefraction = boolValue; break;

                    // CAMERA
                    case "camera.cooleron":
                        device.CoolerOn = boolValue; break;
                    case "camera.fastreadout":
                        device.FastReadout = boolValue; break;

                    // DOME
                    case "dome.slaved":
                        device.Slaved = boolValue; break;

                    //FOCUSER 
                    case "focuser.tempcomp":
                        device.TempComp = boolValue; break;

                    //ROTATOR
                    case "rotator.reverse":
                        device.Reverse = boolValue; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "WriteBool", "Unsupported method: " + method);
                        throw new InvalidValueException("WriteBool - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnString(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            string deviceResponse = "";
            string command;
            string parameters;
            bool raw;
            Exception exReturn = null;

            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // COMMON METHODS
                    case "*.description":
                        deviceResponse = device.Description; break;
                    case "*.driverinfo":
                        deviceResponse = device.DriverInfo; break;
                    case "*.driverversion":
                        deviceResponse = device.DriverVersion; break;
                    case "*.name":
                        deviceResponse = device.Name; break;
                    case "*.commandstring":
                        command = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RAW_PARAMETER_NAME);
                        deviceResponse = device.CommandString(command, raw);
                        break;
                    case "*.action":
                        command = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ACTION_COMMAND_PARAMETER_NAME);
                        parameters = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ACTION_PARAMETERS_PARAMETER_NAME);
                        deviceResponse = device.Action(command, parameters);
                        break;
                    // CAMERA
                    case "camera.lastexposurestarttime":
                        deviceResponse = device.LastExposureStartTime; break;
                    case "camera.sensorname":
                        deviceResponse = device.SensorName; break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.sensordescription":
                        string stringParamValue = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.SENSORNAME_PARAMETER_NAME);
                        deviceResponse = device.SensorDescription(stringParamValue); break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnString", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnString - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringResponse responseClass = new StringResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnShortIndexedString(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            string deviceResponse = "";
            Exception exReturn = null;


            try
            {
                short index = GetParameter<short>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);

                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    #region Switch Methods

                    case "switch.getswitchdescription":
                        deviceResponse = device.GetSwitchDescription(index); break;
                    case "switch.getswitchname":
                        deviceResponse = device.GetSwitchName(index); break;

                    #endregion

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnShortIndexedString", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnShortIndexedString - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringResponse responseClass = new StringResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnStringArray(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            string[] deviceResponse = new string[1];
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // FILTER WHEEL
                    case "filterwheel.names":
                        deviceResponse = device.Names; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnStringArray", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnStringArray - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringArrayResponse responseClass = new StringArrayResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnStringList(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            ArrayList deviceResponse = new ArrayList();
            List<string> responseList = new List<string>();
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    case "*.supportedactions":
                        deviceResponse = (ArrayList)device.SupportedActions;
                        foreach (string action in deviceResponse)
                        {
                            responseList.Add(action);
                        }

                        break;
                    case "camera.gains":
                        deviceResponse = (ArrayList)device.Gains;
                        foreach (string gain in deviceResponse)
                        {
                            responseList.Add(gain);
                        }
                        break;
                    case "camera.readoutmodes":
                        deviceResponse = (ArrayList)device.ReadoutModes;
                        foreach (string mode in deviceResponse)
                        {
                            responseList.Add(mode);
                        }
                        break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnStringList", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnStringList - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringListResponse responseClass = new StringListResponse(clientTransactionID, serverTransactionID, method, responseList)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnDouble(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // TELESCOPE
                    case "telescope.altitude":
                        deviceResponse = device.Altitude; break;
                    case "telescope.aperturearea":
                        deviceResponse = device.ApertureArea; break;
                    case "telescope.aperturediameter":
                        deviceResponse = device.ApertureDiameter; break;
                    case "telescope.azimuth":
                        deviceResponse = device.Azimuth; break;
                    case "telescope.declination":
                        deviceResponse = device.Declination; break;
                    case "telescope.declinationrate":
                        deviceResponse = device.DeclinationRate; break;
                    case "telescope.focallength":
                        deviceResponse = device.FocalLength; break;
                    case "telescope.guideratedeclination":
                        deviceResponse = device.GuideRateDeclination; break;
                    case "telescope.guideraterightascension":
                        deviceResponse = device.GuideRateRightAscension; break;
                    case "telescope.rightascension":
                        deviceResponse = device.RightAscension; break;
                    case "telescope.rightascensionrate":
                        deviceResponse = device.RightAscensionRate; break;
                    case "telescope.siteelevation":
                        deviceResponse = device.SiteElevation; break;
                    case "telescope.sitelatitude":
                        deviceResponse = device.SiteLatitude; break;
                    case "telescope.sitelongitude":
                        deviceResponse = device.SiteLongitude; break;
                    case "telescope.siderealtime":
                        deviceResponse = device.SiderealTime; break;
                    case "telescope.targetdeclination":
                        deviceResponse = device.TargetDeclination; break;
                    case "telescope.targetrightascension":
                        deviceResponse = device.TargetRightAscension; break;
                    // FOCUSER
                    case "focuser.stepsize":
                        deviceResponse = device.StepSize; break;
                    case "focuser.temperature":
                        deviceResponse = device.Temperature; break;
                    // CAMERA
                    case "camera.ccdtemperature":
                        deviceResponse = device.CCDTemperature; break;
                    case "camera.coolerpower":
                        deviceResponse = device.CoolerPower; break;
                    case "camera.electronsperadu":
                        deviceResponse = device.ElectronsPerADU; break;
                    case "camera.fullwellcapacity":
                        deviceResponse = device.FullWellCapacity; break;
                    case "camera.heatsinktemperature":
                        deviceResponse = device.HeatSinkTemperature; break;
                    case "camera.lastexposureduration":
                        deviceResponse = device.LastExposureDuration; break;
                    case "camera.pixelsizex":
                        deviceResponse = device.PixelSizeX; break;
                    case "camera.pixelsizey":
                        deviceResponse = device.PixelSizeY; break;
                    case "camera.setccdtemperature":
                        deviceResponse = device.SetCCDTemperature; break;
                    case "camera.exposuremax":
                        deviceResponse = device.ExposureMax; break;
                    case "camera.exposuremin":
                        deviceResponse = device.ExposureMin; break;
                    case "camera.exposureresolution":
                        deviceResponse = device.ExposureResolution; break;
                    // DOME
                    case "dome.altitude":
                        deviceResponse = device.Altitude; break;
                    case "dome.azimuth":
                        deviceResponse = device.Azimuth; break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.averageperiod":
                        deviceResponse = device.AveragePeriod; break;
                    case "observingconditions.cloudcover":
                        deviceResponse = device.CloudCover; break;
                    case "observingconditions.dewpoint":
                        deviceResponse = device.DewPoint; break;
                    case "observingconditions.humidity":
                        deviceResponse = device.Humidity; break;
                    case "observingconditions.pressure":
                        deviceResponse = device.Pressure; break;
                    case "observingconditions.rainrate":
                        deviceResponse = device.RainRate; break;
                    case "observingconditions.skybrightness":
                        deviceResponse = device.SkyBrightness; break;
                    case "observingconditions.skyquality":
                        deviceResponse = device.SkyQuality; break;
                    case "observingconditions.skytemperature":
                        deviceResponse = device.SkyTemperature; break;
                    case "observingconditions.starfwhm":
                        deviceResponse = device.StarFWHM; break;
                    case "observingconditions.temperature":
                        deviceResponse = device.Temperature; break;
                    case "observingconditions.winddirection":
                        deviceResponse = device.WindDirection; break;
                    case "observingconditions.windgust":
                        deviceResponse = device.WindGust; break;
                    case "observingconditions.windspeed":
                        deviceResponse = device.WindSpeed; break;
                    case "observingconditions.timesincelastupdate": // This is actually a function, hence the parameter retrieval below
                        string stringParamValue = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.SENSORNAME_PARAMETER_NAME);
                        deviceResponse = device.TimeSinceLastUpdate(stringParamValue); break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnDouble", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnDouble - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnShortIndexedDouble(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;


            try
            {
                short index = GetParameter<short>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);

                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    #region Switch Methods

                    case "switch.getswitchvalue":
                        deviceResponse = device.GetSwitchValue(index); break;
                    case "switch.maxswitchvalue":
                        deviceResponse = device.MaxSwitchValue(index); break;
                    case "switch.minswitchvalue":
                        deviceResponse = device.MinSwitchValue(index); break;
                    case "switch.switchstep":
                        deviceResponse = device.SwitchStep(index); break;

                    #endregion

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnShortIndexedDouble", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnShortIndexedDouble - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void WriteDouble(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            double doubleValue = 0;
            Exception exReturn = null;

            try
            {
                doubleValue = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, method);
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    case "telescope.declinationrate":
                        device.DeclinationRate = doubleValue; break;
                    case "telescope.rightascensionrate":
                        device.RightAscensionRate = doubleValue; break;
                    case "telescope.guideratedeclination":
                        device.GuideRateDeclination = doubleValue; break;
                    case "telescope.guideraterightascension":
                        device.GuideRateRightAscension = doubleValue; break;
                    case "telescope.siteelevation":
                        device.SiteElevation = doubleValue; break;
                    case "telescope.sitelatitude":
                        device.SiteLatitude = doubleValue; break;
                    case "telescope.sitelongitude":
                        device.SiteLongitude = doubleValue; break;
                    case "telescope.targetdeclination":
                        device.TargetDeclination = doubleValue; break;
                    case "telescope.targetrightascension":
                        device.TargetRightAscension = doubleValue; break;
                    // CAMERA
                    case "camera.setccdtemperature":
                        device.SetCCDTemperature = doubleValue; break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.averageperiod":
                        device.AveragePeriod = doubleValue; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "WriteDouble", "Unsupported method: " + method);
                        throw new InvalidValueException("WriteDouble - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnFloat(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // ROTATOR
                    case "rotator.position":
                        deviceResponse = (double)device.Position; break;
                    case "rotator.stepsize":
                        deviceResponse = (double)device.StepSize; break;
                    case "rotator.targetposition":
                        deviceResponse = (double)device.TargetPosition; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnFloat", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnFloat - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnShort(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            short deviceResponse = 0;
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // COMMON METHODS
                    case "*.interfaceversion":
                        deviceResponse = device.InterfaceVersion; break;
                    // TELESCOPE
                    case "telescope.slewsettletime":
                        deviceResponse = device.SlewSettleTime; break;
                    // FILTER WHEEL
                    case "filterwheel.position":
                        deviceResponse = device.Position; break;
                    // CAMERA
                    case "camera.binx":
                        deviceResponse = device.BinX; break;
                    case "camera.biny":
                        deviceResponse = device.BinY; break;
                    case "camera.maxbinx":
                        deviceResponse = device.MaxBinX; break;
                    case "camera.maxbiny":
                        deviceResponse = device.MaxBinY; break;
                    case "camera.bayeroffsetx":
                        deviceResponse = device.BayerOffsetX; break;
                    case "camera.bayeroffsety":
                        deviceResponse = device.BayerOffsetY; break;
                    case "camera.gain":
                        deviceResponse = device.Gain; break;
                    case "camera.gainmax":
                        deviceResponse = device.GainMax; break;
                    case "camera.gainmin":
                        deviceResponse = device.GainMin; break;
                    case "camera.percentcompleted":
                        deviceResponse = device.PercentCompleted; break;
                    case "camera.readoutmode":
                        deviceResponse = device.ReadoutMode; break;
                    // SWITCH
                    case "switch.maxswitch":
                        deviceResponse = device.MaxSwitch; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnShort", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnShort - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            ShortResponse responseClass = new ShortResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void WriteShort(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            short shortValue = 0;
            Exception exReturn = null;


            try
            {
                shortValue = GetParameter<short>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, method);
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // TELESCOPE
                    case "telescope.slewsettletime":
                        device.SlewSettleTime = shortValue;
                        break;
                    // FILTER WHEEL
                    case "filterwheel.position":
                        device.Position = shortValue; break;
                    // CAMERA
                    case "camera.binx":
                        device.BinX = shortValue; break;
                    case "camera.biny":
                        device.BinY = shortValue; break;
                    case "camera.gain":
                        device.Gain = shortValue; break;
                    case "camera.readoutmode":
                        device.ReadoutMode = shortValue; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "WriteShort", "Unsupported method: " + method);
                        throw new InvalidValueException("WriteShort - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnInt(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            int deviceResponse = 0;
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // FOCUSER
                    case "focuser.maxincrement":
                        deviceResponse = device.MaxIncrement; break;
                    case "focuser.maxstep":
                        deviceResponse = device.MaxStep; break;
                    case "focuser.position":
                        deviceResponse = device.Position; break;
                    //CAMERA
                    case "camera.cameraxsize":
                        deviceResponse = device.CameraXSize; break;
                    case "camera.cameraysize":
                        deviceResponse = device.CameraYSize; break;
                    case "camera.maxadu":
                        deviceResponse = device.MaxADU; break;
                    case "camera.numx":
                        deviceResponse = device.NumX; break;
                    case "camera.numy":
                        deviceResponse = device.NumY; break;
                    case "camera.startx":
                        deviceResponse = device.StartX; break;
                    case "camera.starty":
                        deviceResponse = device.StartY; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnInt", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnInt - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void ReturnIntArray(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            int[] deviceResponse = new int[1];
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // FILTER WHEEL
                    case "filterwheel.focusoffsets":
                        deviceResponse = device.FocusOffsets; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnIntArray", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnIntArray - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntArray1DResponse responseClass = new IntArray1DResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void WriteInt(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            int intValue = 0;
            Exception exReturn = null;


            try
            {
                intValue = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, method);
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // CAMERA
                    case "camera.numx":
                        device.NumX = intValue; break;
                    case "camera.numy":
                        device.NumY = intValue; break;
                    case "camera.startx":
                        device.StartX = intValue; break;
                    case "camera.starty":
                        device.StartY = intValue; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "WriteInt", "Unsupported method: " + method);
                        throw new InvalidValueException("WriteInt - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnDateTime(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            DateTime deviceResponse = DateTime.Now;
            Exception exReturn = null;


            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    case "telescope.utcdate":
                        deviceResponse = device.UTCDate; break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnDateTime", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnDateTime - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DateTimeResponse responseClass = new DateTimeResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);

        }
        private void WriteDateTime(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            DateTime dateTimeValue = DateTime.Now;
            Exception exReturn = null;


            try
            {
                dateTimeValue = GetParameter<DateTime>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.UTCDATE_PARAMETER_NAME);
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, "Converted DateTime value (UTC): " + dateTimeValue.ToUniversalTime().ToString(SharedConstants.ISO8601_DATE_FORMAT_STRING));
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    case "telescope.utcdate":
                        device.UTCDate = dateTimeValue.ToUniversalTime(); break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "WriteDateTime", "Unsupported method: " + method);
                        throw new InvalidValueException("WriteDateTime - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnTrackingRates(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            System.Collections.IEnumerable deviceResponse = null;
            Exception exReturn = null;


            try
            {
                deviceResponse = (System.Collections.IEnumerable)device.TrackingRates;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            TrackingRatesResponse responseClass = new TrackingRatesResponse(clientTransactionID, serverTransactionID, method)
            {
                DriverException = exReturn
            };

            List<DriveRates> rates = new List<DriveRates>();
            foreach (DriveRates rate in deviceResponse)
            {
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Rate = {0}", rate.ToString()));
                rates.Add(rate);
            }

            LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Number of rates: {0}", rates.Count));
            responseClass.Rates = rates;
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);

        }

        private void ReturnEquatorialSystem(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            EquatorialCoordinateType deviceResponse = EquatorialCoordinateType.equTopocentric;
            Exception exReturn = null;

            try
            {
                deviceResponse = (EquatorialCoordinateType)device.EquatorialSystem;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);

        }

        private void ReturnAlignmentMode(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            AlignmentModes deviceResponse = AlignmentModes.algGermanPolar;
            Exception exReturn = null;


            try
            {
                deviceResponse = (AlignmentModes)device.AlignmentMode;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnTrackingRate(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            DriveRates deviceResponse = DriveRates.driveSidereal;
            Exception exReturn = null;


            try
            {
                deviceResponse = (DriveRates)device.TrackingRate;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }
        private void WriteTrackingRate(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            DriveRates driveRateValue = DriveRates.driveSidereal;
            Exception exReturn = null;


            try
            {
                driveRateValue = (DriveRates)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, method);
                device.TrackingRate = driveRateValue;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnSideofPier(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            PierSide deviceResponse = PierSide.pierUnknown;
            Exception exReturn = null;


            try
            {
                deviceResponse = (PierSide)device.SideOfPier;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnCameraStates(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            CameraStates deviceResponse = CameraStates.cameraIdle;
            Exception exReturn = null;


            try
            {
                deviceResponse = (CameraStates)device.CameraState;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnShutterStatus(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            ShutterState deviceResponse = ShutterState.shutterError;
            Exception exReturn = null;


            try
            {
                deviceResponse = (ShutterState)device.ShutterStatus;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnSensorType(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            SensorType deviceResponse = SensorType.CMYG2;
            Exception exReturn = null;


            try
            {
                deviceResponse = (SensorType)device.SensorType;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnImageArray(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {

            Array deviceResponse = null;
            //ImageArrayResponse 
            dynamic responseClass = new IntArray2DResponse(clientTransactionID, serverTransactionID, method); // Initialise here so that there is a class ready to convey back an error message
            Exception exReturn = null;

            // Create small 3D test array to illustrate the order of elements in the serialised JSON array.

            int[,,] testArray3DInt = new int[3, 3, 3];
            int[,] testArray2DInt = new int[3, 3];
            bool returnTestResponse = false; // Set to true to return the test response, false to return the camera response

            for (int k = 0; k <= testArray3DInt.GetUpperBound(2); k++)
            {
                for (int j = 0; j <= testArray3DInt.GetUpperBound(1); j++)
                {
                    for (int i = 0; i <= testArray3DInt.GetUpperBound(0); i++)
                    {
                        testArray2DInt[i, j] = i + (10 * j);
                        testArray3DInt[i, j, k] = i + (10 * j) + (100 * k);
                    } // End of i
                } // End of j
            } // End of k

            try
            {
                switch (method.ToLowerInvariant())
                {
                    case "imagearray":
                        deviceResponse = device.ImageArray;
                        if (deviceResponse != null)
                        {
                            LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("ImageArray Rank: {0}", deviceResponse.Rank));
                            LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("ImageArray Rank: {0}, Length: {1:n0}", deviceResponse.Rank, deviceResponse.Length));

                            switch (deviceResponse.Rank)
                            {
                                case 2:
                                    responseClass = new IntArray2DResponse(clientTransactionID, serverTransactionID, method);

                                    if (returnTestResponse) responseClass.Value = testArray2DInt;
                                    else responseClass.Value = (int[,])deviceResponse;
                                    break;
                                case 3:
                                    responseClass = new IntArray3DResponse(clientTransactionID, serverTransactionID, method);

                                    if (returnTestResponse) responseClass.Value = testArray3DInt;
                                    else responseClass.Value = (int[,,])deviceResponse;
                                    break;
                                default:
                                    throw new InvalidParameterException("ReturnImageArray received array of Rank " + deviceResponse.Rank + ", this is not currently supported.");
                            }
                        }
                        break;
                    case "imagearrayvariant":
                        deviceResponse = device.ImageArrayVariant;
                        string arrayType = deviceResponse.GetType().Name;
                        string elementType = "";
                        LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Received array of Rank {0} of Length {1:n0} with element type {2} {3}", deviceResponse.Rank, deviceResponse.Length, deviceResponse.GetType().Name, deviceResponse.GetType().UnderlyingSystemType.Name));

                        switch (arrayType) // Process 2D and 3D variant arrays, all other types are unsupported
                        {
                            case "Object[,]": // 2D Array
                                elementType = deviceResponse.GetValue(0, 0).GetType().Name;
                                break;
                            case "Object[,,]":
                                elementType = deviceResponse.GetValue(0, 0, 0).GetType().Name;
                                break;
                            default:
                                throw new InvalidValueException("ReturnImageArray: Received an unsupported return array type: " + arrayType);
                        }
                        LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Array elements are of type: {0}", elementType));

                        switch (deviceResponse.Rank)
                        {
                            case 2:
                                switch (elementType)
                                {

                                    case "Int16":
                                        responseClass = new ShortArray2DResponse(clientTransactionID, serverTransactionID, method, Library.Array2DToShort(deviceResponse));
                                        break;
                                    case "Int32":
                                        responseClass = new IntArray2DResponse(clientTransactionID, serverTransactionID, method);

                                        if (returnTestResponse) responseClass.Value = testArray2DInt;
                                        else responseClass.Value = Library.Array2DToInt(deviceResponse);
                                        break;
                                    case "Double":
                                        responseClass = new DoubleArray2DResponse(clientTransactionID, serverTransactionID, method, Library.Array2DToDouble(deviceResponse));
                                        break;
                                    default:
                                        throw new InvalidValueException("ReturnImageArray: Received an unsupported return array type: " + arrayType + ", with elements of type: " + elementType);
                                }
                                break;
                            case 3:
                                switch (elementType)
                                {
                                    case "Int16":
                                        responseClass = new ShortArray3DResponse(clientTransactionID, serverTransactionID, method, Library.Array3DToShort(deviceResponse));
                                        break;
                                    case "Int32":
                                        responseClass = new IntArray3DResponse(clientTransactionID, serverTransactionID, method);

                                        if (returnTestResponse) responseClass.Value = testArray3DInt;
                                        else responseClass.Value = Library.Array3DToInt(deviceResponse);
                                        break;
                                    case "Double":
                                        responseClass = new DoubleArray3DResponse(clientTransactionID, serverTransactionID, method, Library.Array3DToDouble(deviceResponse));
                                        break;
                                    default:
                                        throw new InvalidValueException("ReturnImageArray: Received an unsupported return array type: " + arrayType + ", with elements of type: " + elementType);
                                }
                                break;
                            default:
                                throw new InvalidParameterException("Received array of Rank " + deviceResponse.Rank + ", this is not currently supported.");
                        }
                        break;
                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "ReturnImageArray", "Unsupported method: " + method);
                        throw new InvalidValueException("ReturnImageArray - Unsupported method: " + method);
                }

            }
            catch (Exception ex)
            {
                LogException(clientID, clientTransactionID, serverTransactionID, "ReturnImageArray Exception", ex.ToString());
                exReturn = ex;
            }

            responseClass.DriverException = exReturn;
            if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, "Starting array serialisation");

            string responseJson = JsonConvert.SerializeObject(responseClass);
            if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Completed array serialisation, Length: {0:n0}", responseJson.Length));

            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnCanMoveAxis(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            bool deviceResponse = false;
            Exception exReturn = null;


            try
            {
                TelescopeAxes axis = (TelescopeAxes)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AXIS_PARAMETER_NAME);
                deviceResponse = device.CanMoveAxis(axis);
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }
            BoolResponse responseClass = new BoolResponse(clientTransactionID, serverTransactionID, method, deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnDestinationSideOfPier(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            PierSide deviceResponse = PierSide.pierUnknown;
            Exception exReturn = null;


            try
            {
                double ra = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RA_PARAMETER_NAME);
                double dec = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DEC_PARAMETER_NAME);
                deviceResponse = (PierSide)device.DestinationSideOfPier(ra, dec);
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(clientTransactionID, serverTransactionID, method, (int)deviceResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);

            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void ReturnAxisRates(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            dynamic deviceResponse = null;
            Exception exReturn = null;


            try
            {
                TelescopeAxes axis = (TelescopeAxes)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AXIS_PARAMETER_NAME);
                deviceResponse = device.AxisRates(axis);
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            List<RateResponse> rateResponse = new List<RateResponse>();
            foreach (dynamic r in deviceResponse)
            {
                rateResponse.Add(new RateResponse(r.Minimum, r.Maximum));
            }

            AxisRatesResponse responseClass = new AxisRatesResponse(clientTransactionID, serverTransactionID, method, rateResponse)
            {
                DriverException = exReturn
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);

            SendResponseToClient(method, request, response, exReturn, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void CallMethod(string deviceType, string method, HttpListenerRequest request, HttpListenerResponse response, NameValueCollection suppliedparameters, int clientID, int clientTransactionID, int serverTransactionID)
        {
            double ra, dec, az, alt, duration, switchValue;
            string command;
            bool raw, light, switchState;
            string switchName;
            PierSide pierSideValue = PierSide.pierUnknown;
            int positionInt, guideDuration, switchIndex;
            float positionFloat;
            GuideDirections guideDirection = GuideDirections.guideNorth;
            TelescopeAxes axis = TelescopeAxes.axisPrimary;

            Exception exReturn = null;

            try
            {
                switch (deviceType.ToLowerInvariant() + "." + method.ToLowerInvariant())
                {
                    // COMMON METHODS
                    case "*.commandblind":
                        command = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RAW_PARAMETER_NAME);
                        device.CommandBlind(command, raw);
                        break;
                    //TELESCOPE
                    case "telescope.sideofpier":
                        pierSideValue = (PierSide)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.SIDEOFPIER_PARAMETER_NAME);
                        device.SideOfPier = pierSideValue;
                        break;
                    case "telescope.unpark":
                        device.Unpark(); break;
                    case "telescope.park":
                        device.Park(); break;
                    case "telescope.abortslew":
                        device.AbortSlew(); break;
                    case "telescope.findhome":
                        device.FindHome(); break;
                    case "telescope.setpark":
                        device.SetPark(); break;
                    case "telescope.slewtotarget":
                        device.SlewToTarget(); break;
                    case "telescope.slewtotargetasync":
                        device.SlewToTargetAsync(); break;
                    case "telescope.synctotarget":
                        device.SyncToTarget(); break;
                    case "telescope.slewtocoordinates":
                        ra = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DEC_PARAMETER_NAME);
                        device.SlewToCoordinates(ra, dec);
                        break;
                    case "telescope.slewtocoordinatesasync":
                        ra = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DEC_PARAMETER_NAME);
                        device.SlewToCoordinatesAsync(ra, dec);
                        break;
                    case "telescope.slewtoaltaz":
                        az = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltAz(az, alt);
                        break;
                    case "telescope.slewtoaltazasync":
                        az = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltAzAsync(az, alt);
                        break;
                    case "telescope.synctoaltaz":
                        az = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ALT_PARAMETER_NAME);
                        device.SyncToAltAz(az, alt);
                        break;
                    case "telescope.synctocoordinates":
                        ra = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DEC_PARAMETER_NAME);
                        device.SyncToCoordinates(ra, dec);
                        break;
                    case "telescope.moveaxis":
                        axis = (TelescopeAxes)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AXIS_PARAMETER_NAME);
                        double rate = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.RATE_PARAMETER_NAME);
                        device.MoveAxis(axis, rate);
                        break;
                    case "telescope.pulseguide":
                        guideDirection = (GuideDirections)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DIRECTION_PARAMETER_NAME);
                        guideDuration = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DURATION_PARAMETER_NAME);
                        device.PulseGuide(guideDirection, guideDuration);
                        break;
                    // FOCUSER
                    case "focuser.halt":
                        device.Halt(); break;
                    case "focuser.move":
                        positionInt = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.POSITION_PARAMETER_NAME);
                        device.Move(positionInt);
                        break;
                    //CAMERA
                    case "camera.abortexposure":
                        device.AbortExposure(); break;
                    case "camera.pulseguide":
                        guideDirection = (GuideDirections)GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DIRECTION_PARAMETER_NAME);
                        guideDuration = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DURATION_PARAMETER_NAME);
                        device.PulseGuide(guideDirection, guideDuration);
                        break;
                    case "camera.startexposure":
                        duration = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.DURATION_PARAMETER_NAME);
                        light = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.LIGHT_PARAMETER_NAME);
                        device.StartExposure(duration, light);
                        break;
                    case "camera.stopexposure":
                        device.StopExposure(); break;
                    // DOME
                    case "dome.abortslew":
                        device.AbortSlew(); break;
                    case "dome.closeshutter":
                        device.CloseShutter(); break;
                    case "dome.findhome":
                        device.FindHome(); break;
                    case "dome.openshutter":
                        device.OpenShutter(); break;
                    case "dome.park":
                        device.Park(); break;
                    case "dome.setpark":
                        device.SetPark(); break;
                    case "dome.slewtoaltitude":
                        alt = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltitude(alt);
                        break;
                    case "dome.slewtoazimuth":
                        az = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AZ_PARAMETER_NAME);
                        device.SlewToAzimuth(az);
                        break;
                    case "dome.synctoazimuth":
                        az = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.AZ_PARAMETER_NAME);
                        device.SyncToAzimuth(az);
                        break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.refresh":
                        device.Refresh(); break;
                    // SWITCH
                    case "switch.setswitchname":
                        switchIndex = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);
                        switchName = GetParameter<string>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.NAME_PARAMETER_NAME);
                        device.SetSwitchName(switchIndex, switchName);
                        break;
                    case "switch.setswitch":
                        switchIndex = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);
                        switchState = GetParameter<bool>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.STATE_PARAMETER_NAME);
                        device.SetSwitch(switchIndex, switchState);
                        break;
                    case "switch.setswitchvalue":
                        switchIndex = GetParameter<int>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.ID_PARAMETER_NAME);
                        switchValue = GetParameter<double>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.VALUE_PARAMETER_NAME);
                        device.SetSwitchValue(switchIndex, switchValue);
                        break;
                    // FOCUSER
                    case "rotator.halt":
                        device.Halt(); break;
                    case "rotator.move":
                        positionFloat = GetParameter<float>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.POSITION_PARAMETER_NAME);
                        device.Move(positionFloat);
                        break;
                    case "rotator.moveabsolute":
                        positionFloat = GetParameter<float>(clientID, clientTransactionID, serverTransactionID, suppliedparameters, method, SharedConstants.POSITION_PARAMETER_NAME);
                        device.MoveAbsolute(positionFloat);
                        break;

                    default:
                        LogMessage(clientID, clientTransactionID, serverTransactionID, "CallMethod", "Unsupported method: " + method);
                        throw new InvalidValueException("CallMethod - Unsupported method: " + method);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            ReturnNoResponse(method, request, response, exReturn, clientID, clientTransactionID, serverTransactionID);
        }

        private T GetParameter<T>(int clientID, int clientTransactionID, int serverTransactionID, NameValueCollection suppliedParameters, string method, string ParameterName)
        {

            // Make sure a valid Parameter name was passed to us
            if (string.IsNullOrEmpty(ParameterName))
            {
                string errorMessage = string.Format("GetParameter - ParameterName is null or empty, when retrieving an {0} value", typeof(T).Name);
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, errorMessage);
                throw new InvalidParameterException(errorMessage);
            }

            // Check whether a string value of any kind was supplied as the parameter value
            string parameterStringValue = suppliedParameters[ParameterName];
            if (parameterStringValue == null)
            {
                string errorMessage = string.Format("GetParameter - The mandatory parameter: {0} is missing or has a null value.", ParameterName);
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, errorMessage);
                throw new InvalidParameterException(errorMessage);
            }

            // Handle string values first because they don't need to be converted into another type 
            if (typeof(T) == typeof(string))
            {
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, parameterStringValue));
                return (T)(object)parameterStringValue;
            }
            // Convert the parameter value into the required type
            switch (typeof(T).Name)
            {
                case "Single":
                    float singleValue;
                    if (!float.TryParse(parameterStringValue, out singleValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to a floating point value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, singleValue.ToString()));
                    return (T)(object)singleValue;
                case "Double":
                    double doubleValue;
                    if (!double.TryParse(parameterStringValue, out doubleValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to a floating point value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, doubleValue.ToString()));
                    return (T)(object)doubleValue;
                case "Int16":
                    short shortValue;
                    if (!short.TryParse(parameterStringValue, out shortValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to an Int16 value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, shortValue.ToString()));
                    return (T)(object)shortValue;
                case "Int32":
                    int intValue;
                    if (!int.TryParse(parameterStringValue, out intValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to an Int32 value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, intValue.ToString()));
                    return (T)(object)intValue;
                case "Boolean":
                    bool boolValue;
                    if (!bool.TryParse(parameterStringValue, out boolValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to a boolean value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, boolValue.ToString()));
                    return (T)(object)boolValue;
                case "DateTime":
                    DateTime dateTimeValue;
                    if (!DateTime.TryParse(parameterStringValue, out dateTimeValue)) throw new InvalidParameterException(string.Format("GetParameter - Supplied argument {0} for parameter {1} can not be converted to a DateTime value", parameterStringValue, ParameterName));
                    LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("{0} = {1}", ParameterName, dateTimeValue.ToString()));
                    return (T)(object)dateTimeValue;
                default:
                    string errorMessage = string.Format("Unsupported type: {0} called by method: ", typeof(T).Name, method);
                    LogMessage(clientID, clientTransactionID, serverTransactionID, "GetParameter", errorMessage);
                    throw new InvalidParameterException(errorMessage);
            }
        }

        private void ReturnNoResponse(string method, HttpListenerRequest request, HttpListenerResponse response, Exception ex, int clientID, int clientTransactionID, int serverTransactionID)
        {
            MethodResponse responseClass = new MethodResponse(clientTransactionID, serverTransactionID, method)
            {
                DriverException = ex
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseToClient(method, request, response, ex, responseJson, clientID, clientTransactionID, serverTransactionID);
        }

        private void SendResponseToClient(string method, HttpListenerRequest request, HttpListenerResponse response, Exception ex, string jsonResponse, int clientID, int clientTransactionID, int serverTransactionID)
        {
            if (ex == null) // Command ran successfully so return the JSON encoded result
            {
                LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("OK - no exception. Thread: {0}, Json: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), (jsonResponse.Length < 1000) ? jsonResponse : jsonResponse.Substring(0, 1000)));
                if (ScreenLogResponses) LogToScreen(string.Format("  OK - JSON: {0}", jsonResponse));
                TransmitResponse("application/json; charset=utf-8", HttpStatusCode.OK, "200 OK", jsonResponse, method, response, clientID, clientTransactionID, serverTransactionID);
            }
            else // Some sort of exception was thrown during command execution
            {
                if (ex.GetType() == typeof(InvalidParameterException)) // A required parameter is missing or invalid in the supplied http call
                {
                    if (ScreenLogResponses) LogToScreen(string.Format("  PARAMETER ERROR - ClientId: {0}, ClientTxnID: {1}, ServerTxnID: {2} - {3}", clientID, clientTransactionID, serverTransactionID, ex.Message));
                    TransmitResponse("text/plain; charset=utf-8", HttpStatusCode.BadRequest, "400 " + ex.Message, "400 " + ex.Message, method, response, clientID, clientTransactionID, serverTransactionID);
                }
                else
                {
                    if (ScreenLogResponses) LogToScreen(string.Format("  DEVICE ERROR - ClientId: {0}, ClientTxnID: {1}, ServerTxnID: {2} - {3}", clientID, clientTransactionID, serverTransactionID, ex.Message));
                    TransmitResponse("application/json; charset=utf-8", HttpStatusCode.OK, "200 OK", jsonResponse, method, response, clientID, clientTransactionID, serverTransactionID);
                }

                if (DebugTraceState) LogException(clientID, clientTransactionID, serverTransactionID, method, "Exception: " + ex.ToString());
                else LogException(clientID, clientTransactionID, serverTransactionID, method, "Exception: " + ex.Message);

                LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Thread: {0}, Json: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), jsonResponse));
            }
        }

        void TransmitResponse(string contentType, HttpStatusCode httpStatusCode, string statusDescription, string messageToSend, string method, HttpListenerResponse response, int clientID, int clientTransactionID, int serverTransactionID)
        {
            byte[] bytesToSend; // Array to hold the encoded message

            try
            {
                response.ContentType = contentType;
                response.StatusCode = (int)httpStatusCode; // Set the response status and status code
                response.StatusDescription = statusDescription;

                bytesToSend = Encoding.UTF8.GetBytes(messageToSend); // Convert the message to be returned into UTF8 bytes that can be sent over the wire
                if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Completed Encoding.GetBytes, array length: {0:n0}", bytesToSend.Length));

                if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Before setting response bytes - Length: {0:n0}, Response is null: {1}", bytesToSend.Length, response == null));
                response.ContentLength64 = bytesToSend.Length;
                if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, string.Format("Before writing {0:n0} bytes to output stream", response.ContentLength64));
                response.OutputStream.Write(bytesToSend, 0, bytesToSend.Length);
                if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, "After writing bytes to output stream");
                response.OutputStream.Close();
                if (DebugTraceState) LogMessage(clientID, clientTransactionID, serverTransactionID, method, "After closing output stream");
            }
            catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
            {
                LogException(clientID, clientTransactionID, serverTransactionID, "ListenerException", string.Format("Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
            }


        }

        #endregion

    } // End of ServerForm class

} // End of namespace
