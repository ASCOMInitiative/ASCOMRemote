using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using ASCOM.Common.Alpaca;
using ASCOM.Common.Helpers;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using Newtonsoft.Json;

namespace ASCOM.Remote
{

    public partial class ServerForm : Form
    {
        #region Constants and Enums

        // Testing Constants - NEVER TO BE USED IN PRODUCTION!
        private const bool FORCE_JSON_RESPONSE_TO_LOWER_CASE_TESTING_ONLY = false;

        // NOTE - Setup page HTML is set in the ReturnHTMLPageOrImage  method

        private const string SERVER_TRACELOGGER_NAME = "RemoteServer";
        private const string ACCESSLOG_TRACELOGGER_NAME = "ServerAccessLog";

        private const string SETUP_DEFAULT_INDEX_PAGE_NAME = "index.html";
        private const string SETUP_DEVICE_DEFAULT_INDEX_PAGE_NAME = "indexdevice.html";

        private const int MAX_ERRORS_BEFORE_CLOSE = 10; // Maximum number of async listen errors before the application permanently shuts down
        private const int SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH = 500; // Maximum length of a message that can be logged to the screen - required to prevent huge messages from locking up the system while the text box attempts to processes them
        private const int SCREEN_LOG_MAXIMUM_LENGTH = 50000; // Maximum length of the screen log - The screen log will be pruned to ensure it never exceeds this length, which would start to degrade performance

        private const string CORRECT_API_FORMAT_STRING_HTML = "<br>Required format is: <b>" +
                            "<font color=\"red\">api/v</font>" +
                            "<font color=\"blue\">x</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">devicetype</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">y</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">method</font>" +
                            "</b> where " +
                            "<font color=\"blue\">x</font>" +
                            " is the one based API version number and " +
                            "<font color=\"blue\">y</font>" +
                            " is the zero based number of the device. The whole URL must be in lower case."; // HTML error message when an unrecognised is received

        private const string CORRECT_API_FORMAT_STRING_PLAIN = "Required format is: api/vx/devicetype/y/method where x is the one based API version number and y is the zero based number of the device. " +
            "The whole URL must be in lower case."; // Plain text error message when an unrecognised is received

        private const string CORRECT_SERVER_FORMAT_STRING_HTML = "<br>Required format is: <b>" +
                            "<font color=\"red\">server/v</font>" +
                            "<font color=\"blue\">x</font>" +
                            "<font color=\"red\">/</font>" +
                            "<font color=\"blue\">configuration | profile | concurrency</font>" +
                            "</b> where x is the one based API version number. The" +
                            "<font color=\"blue\">server</font>" +
                            " and " +
                            "<font color=\"blue\">command</font>" +
                            " fields must be in lower case."; // HTML error message when an unknown server command is received.

        private const string CORRECT_SERVER_FORMAT_STRING_PLAIN = "Required format is: server/vx/configuration | profile | concurrency where x is the one based API version number. " +
            "The server and command fields must be in lower case."; // PLain text error message when an unknown server command is received.

        private const string UNRECOGNISED_URI_MESSAGE_HTML = "You have reached the <i><b>ASCOM Remote Server</b></i> API portal but your url did not start with /api, /management, /server or /setup (must be lower case) <p><b>Available devices:</b></p>";

        private const string UNRECOGNISED_URI_MESSAGE_PLAIN = "You have reached the ASCOM Remote Server API portal but your url did not start with /api, /management, /server or /setup (must be lower case) Available devices: \r\n";

        private const string GET_UNKNOWN_METHOD_MESSAGE = "GET - Unknown device method: ";
        private const string PUT_UNKNOWN_METHOD_MESSAGE = "PUT - Unknown device method: ";
        private const string MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE = "The management interface is not enabled, please enable it using the remote access server configuration dialogue";
        private const string API_INTERFACE_NOT_ENABLED_MESSAGE = "Access to the device server API is not currently available, please ask the owner to enable it.";
        private const char FORWARD_SLASH = '/'; // Forward slash character as a char value
        private const string X_FORWARDED_FOR = "X-Forwarded-For";

        // CORS header constants
        private const string CORS_ORIGIN_HEADER = "Origin";
        private const string CORS_VARY_HEADER = "Vary";
        private const string CORS_ALLOWED_METHODS_HEADER = "Access-Control-Allow-Methods";
        private const string CORS_MAX_AGE_HEADER = "Access-Control-Max-Age";
        private const string CORS_ALLOW_ORIGIN_HEADER = "Access-Control-Allow-Origin";
        private const string CORS_ALLOW_CREDENTIALS_HEADER = "Access-Control-Allow-Credentials";

        // CORS value constants
        private const string CORS_ALLOWED_METHODS = "GET, PUT, OPTIONS";
        private const string CORS_PERMISSION_ALL_ORIGINS = "*";
        private const string CORS_PERMISSION_REFLECT_SUPPLIED_ORIGIN = "=";
        private const string CORS_ALLOW_CREDENTIALS = "true";

        // Position of each element within the client's requested URI 
        internal const int URL_ELEMENT_API = 0; // For /api/ URIs
        internal const int URL_ELEMENT_API_VERSION = 1;
        internal const int URL_ELEMENT_DEVICE_TYPE = 2;
        internal const int URL_ELEMENT_DEVICE_NUMBER = 3;
        internal const int URL_ELEMENT_METHOD = 4;
        internal const int URL_ELEMENT_SERVER_COMMAND = 2; // For /server/ type uris

        // Device server profile persistence constants
        internal const string SERVER_LOG_FOLDER_PROFILENAME = "Server Log Folder"; // No default value constant because the default Documents folder has to be calculated dynamically at run time
        internal const string SERVER_ACCESS_LOG_PROFILENAME = "Server Access Log Enabled"; public const bool SERVER_ACCESS_LOG_DEFAULT = true;
        internal const string SERVER_TRACE_LEVEL_PROFILENAME = "Server Trace Level"; public const bool SERVER_TRACE_LEVEL_DEFAULT = true;
        internal const string SERVER_DEBUG_TRACE_PROFILENAME = "Server Include Debug Trace"; public const bool SERVER_DEBUG_TRACE_DEFAULT = false;
        internal const string SERVER_IPADDRESS_PROFILENAME = "Server IP Address"; public const string SERVER_IPADDRESS_DEFAULT = SharedConstants.LOCALHOST_ADDRESS_IPV4;
        internal const string SERVER_PORTNUMBER_PROFILENAME = "Server Port Number"; public const decimal SERVER_PORTNUMBER_DEFAULT = 11111;
        internal const string SERVER_AUTOCONNECT_PROFILENAME = "Server Auto Connect"; public const bool SERVER_AUTOCONNECT_DEFAULT = true;
        internal const string SCREEN_LOG_REQUESTS_PROFILENAME = "Log Requests To Screen"; public const bool SCREEN_LOG_REQUESTS_DEFAULT = true;
        internal const string SCREEN_LOG_RESPONSES_PROFILENAME = "Log Responses To Screen"; public const bool SCREEN_LOG_RESPONSES_DEFAULT = false;
        internal const string ALLOW_CONNECTED_SET_FALSE_PROFILENAME = "Allow Connected Set False"; public const bool ALLOW_CONNECTED_SET_FALSE_DEFAULT = true;
        internal const string ALLOW_CONNECTED_SET_TRUE_PROFILENAME = "Allow Connected Set True"; public const bool ALLOW_CONNECTED_SET_TRUE_DEFAULT = true;
        internal const string ALLOW_CONCURRENT_ACCESS_PROFILENAME = "Allow Concurrent Access"; public const bool ALLOW_CONCURRENT_ACCESS_DEFAULT = false;
        internal const string MANAGEMENT_INTERFACE_ENABLED_PROFILENAME = "Management Interface Enabled"; public const bool MANGEMENT_INTERFACE_ENABLED_DEFAULT = false;
        internal const string START_WITH_API_ENABLED_PROFILENAME = "Start WIth API Enabled"; public const bool START_WITH_API_ENABLED_DEFAULT = true;
        internal const string RUN_DRIVERS_ON_SEPARATE_THREADS_PROFILENAME = "Run Drivers On Separate Threads"; public const bool RUN_DRIVERS_ON_SEPARATE_THREADS_DEFAULT = true;
        internal const string LOG_CLIENT_IPADDRESS_PROFILENAME = "Log Client IP Address"; public const bool LOG_CLIENT_IPADDRESS_DEFAULT = false;
        internal const string INCLUDE_DRIVEREXCEPTION_IN_JSON_RESPONSE_PROFILENAME = "Include Driver Exception In JSON Response"; public const bool INCLUDE_DRIVEREXCEPTION_IN_JSON_RESPONSE_DEFAULT = false;
        internal const string REMOTE_SERVER_LOCATION_PROFILENAME = "Remote Server Location"; public const string REMOTE_SERVER_LOCATION_DEFAULT = "Unknown";
        internal const string CORS_PERMITTED_ORIGINS_PROFILENAME = "CORS Permitted Origins"; public const string CORS_PERMITTED_ORIGINS_DEFAULT = "*";
        internal const string CORS_SUPPORT_ENABLED_PROFILENAME = "CORS Support Enabled"; public const bool CORS_SUPPORT_ENABLED_DEFAULT = false;
        internal const string CORS_MAX_AGE_PROFILENAME = "CORS Max Age"; public const decimal CORS_MAX_AGE_DEFAULT = 3600;
        internal const string CORS_CREDENTIALS_PERMITTED_PROFILENAME = "CORS Credentials Permitted"; public const bool CORS_CREDENTIALS_PERMITTED_DEFAULT = false;
        internal const string ALPACA_DISCOVERY_ENABLED_PROFILENAME = "Alpaca Discovery Enabled"; public const bool ALPACA_DISCOVERY_ENABLED_DEFAULT = true;
        internal const string ALPACA_UNIQUE_ID_PROFILENAME = "Alpaca Unique ID"; public static string ALPACA_UNIQUE_ID_DEFAULT = Guid.Empty.ToString();
        internal const string ALPACA_DISCOVERY_PORT_PROFILENAME = "Alpaca Discovery Port"; public static int ALPACA_DISCOVERY_PORT_DEFAULT = SharedConstants.ALPACA_DISCOVERY_PORT;
        internal const string MAXIMUM_NUMBER_OF_DEVICES_PROFILENAME = "Maximum Number Of Devices"; public static int MAXIMUM_NUMBER_OF_DEVICES_DEFAULT = 10;
        internal const string IPV4_ENABLED_PROFILENAME = "IP v4 Enabled"; public const bool IPV4_ENABLED_DEFAULT = true;
        internal const string IPV6_ENABLED_PROFILENAME = "IP v6 Enabled"; public const bool IPV6_ENABLED_DEFAULT = false;
        internal const string ROLLOVER_LOGS_ENABLED_PROFILENAME = "Rollover Logs Enabled"; public const bool ROLLOVER_LOGS_ENABLED_DEFAULT = false;
        internal const string ROLLOVER_TIME_PROFILENAME = "Rollover Time"; public static DateTime ROLLOVER_TIME_DEFAULT = DateTime.Now.Date; // Default value can't be const because it's a date-time type
        internal const string USE_UTC_TIME_IN_LOGS_PROFILENAME = "Use UTC Time In Logs"; public const bool USE_UTC_TIME_IN_LOGS_ENABLED_DEFAULT = false;
        internal const string MINIMISE_TO_SYSTEM_TRAY_PROFILENAME = "Minimise To System Tray"; public const bool MINIMISE_TO_SYSTEM_TRAY_DEFAULT = false;
        internal const string CONFIRM_EXIT_PROFILENAME = "Confirm Exit"; public const bool CONFIRM_EXIT_DEFAULT = false;
        internal const string MINIMISE_ON_START_PROFILENAME = "Minimise On Start"; public const bool MINIMISE_ON_START_DEFAULT = false;

        // Minimise behaviour strings
        internal const string MINIMISE_TO_SYSTEM_TRAY_KEY = "Minimise to system tray";
        internal const string MINIMISE_TO_SYSTEM_TRAY_DESCRIPTION = "Service like behaviour - Minimises to the system tray and is hidden from ALT/TAB when minimised.";
        internal const string MINIMISE_TO_TASK_BAR_KEY = "Minimise to task bar";
        internal const string MINIMISE_TO_TASK_BAR_DESCRIPTION = "Normal application behaviour - Minimises to the task bar and can be restored using ALT/TAB.";

        //Device profile persistence constants
        internal const string DEVICE_SUBFOLDER_NAME = "Device";
        internal const string DEVICETYPE_PROFILENAME = "Device Type"; public const string DEVICETYPE_DEFAULT = SharedConstants.DEVICE_NOT_CONFIGURED;
        internal const string PROGID_PROFILENAME = "ProgID"; public const string PROGID_DEFAULT = SharedConstants.DEVICE_NOT_CONFIGURED;
        internal const string DESCRIPTION_PROFILENAME = "Description"; public const string DESCRIPTION_DEFAULT = "";
        internal const string DEVICENUMBER_PROFILENAME = "Device Number"; public const int DEVICENUMBER_DEFAULT = 0;
        internal const string DEVICE_UNIQUE_ID_PROFILENAME = "Device Unique ID"; public const string DEVICE_UNIQUE_ID_DEFAULT = "Uninitialised";

        // !!!!! This list must match the names of the ServedDevice instances on the SetupForm !!!!!
        internal static SortedSet<string> ServerDeviceNames; // = new SortedSet<string>() { "ServedDevice9", "ServedDevice8", "ServedDevice7", "ServedDevice6", "ServedDevice5", "ServedDevice4", "ServedDevice3", "ServedDevice2", "ServedDevice1", "ServedDevice0" };
        internal static SortedSet<string> ServerDeviceNumbers; // = new SortedSet<string>() { "9", "8", "7", "6", "5", "4", "3", "2", "1", "0" };

        // Form size and resize constants
        internal const int FORM_MINIMUM_WIDTH = 400; // Server form minimum width
        internal const int FORM_MINIMUM_HEIGHT = 350; // Server form minimum height
        internal const int TITLE_OFFSET_FROM_TOP = 22; // Offset of the title from the top of the form
        internal const int TITLE_TRANSITION_POSITION_END = 900; // Form width above which the title position is always centred over the message list
        internal const int LOG_HEIGHT_OFFSET = 123; // Offset from the height of the form so that the log text box just fits within the form when resized
        internal const int CONTROL_OVERALL_HEIGHT = 103; // Overall height of all control groups
        internal const int CONTROL_SPACING_MAXIMUM = 45; // Maximum separation between control groups
        internal const int CONTROL_SPACE_WIDTH = 240; // Size of the free space, to the right of the log messages text box, that must be left clear for server controls
        internal const int CONTROL_LEFT_OFFSET = 206; // Offset from the width of the form to the start of a full sized control
        internal const int CONTROL_CENTRE_OFFSET = 36; // Offset from the CONTROL_LEFT_OFFSET to the start of a centred control

        // The server form displays controls in several groups:
        // Control Group 1 - Concurrent transactions counter
        // Control Group 2 - Log requests and responses check boxes
        // Control Group 3 - Remote Server Status panel plus the Stop and Start buttons
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

        public bool RestartApplication = false;

        internal static HttpListener httpListener;
        //internal static UdpClient udpClientIpV4;
        internal static TraceLoggerPlus AccessLog;

        // Lists to hold discovery UDP clients
        private List<UdpClient> discoveryClientsIpV6 = new List<UdpClient>();
        private List<UdpClient> discoveryClientsIpV4 = new List<UdpClient>();
        private int discoveryCount; // Unique number for each discovery packet received

        internal readonly object counterLock = new object();
        internal readonly object managementCommandLock = new object();

        // Application Status variables
        internal static bool apiIsEnabled = false;
        internal static bool devicesAreConnected = false;

        internal static uint serverTransactionIDCounter = 0; // Internal variable used to keep track of the current server transaction ID
        internal static int numberOfConcurrentTransactions = 0; // Internal variable to keep track of the number of concurrent transactions
        internal static int numberOfConsecutiveErrors = 0; // Counter to record the number of consecutive errors, this is reset to zero whenever a successful async listen occurs

        // Variable to hold the last log time so that old logs are closed and new logs are started when we move to a new day
        internal static DateTime NextTraceLogRolloverTime = DateTime.Now; // Initialise to now to ensure that the roll-over code works correctly
        internal static DateTime NextAccessLogRolloverTime = DateTime.Now; // Initialise to now to ensure that the roll-over code works correctly

        internal static ConcurrentDictionary<string, ConfiguredDevice> ConfiguredDevices;
        internal static ConcurrentDictionary<string, ActiveObject> ActiveObjects;

        // Configuration variables that can be changed through the Setup dialogue 
        internal static string TraceFolder;
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
        internal static bool LogClientIPAddress;
        internal static bool IncludeDriverExceptionInJsonResponse;
        internal static string RemoteServerLocation;
        internal static List<string> CorsPermittedOrigins = new List<string>(); // List of permitted origins
        internal static bool CorsSupportIsEnabled;
        internal static decimal CorsMaxAge;
        internal static bool CorsCredentialsPermitted;
        internal static bool AlpacaDiscoveryEnabled;
        internal static string AlpacaUniqueId; // Unique UUID / GUID for this particular Alpaca device (common to all served devices)
        internal static decimal AlpacaDiscoveryPort; // Port on which the Alpaca discovery listener will listen
        internal static int MaximumNumberOfDevices;
        internal static bool IpV4Enabled;
        internal static bool IpV6Enabled;
        internal static bool RolloverLogsEnabled;
        internal static DateTime RolloverTime;
        internal static bool UseUtcTimeInLogs;
        internal static bool MinimiseToSystemTray;
        internal static bool ConfirmExit;
        internal static bool StartMinimised;

        #endregion

        #region Thread Global Variables

        // These are global within a Remote Device Server thread but will be different between threads

        [ThreadStatic]
        internal static dynamic device; // Shortcut to the device referenced by the incoming URI
        [ThreadStatic]
        internal static bool allowConnectedSetFalse; // Shortcut to a flag indicating whether Connected can be set False
        [ThreadStatic]
        internal static bool allowConnectedSetTrue; // Shortcut to a flag indicating whether Connected can be set True
        [ThreadStatic]
        internal static bool allowConcurrentAccess; // Shortcut to a flag indicating whether the driver can handle concurrent calls

        #endregion

        #region Private Variables

        private static readonly object logLockObject = new object(); // Lock object to ensure that midnight log change overs happen smoothly
        private static TraceLoggerPlus TL; // Variable to hold the trace logger
        private static FormWindowState formWindowState; // Holds the server form window state in use before the application is minimised to the system tray so that this state can be restored when required

        #endregion

        #region Delegates for Form callbacks

        private delegate void SetTextCallback(string text);
        private delegate void SetConcurrencyCallback();
        private delegate void DestroyDriverDelegate();
        private delegate Thread DriverCommandDelegate(RequestData requestData);
        private delegate void CreateInstanceDelegate(KeyValuePair<string, ConfiguredDevice> kvp);

        #endregion

        #region Initialisation, Form Load and Close

        public ServerForm()
        {
            try
            {
                InitializeComponent();
                // This TraceLogger is only used for debugging Profile value Retrieval. It is replaced below after the required logging location is retrieved from the Profile 
                TL = new TraceLoggerPlus("", SERVER_TRACELOGGER_NAME) // Trace state is enabled or disabled in the ReadProfile method
                {
                    UseUtcTime = UseUtcTimeInLogs
                };

                // Initialise lists
                ConfiguredDevices = new ConcurrentDictionary<string, ConfiguredDevice>();
                ActiveObjects = new ConcurrentDictionary<string, ActiveObject>();
                ServerDeviceNames = new SortedSet<string>();
                ServerDeviceNumbers = new SortedSet<string>();

                ReadProfile();

                // Close the debugging log
                TL.Enabled = false;
                TL.Dispose();

                // Create the application log file in the folder specified by the user
                TL = new TraceLoggerPlus("", TraceFolder, SERVER_TRACELOGGER_NAME, TraceState)
                {
                    LogFilePath = TraceFolder, // Set the trace folder to the user specified value
                    Enabled = TraceState, // Enable the log if required
                    UseUtcTime = UseUtcTimeInLogs
                };

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                LogMessage(0, 0, 0, "New", $"Remote Server Version {version}, Started on {DateTime.Now.ToString("dddd d MMMM yyyy HH: mm:ss")}");
                LogMessage(0, 0, 0, "New", $"Running as user {WindowsIdentity.GetCurrent().Name}");
                LogMessage(0, 0, 0, "New", $"Running as a {(Environment.Is64BitProcess ? "64" : "32")}bit application on a {(Environment.Is64BitOperatingSystem ? "64" : "32")}bit OS.");

                AccessLog = new TraceLoggerPlus("", TraceFolder, ACCESSLOG_TRACELOGGER_NAME, AccessLogEnabled)
                {
                    LogFilePath = TraceFolder, // Set the trace folder to the user specified value
                    Enabled = AccessLogEnabled,
                    UseUtcTime = UseUtcTimeInLogs
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
                notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
                notifyIcon.Click += NotifyIcon_Click;
                systemTrayMenuItems.Items["Exit"].Click += SystemTrayCloseServer_Click;
                systemTrayMenuItems.Items["Title"].Click += SystemTrayTitle_Click;

                this.FormClosing += ServerForm_FormClosing;
                ServerForm_Resize(this, new EventArgs()); // Move controls to their correct positions

                // Check whether this device already has an Alpaca unique ID, if not, create one
                if (AlpacaUniqueId == Guid.Empty.ToString()) // Guid.Empty is the default value created when the configuration is retrieved if there is no pre-existing value
                {
                    // We don't have a unique ALpaca ID so create a new UUID and persist it
                    string guidString = Guid.NewGuid().ToString().ToUpperInvariant(); // Create a new GUID
                    AlpacaUniqueId = guidString; // Save the new GUID to the working variable
                    WriteProfile();
                    LogMessage(0, 0, 0, "AlpacaUniqueID", $"Created new Alpaca unique device ID: {AlpacaUniqueId}");
                }

                // Write starting configuration to log
                WriteConfigurationToLog();

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
            this.Text = $"ASCOM Remote Server - v{Assembly.GetExecutingAssembly().GetName().Version} - {(Environment.Is64BitProcess ? "64" : "32")}bit mode.";
            if (StartWithDevicesConnected)
            {
                ConnectDevices();
            }

            if (StartWithApiEnabled)
            {
                StartRESTServer();
            }

            try
            {
                // Start minimised or bring for to the top of the Z order
                if (StartMinimised) // Start minimised
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                else // Start normally
                {
                    // Bring this form to the front of the screen
                    this.WindowState = FormWindowState.Minimized;
                    this.Show();
                    this.WindowState = FormWindowState.Normal;

                    // Ensure that the system tray icon is not visible when the application starts
                    notifyIcon.Visible = false;
                    this.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "ServerForm.Load", $"Exception setting windows state: \r\n{ex} ");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Clear down the listener
            LogMessage(0, 0, 0, "FormClosed", string.Format("Stopping Remote server on thread {0}", Thread.CurrentThread.ManagedThreadId));
            StopRESTServer();
            LogMessage(0, 0, 0, "FormClosed", string.Format("Stopped Remote server on thread {0}", Thread.CurrentThread.ManagedThreadId));

            // Clear all of the current objects
            LogMessage(0, 0, 0, "FormClosed", string.Format("Disconnecting devices on thread {0}", Thread.CurrentThread.ManagedThreadId));
            DisconnectDevices();
            LogMessage(0, 0, 0, "FormClosed", string.Format("Disconnected devices on thread {0}", Thread.CurrentThread.ManagedThreadId));

            WaitFor(200);
            LogMessage(0, 0, 0, "FormClosed", "");
            LogMessage(0, 0, 0, "FormClosed", string.Format("Remote Server SHUT DOWN on thread {0}", Thread.CurrentThread.ManagedThreadId));
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Restore the server form when minimised
        /// </summary>
        private void RestoreForm()
        {
            // Show the form
            this.Show();

            // Restore to the window state in use before the application was minimised
            this.WindowState = formWindowState;

            // Hide the system tray icon
            notifyIcon.Visible = false;
            
            this.BringToFront();
        }

        private uint GetServerTransactionID()
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

        private void StartRESTServer()
        {
            if (this.InvokeRequired)
            {
                LogMessage(0, 0, 0, "StartRESTServer", "Invoke required...");
                this.Invoke(new Action(() => this.StartRESTServer()));
            }
            else
            {
                try
                {
                    if (IpV4Enabled | IpV6Enabled)
                    {

                        // Construct URIs based on configured IP address parameters 
                        string apiOperatingUri = $@"http://{ServerIPAddressString}:{ServerPortNumber}{SharedConstants.API_URL_BASE}";
                        string remoteServerManagementUri = $@"http://{ServerIPAddressString}:{ServerPortNumber}{SharedConstants.REMOTE_SERVER_MANAGEMENT_URL_BASE}";
                        string alpacaDeviceManagementUri = $@"http://{ServerIPAddressString}:{ServerPortNumber}{SharedConstants.ALPACA_DEVICE_MANAGEMENT_URL_BASE}";
                        string alpacaDeviceSetupUri = $@"http://{ServerIPAddressString}:{ServerPortNumber}{SharedConstants.ALPACA_DEVICE_SETUP_URL_BASE}";

                        // Construct a URI for the configured address i.e the effective address after filtering
                        string configuredOperatingUri = $@"http://{ServerIPAddressString}:{ServerPortNumber}{SharedConstants.API_URL_BASE}";

                        LogMessage(0, 0, 0, "StartRESTServer", "IPv4 Operating URI: " + apiOperatingUri);
                        LogMessage(0, 0, 0, "StartRESTServer", "IPv4 Management URI: " + remoteServerManagementUri);
                        LogMessage(0, 0, 0, "StartRESTServer", "Configured URI: " + configuredOperatingUri);
                        LogToScreen($"ASCOM Remote Alpaca device listening URI: {configuredOperatingUri}");

                        // Create the listener on the required URIs
                        LogMessage(0, 0, 0, "StartRESTServer", "Stopping existing server");
                        StopRESTServer();

                        LogMessage(0, 0, 0, "StartRESTServer", "Creating the listener");
                        httpListener = new HttpListener();
                        httpListener.Prefixes.Add(apiOperatingUri); // Set up the listener on the api URI
                        httpListener.Prefixes.Add(remoteServerManagementUri); // Set up the listener on the remote server bespoke management command URI
                        httpListener.Prefixes.Add(alpacaDeviceManagementUri); // Set up the listener on the management URI common to all Alpaca devices
                        httpListener.Prefixes.Add(alpacaDeviceSetupUri); // Set up the listener on the HTTP Setup URI common to all Alpaca devices

                        // Start the listener and ask permission if required
                        while (!httpListener.IsListening)
                        {
                            try
                            {
                                LogMessage(0, 0, 0, "StartRESTServer", "Starting the listener");
                                httpListener.Start();
                                LogMessage(0, 0, 0, "StartRESTServer", "Listener started");
                            }
                            catch (HttpListenerException ex) when (ex.ErrorCode == (int)WindowsErrorCodes.ERROR_ACCESS_DENIED) // User does not have an ACL permitting this address and port to be used so get permission
                            {
                                DialogResult dlgResult = MessageBox.Show("You need to give permission for the Remote Server to listen for incoming requests, do you wish to do this?\r\n\r\nThe server will restart after the new permissions are set.\r\n\r\n(Requires administrator privilege)", "HTTP listen permissions required", MessageBoxButtons.YesNo);
                                if (dlgResult == DialogResult.Yes) // Permission given so set the ACL using net-sh, which will ask for elevation if required
                                {
                                    LogMessage(0, 0, 0, "StartRESTServer", "User gave permission to set port ACL");
                                    LogMessage(0, 0, 0, "StartRESTServer", "Closing listener"); // Have to close listener before setting ACL
                                    httpListener.Close();
                                    httpListener = null;
                                    LogMessage(0, 0, 0, "StartRESTServer", "Enabling URIs");

                                    string userName = $"\"{Environment.UserDomainName}\\{Environment.UserName}\"";

                                    LogMessage(0, 0, 0, "StartRESTServer", $"API URI: {apiOperatingUri}");
                                    LogMessage(0, 0, 0, "StartRESTServer", $"Alpaca HTTP Setup URI: {alpacaDeviceSetupUri}");
                                    LogMessage(0, 0, 0, "StartRESTServer", $"Alpaca device management URI: {alpacaDeviceManagementUri}");
                                    LogMessage(0, 0, 0, "StartRESTServer", $"Remote server management URI: {remoteServerManagementUri}");
                                    LogMessage(0, 0, 0, "StartRESTServer", $"User: {userName}");

                                    try
                                    {
                                        string setNetworkPermissionsPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + SharedConstants.SET_NETWORK_PERMISSIONS_EXE_PATH;
                                        LogMessage(0, 0, 0, "StartRESTServer", string.Format("SetNetworkPermissionspath: {0}", setNetworkPermissionsPath));

                                        // Check that the SetNetworkPermissions exe exists
                                        if (File.Exists(setNetworkPermissionsPath)) // SetNetworkPermissions exists
                                        {
                                            string args = $"--{SharedConstants.ENABLE_API_URI_COMMAND_NAME} {apiOperatingUri} " +
                                                $"--{SharedConstants.ENABLE_REMOTE_SERVER_MANAGEMENT_URI_COMMAND_NAME} {remoteServerManagementUri} " +
                                                $"--{SharedConstants.ENABLE_ALPACA_DEVICE_MANAGEMENT_URI_COMMAND_NAME} {alpacaDeviceManagementUri} " +
                                                $"--{SharedConstants.ENABLE_ALPACA_SETUP_URI_COMMAND_NAME} {alpacaDeviceSetupUri} " +
                                                $"--{SharedConstants.ENABLE_HTTP_DOT_SYS_PORT_COMMAND_NAME} {ServerPortNumber} " +
                                                $"--{SharedConstants.USER_NAME_COMMAND_NAME} {userName}";
                                            LogMessage(0, 0, 0, "StartRESTServer", $"SetNetworkPermissions arguments: {args}");

                                            ProcessStartInfo psi = new ProcessStartInfo(setNetworkPermissionsPath, args)
                                            {
                                                Verb = "runas",
                                                CreateNoWindow = true,
                                                WindowStyle = ProcessWindowStyle.Hidden,
                                                UseShellExecute = true
                                            };

                                            LogToScreen(" ");
                                            LogToScreen("Setting network permissions, please wait until the Remote Server restarts, this can take several seconds.");
                                            LogMessage(0, 0, 0, "StartRESTServer", "Starting SetNetworkPermissions process");
                                            Process.Start(psi).WaitForExit();
                                            LogMessage(0, 0, 0, "StartRESTServer", "Completed SetNetworkPermissions process");

                                            // Restart the server so that the revised permissions come into effect
                                            try
                                            {
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"");
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"RESTARTING SERVER TO ENABLE NEW PERMISSIONS TO BE USED");
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"");

                                                LogToScreen("Disconnecting devices...");
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"About to disconnect devices...");
                                                DisconnectDevices();
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"Devices disconnected");

                                                LogToScreen("Restarting the Remote Server...");
                                                this.RestartApplication = true;
                                            }
                                            catch (Exception ex2)
                                            {
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"Exception while attempting to restart the server: {ex2.Message}");
                                                LogException(0, 0, 0, "RestartRESTServer", ex2.ToString());
                                            }
                                            finally
                                            {
                                                LogMessage(0, 0, 0, "RestartRESTServer", $"About to close form ...");
                                                this.Close(); // Close the form
                                            }
                                            return; // Leave the routine and wait for the form to close
                                        }
                                        else // SetNetworkPermissions does not exist
                                        {
                                            string errorMessage = string.Format("Cannot find SetNetworkPermissions program: {0} ", setNetworkPermissionsPath);
                                            LogMessage(0, 0, 0, "StartRESTServer", errorMessage);
                                            LogToScreen(errorMessage);
                                            return;
                                        }
                                    }
                                    catch (Exception ex1)
                                    {
                                        LogException(0, 0, 0, "StartRESTServer", ex1.ToString());
                                        LogToScreen("Exception while enabling the API and Management URIs: " + ex1.Message);
                                    }

                                    // Create a new listener instance and loop round to attempt to start it again
                                    LogMessage(0, 0, 0, "StartRESTServer", "Creating listener");
                                    httpListener = new HttpListener(); // Set up the listener so that Start can be attempted again at the top of the while loop
                                    httpListener.Prefixes.Add(apiOperatingUri); // Set up the listener on the required URI
                                    httpListener.Prefixes.Add(remoteServerManagementUri); // Set up the listener on the management URI
                                }
                                else
                                {
                                    LogMessage(0, 0, 0, "StartRESTServer", "User did NOT give permission to set port ACL");
                                    return; // Just exit and wait for user to do something
                                }
                            }
                        }

                        LogMessage(0, 0, 0, "StartRESTServer", "Starting wait for incoming API requests");
                        IAsyncResult result = httpListener.BeginGetContext(new AsyncCallback(RestRequestReceivedHandler), httpListener);
                    }

                    apiIsEnabled = true;
                    LblRESTStatus.BackColor = Color.Green;
                    LblRESTStatus.Text = "Remote Server Up";
                    LogBlankLine(0, 0, 0);

                    // Start the Alpaca discovery listeners if configured to do so
                    if (AlpacaDiscoveryEnabled) // Discovery is enabled
                    {

                        // Get a list of all network interfaces on this PC
                        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

                        if (IpV4Enabled) // IPv4 discovery is supported
                        {
                            LogMessage(0, 0, 0, "StartRESTServer", $"IPv4 discovery is enabled, searching for interfaces...");

                            // Close the IPv6 discovery UDP listeners
                            CloseIpV4BroadcastListeners();

                            // Bind a UDP client to each network adapter and set the index and address for multicast. Windows needs to have both the IP Address and index set for an IPv6 multicast socket

                            // Examine each adapter in turn to test whether it has a link local IPv6 address to which a UDP client should be bound
                            foreach (var adapter in adapters)
                            {
                                if (adapter.OperationalStatus == OperationalStatus.Up) // Only check operational interfaces
                                {
                                    if (adapter.Supports(NetworkInterfaceComponent.IPv4)) // Only check interfaces that support IPv4
                                    {
                                        // Try to get detailed information about this interface
                                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                                        if (adapterProperties != null) // Detailed information is available
                                        {
                                            // Extract a list of all unicast addresses assigned to this adapter
                                            UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;

                                            // Iterate over the unicast addresses looking for IPv4 addresses or the IPv4 loop back address
                                            foreach (UnicastIPAddressInformation uni in uniCast)
                                            {
                                                if (uni.Address.AddressFamily == AddressFamily.InterNetwork) // Found an IPv4 address
                                                {
                                                    // Create a UDP client to listen for Alpaca IPv4 broadcasts on this IPv4 interface
                                                    UdpClient udpClientV4 = new UdpClient(AddressFamily.InterNetwork);
                                                    udpClientV4.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                                    udpClientV4.EnableBroadcast = true;
                                                    udpClientV4.MulticastLoopback = false;
                                                    udpClientV4.ExclusiveAddressUse = false;
                                                    udpClientV4.Client.Bind(new IPEndPoint(uni.Address, (int)AlpacaDiscoveryPort));

                                                    //  Add the client to the collection of UDP clients
                                                    discoveryClientsIpV4.Add(udpClientV4);
                                                    LogMessage(0, 0, 0, "StartRESTServer", $"Added IPv4 discovery listener on address {uni.Address}");

                                                    // Start listening for IPv4 broadcasts on this interface
                                                    udpClientV4.BeginReceive(DiscoveryCallback, udpClientV4);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            LogMessage(0, 0, 0, "StartRESTServer", $"Listening for IPv4 discovery broadcasts on port {AlpacaDiscoveryPort}.");
                            LogBlankLine(0, 0, 0);

                            LogToScreen($"Listening for IPv4 discovery broadcasts on port {AlpacaDiscoveryPort}.");
                        }
                        else
                        {
                            LogToScreen($"IPv4 discovery not enabled.");
                        }

                        if (IpV6Enabled) // IPv6 discovery is supported
                        {
                            LogMessage(0, 0, 0, "StartRESTServer", $"IPv6 discovery is enabled, searching for interfaces...");

                            // Close the IPv6 discovery UDP listeners
                            CloseIpV6BroadcastListeners();

                            // Bind a UDP client to each network adapter and set the index and address for multicast. Windows needs to have both the IP Address and index set for an IPv6 multicast socket

                            // Examine each adapter in turn to test whether it has a link local IPv6 address to which a UDP client should be bound
                            foreach (var adapter in adapters)
                            {
                                if (adapter.OperationalStatus == OperationalStatus.Up) // Only check operational interfaces
                                {
                                    if (adapter.Supports(NetworkInterfaceComponent.IPv6) && adapter.SupportsMulticast) // Only check interfaces that support IPv6 multicast
                                    {
                                        // Try to get detailed information about this interface
                                        IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                                        if (adapterProperties != null) // Detailed information is available
                                        {
                                            // Extract a list of all unicast addresses assigned to this adapter
                                            UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;

                                            // Iterate over the unicast addresses looking for IPv6 link local addresses or the IPv6 loop back address
                                            foreach (UnicastIPAddressInformation uni in uniCast)
                                            {
                                                if (uni.Address.IsIPv6LinkLocal | ((uni.Address.AddressFamily == AddressFamily.InterNetworkV6) & IPAddress.IsLoopback(uni.Address))) // Found either an IPv6 link local address or the IPv6 loop back address
                                                {
                                                    // Create a UDP client to listen for Alpaca IPv6 multi casts on this IPv6 interface
                                                    UdpClient udpClientV6 = new UdpClient(AddressFamily.InterNetworkV6);
                                                    udpClientV6.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                                                    udpClientV6.ExclusiveAddressUse = false;
                                                    udpClientV6.Client.Bind(new IPEndPoint(uni.Address, (int)AlpacaDiscoveryPort));
                                                    udpClientV6.JoinMulticastGroup(adapterProperties.GetIPv6Properties().Index, IPAddress.Parse(SharedConstants.ALPACA_DISCOVERY_MULTICAST_GROUP));

                                                    //  Add the client to the collection of UDP clients
                                                    discoveryClientsIpV6.Add(udpClientV6);
                                                    LogMessage(0, 0, 0, "StartRESTServer", $"Added discovery listener on {(uni.Address.IsIPv6LinkLocal ? $"link local address {uni.Address} with index {uni.Address.ScopeId}." : $"loopback address {uni.Address}.")}");

                                                    // Start listening for IPv6 broadcasts on this interface
                                                    udpClientV6.BeginReceive(DiscoveryCallback, udpClientV6);
                                                }
                                                else
                                                {
                                                    if (uni.Address.AddressFamily == AddressFamily.InterNetworkV6)
                                                    {
                                                        LogMessage(0, 0, 0, "StartRESTServer", $"Ignoring address {uni.Address} because it is global.");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            LogMessage(0, 0, 0, "StartRESTServer", $"Listening for IPv6 discovery broadcasts on port {AlpacaDiscoveryPort}.");
                            LogBlankLine(0, 0, 0);

                            LogToScreen($"Listening for IPv6 discovery broadcasts on port {AlpacaDiscoveryPort}.");
                        }
                        else
                        {
                            LogToScreen($"IPv6 discovery not enabled.");
                        }
                    }
                    else // Discovery is not enabled
                    {
                        LogMessage(0, 0, 0, "StartRESTServer", $"NOT listening for discovery broadcasts because this is DISABLED by configuration.");
                        LogToScreen($"NOT Listening for discovery broadcasts");
                    }

                    LogMessage(0, 0, 0, "StartRESTServer", "Server started successfully.");
                    LogToScreen("Server started successfully.");

                }
                catch (Exception ex)
                {
                    LogException(0, 0, 0, "StartRESTServer", ex.ToString());
                    LogToScreen("Exception while attempting to start the API or discovery listeners: " + ex.Message);
                }
            }
        }

        private void CloseIpV6BroadcastListeners()
        {
            // Close any current listeners
            foreach (UdpClient client in discoveryClientsIpV6)
            {
                try { client.Close(); } catch { }
                try { client.Dispose(); } catch { }
            }

            // Remove any current, and now closed, listeners
            discoveryClientsIpV6.Clear();
        }

        private void CloseIpV4BroadcastListeners()
        {
            // Close any current listeners
            foreach (UdpClient client in discoveryClientsIpV4)
            {
                try { client.Close(); } catch { }
                try { client.Dispose(); } catch { }
            }

            // Remove any current, and now closed, listeners
            discoveryClientsIpV4.Clear();
        }

        private void StopRESTServer()
        {
            if (this.InvokeRequired)
            {
                LogMessage(0, 0, 0, "StopRESTServer", "Invoke required...");
                this.Invoke(new Action(() => this.StopRESTServer()));
            }
            else
            {
                if (httpListener != null) // Close and dispose of the current listener, if there is one.
                {
                    // Close the IPv4 and IPv6 discovery UDP listeners
                    CloseIpV4BroadcastListeners();
                    CloseIpV6BroadcastListeners();

                    // Create variables to hold the ASCOM device server operating URIs
                    string apiOperatingUri = string.Format(@"http://+:{0}{1}", ServerPortNumber, SharedConstants.API_URL_BASE);
                    string remoteServerManagementUri = string.Format(@"http://+:{0}{1}", ServerPortNumber, SharedConstants.REMOTE_SERVER_MANAGEMENT_URL_BASE);
                    string alpacaDeviceManagementUri = string.Format(@"http://+:{0}{1}", ServerPortNumber, SharedConstants.ALPACA_DEVICE_MANAGEMENT_URL_BASE);
                    string alpacaDeviceSetupUri = string.Format(@"http://+:{0}{1}", ServerPortNumber, SharedConstants.ALPACA_DEVICE_SETUP_URL_BASE);

                    LogMessage(0, 0, 0, "StopRESTServer", "Removing Prefixes");
                    try { httpListener.Prefixes.Remove(apiOperatingUri); } catch { } // Remove the listener on the api URI
                    try { httpListener.Prefixes.Remove(remoteServerManagementUri); } catch { } // Remove the listener on the remote server bespoke management command URI
                    try { httpListener.Prefixes.Remove(alpacaDeviceManagementUri); } catch { }  // Remove the listener on the management URI common to all Alpaca devices
                    try { httpListener.Prefixes.Remove(alpacaDeviceSetupUri); } catch { }  // Remove the listener on the HTTP Setup URI common to all Alpaca devices

                    LogMessage(0, 0, 0, "StopRESTServer", "Stopping the Remote server");
                    try { httpListener.Stop(); } catch { }
                    try { httpListener.Close(); } catch { }
                    try { httpListener = null; } catch { }
                }

                apiIsEnabled = false;
                LblRESTStatus.BackColor = Color.Red;
                LblRESTStatus.Text = "Remote Server Down";
            }
        }

        private void ConnectDevices()
        {
            if (this.InvokeRequired)
            {
                LogMessage(0, 0, 0, "ConnectDevices", "Invoke required...");
                this.Invoke(new Action(() => this.ConnectDevices()));
            }
            else
            {
                try
                {
                    DisconnectDevices(); // Shut down all the ASCOM device instances
                    ActiveObjects.Clear();
                    GC.Collect();
                    LogBlankLine(0, 0, 0);

                    // Create new ASCOM device instances
                    foreach (KeyValuePair<string, ConfiguredDevice> configuredDevice in ConfiguredDevices)
                    {
                        if ((configuredDevice.Value.ProgID != SharedConstants.DEVICE_NOT_CONFIGURED) & (configuredDevice.Value.ProgID.Trim(" ".ToCharArray()) != "")) // Only attempt to process devices with valid ProgIDs
                        {
                            try
                            {

                                // Create an active object for this device
                                ActiveObjects[configuredDevice.Value.DeviceKey] = new ActiveObject(configuredDevice.Value.AllowConnectedSetFalse, configuredDevice.Value.AllowConnectedSetTrue, configuredDevice.Value.AllowConcurrentAccess, configuredDevice.Key);

                                if (RunDriversOnSeparateThreads)
                                {
                                    LogMessage(0, 0, 0, "ConnectDevices", string.Format("Creating driver {0} on separate thread. This is thread: {1}", configuredDevice.Value.ProgID, Thread.CurrentThread.ManagedThreadId));
                                    Thread driverThread = new Thread(DriverOnSeparateThread);
                                    driverThread.SetApartmentState(ApartmentState.STA);
                                    driverThread.DisableComObjectEagerCleanup();
                                    driverThread.IsBackground = true;
                                    driverThread.Start(configuredDevice);
                                    LogMessage(0, 0, 0, "ConnectDevices", string.Format("Thread started successfully for {0}. This is thread: {1}", configuredDevice.Value.ProgID, Thread.CurrentThread.ManagedThreadId));

                                    string deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber);

                                    do
                                    {
                                        Thread.Sleep(50);
                                        Application.DoEvents();
                                    } while (ActiveObjects[deviceKey].DriverHostForm == null);

                                    LogMessage(0, 0, 0, "ConnectDevices", string.Format("Completed create driver delegate for {0} on thread {1}", configuredDevice.Value.ProgID, Thread.CurrentThread.ManagedThreadId));
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
                            LogBlankLine(0, 0, 0);
                        }
                    }

                    devicesAreConnected = true;

                    LblDriverStatus.BackColor = Color.Green; // Turn the "Connected / Disconnected" colour box green
                    LblDriverStatus.Text = "Drivers Connected";
                }
                catch (Exception ex)
                {
                    LogToScreen("Exception while attempting to create devices: " + ex.Message);
                    LogException(0, 0, 0, "ConnectDevices", ex.ToString());
                }
            }
        }
        /// <summary>
        /// This method is called on receipt of both IPv4 broadcasts and IPv6 multi casts
        /// </summary>
        /// <param name="ar"></param>
        private void DiscoveryCallback(IAsyncResult ar)
        {
            // Increment the discovery packet number so that all log messages from this particular event can be recognised even if they come out of order due to handling multiple events concurrently
            int discoveryNumber = Interlocked.Increment(ref discoveryCount);

            try
            {

                // Present the supplied state value as a UDP client
                UdpClient udpClient = (UdpClient)ar.AsyncState;

                // Create a dummy value into which the actual endpoint can be placed by the UdpClient.EndReceive method
                IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, (int)AlpacaDiscoveryPort);

                // Convert the UDP message body to a string
                string ReceiveString = Encoding.ASCII.GetString(udpClient.EndReceive(ar, ref remoteEndpoint));

                // Get the IP address of the interface on this PC that received the message
                IPEndPoint localIpEndPoint = (IPEndPoint)udpClient.Client.LocalEndPoint;

                // Filter based on receiving port IP address 
                if ((ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG) | (ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG)) // The Remote Server is configured to respond on all its IP addresses so filtering is not required here
                {
                    if (DebugTraceState) LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"The configured server IP address is + or * so we will process this request.");
                }
                else  // The Remote Server is configured to respond on one specific address rather than on all of its IP addresses so test whether the received message is on the selected IP address
                {
                    if (DebugTraceState) LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"Request received on endpoint: {localIpEndPoint} - Enabled endpoint: {new IPEndPoint(IPAddress.Parse(ServerIPAddressString), (int)AlpacaDiscoveryPort)}");
                    if (localIpEndPoint.ToString() != new IPEndPoint(IPAddress.Parse(ServerIPAddressString), (int)AlpacaDiscoveryPort).ToString()) // IP Addresses don't match so we just ignore the request
                    {
                        if (DebugTraceState) LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"  The endpoint is NOT enabled - This request will be dropped.");
                        // Continue to listen for discovery packets and don't process this request further
                        udpClient.BeginReceive(DiscoveryCallback, udpClient);
                        return;
                    }
                    else // IP addresses do match so we can process this request
                    {
                        if (DebugTraceState) LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"  The endpoint is ENABLED - This request will be processed");
                    }
                }

                if (ServerForm.DebugTraceState) ServerForm.LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"Received UDP broadcast: {ReceiveString}"); // Log received broadcasts if we are debug tracing

                // Filter based on received message content
                // Validate and process the received datagram if it is a valid Alpaca discovery broadcast
                if (ReceiveString.StartsWith(SharedConstants.ALPACA_DISCOVERY_BROADCAST_ID)) // The first 15 characters are "alpacadiscovery" so this is an Alpaca discovery packet
                {
                    // Extract the discovery packet version number so it can be recorded
                    char discoveryBroadcastVersionNumber = '0'; // Initialise to the default "missing" version number of "0"
                    if (ReceiveString.Length > SharedConstants.ALPACA_DISCOVERY_BROADCAST_ID.Length) // There is at least one character more, which should be the broadcast version number so extract this as well
                    {
                        discoveryBroadcastVersionNumber = ReceiveString.ToCharArray(SharedConstants.ALPACA_DISCOVERY_BROADCAST_ID.Length, 1)[0];
                    }

                    ServerForm.LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"Received a version {discoveryBroadcastVersionNumber} (0x{(int)Char.GetNumericValue(discoveryBroadcastVersionNumber):X}) discovery packet from the client IP address {remoteEndpoint.Address} of type {remoteEndpoint.AddressFamily}. Returning Alpaca port number: {ServerPortNumber}");

                    // Create a discovery response, convert it to JSON and return this to the caller
                    AlpacaDiscoveryResponse alpacaDiscoveryResponse = new AlpacaDiscoveryResponse((int)ServerPortNumber); // Create the response object

                    string jsonResponse = JsonConvert.SerializeObject(alpacaDiscoveryResponse); // Convert the response object to a JSON string
                    ServerForm.LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"JSON Discovery response: {jsonResponse}");

                    byte[] response = Encoding.ASCII.GetBytes(jsonResponse); // Convert the JSON string to a byte array and send this back to the caller
                    udpClient.Send(response, response.Length, remoteEndpoint);
                }

                // Continue to listen for discovery packets
                udpClient.BeginReceive(DiscoveryCallback, udpClient);
            }
            catch (ObjectDisposedException)
            {
                LogMessage((uint)discoveryNumber, 0, 0, "DiscoveryCallback", "Alpaca listener has closed down, ignoring this call.");
            }
            catch (Exception ex)
            {
                LogException((uint)discoveryNumber, 0, 0, "DiscoveryCallback", $"Unexpected exception: {ex}");
            }
        }

        private void DriverOnSeparateThread(object arg)
        {
            KeyValuePair<string, ConfiguredDevice> configuredDevice = (KeyValuePair<string, ConfiguredDevice>)arg; // Convert the supplied argument to the correct type

            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("About to create driver host form"));
            DriverHostForm driverHostForm = new DriverHostForm(configuredDevice, this); // Create the form
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Created driver host form"));
            driverHostForm.Show(); // Make it come into existence - it doesn't exist until its shown for some reason
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Shown driver host form"));
            driverHostForm.Hide(); // Hide the form from view
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Hidden driver host form"));

            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Starting driver host environment for {0} on thread {1}", configuredDevice.Value.DeviceKey, Thread.CurrentThread.ManagedThreadId));
            Application.Run();  // Start the message loop on this thread to bring the form to life
            LogMessage(0, 0, 0, "DriverOnSeparateThread", string.Format("Environment for driver host {0} shut down on thread {1}", configuredDevice.Value.DeviceKey, Thread.CurrentThread.ManagedThreadId));
            driverHostForm.Dispose();

        }

        internal void CreateInstance(KeyValuePair<string, ConfiguredDevice> configuredDevice)
        {
            try
            {
                LogMessage(0, 0, 0, "CreateInstance", string.Format("Creating device: {0}, ProgID: {1}, Key: {2} on thread {3}", configuredDevice.Key, configuredDevice.Value.ProgID, configuredDevice.Value.DeviceKey, Thread.CurrentThread.ManagedThreadId));

                // Create the device and save its reference to the ActiveObject instance
                Type deviceType = Type.GetTypeFromProgID(configuredDevice.Value.ProgID);

                if (deviceType != null) // Attempt to create the driver object if we successfully got a Type from Type.GetTypeFromProgID
                {
                    dynamic deviceObject = Activator.CreateInstance(deviceType);
                    ActiveObjects[configuredDevice.Value.DeviceKey].DeviceObject = deviceObject;
                    LogMessage(0, 0, 0, "CreateInstance", string.Format("Device {0} created OK!", configuredDevice.Value.ProgID));

                    // make sure that a device object was actually returned
                    if (ActiveObjects[configuredDevice.Value.DeviceKey].DeviceObject != null) // A device object was returned
                    {
                        try
                        {
                            LogMessage(0, 0, 0, "CreateInstance", string.Format("Connecting device {0}", configuredDevice.Value.ProgID));
                            try
                            {
                                ActiveObjects[configuredDevice.Value.DeviceKey].DeviceObject.Connected = true;
                            }
                            catch (Exception ex2) when (configuredDevice.Value.DeviceType.ToLowerInvariant() == "focuser")
                            {
                                LogException(0, 0, 0, "CreateInstance", $"Error setting Connected to true for focuser device {configuredDevice.Value.ProgID} now trying Link for IFocuserV1 devices: \r\n{ex2}");
                                ActiveObjects[configuredDevice.Value.DeviceKey].DeviceObject.Link = true;
                            }
                            ActiveObjects[configuredDevice.Value.DeviceKey].InitialisedOk = true; // Set flag indicating that this device initialised and connected OK
                            LogMessage(0, 0, 0, "CreateInstance", string.Format("Device {0} connected OK", configuredDevice.Value.ProgID));
                            LogToScreen(string.Format("Device {0} {1} ({2}) connected OK and is available", configuredDevice.Value.DeviceType, configuredDevice.Value.DeviceNumber, configuredDevice.Value.ProgID));
                        }
                        catch (Exception ex1) // Exception on setting Connected to True
                        {
                            ActiveObjects[configuredDevice.Value.DeviceKey].InitialisationErrorMessage = string.Format("Device {0} is unavailable - it threw an error while being connected: {1}", configuredDevice.Value.ProgID, ex1.Message);
                            LogException(0, 0, 0, "CreateInstance", string.Format("Error connecting to device {0}: \r\n{1}", configuredDevice.Value.ProgID, ex1.ToString()));
                            LogToScreen(string.Format("ERROR - Device {0} {1} ({2}) failed to connect and is NOT available: {3}", configuredDevice.Value.DeviceType, configuredDevice.Value.DeviceNumber, configuredDevice.Value.ProgID, ex1.Message));
                        }
                    }
                    else // No device object was returned so flag that an error occurred
                    {
                        ActiveObjects[configuredDevice.Value.DeviceKey].InitialisationErrorMessage = string.Format("Device {0} is unavailable - no device was returned from CreateInstance but no exception was generated either!", configuredDevice.Value.ProgID);
                        LogMessage(0, 0, 0, "CreateInstance", string.Format("Device created OK - ActiveObjects.DeviceObject is null: {0}", (ActiveObjects[configuredDevice.Value.DeviceKey].DeviceObject == null)));
                        LogToScreen(string.Format("ERROR - Device {0} {1} ({2}) could not be created and is NOT available. It did not create an exception or error message!", configuredDevice.Value.DeviceType, configuredDevice.Value.DeviceNumber, configuredDevice.Value.ProgID));
                    }
                }
                else // We did not get a Type from Type.GetTypeFromProgID, it returned null. Record an error instead
                {
                    ActiveObjects[configuredDevice.Value.DeviceKey].InitialisationErrorMessage = string.Format("Device {0} is unavailable - unable to create a Type from its ProgID. This implies that the driver may not be correctly registered for COM.", configuredDevice.Value.ProgID);
                    LogMessage(0, 0, 0, "CreateInstance", string.Format("Device {0} is unavailable - unable to create a Type from its ProgID. This implies that the driver may not be correctly registered for COM.", configuredDevice.Value.ProgID));
                    LogToScreen(string.Format("ERROR - Device {0} {1} ({2}) could not be created and is NOT available: Unable to create a Type from its ProgID. This implies that the driver may not be correctly registered for COM.", configuredDevice.Value.DeviceType, configuredDevice.Value.DeviceNumber, configuredDevice.Value.ProgID));
                }
            }
            catch (Exception ex) // Exception when creating device
            {
                ActiveObjects[configuredDevice.Value.DeviceKey].InitialisationErrorMessage = string.Format("Device {0} is unavailable - it threw an error while being created: {1}", configuredDevice.Value.ProgID, ex.Message);
                LogException(0, 0, 0, "CreateInstance", "Error creating device: \r\n" + ex.ToString());
                LogToScreen(string.Format("ERROR - Device {0} {1} ({2}) could not be created and is NOT available: {3}", configuredDevice.Value.DeviceType, configuredDevice.Value.DeviceNumber, configuredDevice.Value.ProgID, ex.Message));
            }
        }

        private void DisconnectDevices()
        {
            LogMessage(0, 0, 0, "DisconnectDevices", "Entered DisconnectDevices()");
            if (this.InvokeRequired)
            {
                LogMessage(0, 0, 0, "DisconnectDevices", "Invoke required...");
                this.Invoke(new Action(() => this.DisconnectDevices()));
                LogMessage(0, 0, 0, "DisconnectDevices", "Invoke completed.");
            }
            else
            {
                LogBlankLine(0, 0, 0);
                LogMessage(0, 0, 0, "DisconnectDevices", "Clearing devices");
                int RemainingObjectCount;

                LblDriverStatus.BackColor = Color.Red; // Turn the "Connected / Disconnected" colour box red
                LblDriverStatus.Text = "Drivers Unloaded";
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
                            LogMessage(0, 0, 0, "DisconnectDevices", string.Format("Drivers on same thread - destroying device {0} on thread {1}", activeObject.Key, Thread.CurrentThread.ManagedThreadId));
                            RemainingObjectCount = DestroyDriver(activeObject.Key);
                        }
                    }
                    catch (KeyNotFoundException) { } // Ignore key not found exceptions, they are expected for unconfigured devices

                    catch (Exception ex)
                    {
                        LogException(0, 0, 0, "DisconnectDevices", "  ReleaseComObject Exception: " + ex.ToString());
                    }
                    LogBlankLine(0, 0, 0);
                }

                ActiveObjects.Clear(); // Clear the list of active objects now that all active device instances have been destroyed

                GC.Collect(); // Reclaim memory and destroy all deleted objects
            }
        }

        internal static int DestroyDriver(string DeviceKey)
        {
            int RemainingObjectCount;

            LogMessage(0, 0, 0, "DestroyDriver", string.Format("Destroying driver: {0}", DeviceKey));

            device = ActiveObjects[DeviceKey].DeviceObject;

            if (DebugTraceState) LogMessage(0, 0, 0, "DestroyDriver", string.Format("Before setting Connected false for driver: {0}", DeviceKey));
            try { device.Connected = false; } catch { }// Don't throw exceptions from these

            if (DebugTraceState) LogMessage(0, 0, 0, "DestroyDriver", string.Format("Before setting Link false for driver: {0}", DeviceKey));
            try { device.Link = false; } catch { }

            if (DebugTraceState) LogMessage(0, 0, 0, "DestroyDriver", string.Format("Before calling Dispose() for driver: {0}", DeviceKey));
            try { device.Dispose(); } catch { }

            // Now destroy the device instance
            int LoopCount = 0;
            RemainingObjectCount = 0;

            if (device != null)
            {
                do
                {
                    LoopCount += 1;
                    if (DebugTraceState) LogMessage(0, 0, 0, "DestroyDriver", string.Format("Before calling ReleaseComObject() for driver: {0}", DeviceKey));
                    try { RemainingObjectCount = Marshal.ReleaseComObject(device); } catch { } // Don't throw exceptions from this
                    LogMessage(0, 0, 0, "DestroyDriver", string.Format("Remaining {0} count: {1}, LoopCount: {2}", DeviceKey, RemainingObjectCount, LoopCount));
                }
                while ((RemainingObjectCount > 0) & (LoopCount != 20));
                device = null;
            }

            LogMessage(0, 0, 0, "DestroyDriver", string.Format("Destroyed driver: {0}", DeviceKey));

            return RemainingObjectCount;
        }

        internal static void WaitFor(int Duration)
        {
            const int SLEEP_TIME = 20;
            int remainingDuration;

            remainingDuration = Duration;

            if (DebugTraceState) LogMessage(0, 0, 0, "WaitFor", string.Format("Starting wait for {0} milli-seconds", Duration));
            do
            {
                remainingDuration -= SLEEP_TIME;
                Thread.Sleep(SLEEP_TIME);
                Application.DoEvents();

            } while (remainingDuration > 0);
            if (DebugTraceState) LogMessage(0, 0, 0, "WaitFor", string.Format("Completed wait for {0} milli-seconds", Duration));
        }

        internal static void LogMessage(uint clientID, uint clientTransactionID, uint serverTransactionID, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.LogMessageCrLf(clientID, clientTransactionID, serverTransactionID, Method, Message);
            }
        }

        internal static void LogMessage1(RequestData requestData, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID);
                TL.LogMessageCrLf(requestData, Method, Message);
            }
        }

        internal static void LogException(uint clientID, uint clientTransactionID, uint serverTransactionID, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.LogMessageCrLf(clientID, clientTransactionID, serverTransactionID, Method, Message);
            }
        }

        internal static void LogException1(RequestData requestData, string Method, string Message)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID);
                TL.LogMessageCrLf(requestData, Method, Message);
            }
        }

        internal static void LogBlankLine(uint clientID, uint clientTransactionID, uint serverTransactionID)
        {
            lock (logLockObject) // Ensure that only one message is logged at once and that the midnight log change over is effected within just one log message call
            {
                CheckWhetherNewLogRequired(clientID, clientTransactionID, serverTransactionID);
                TL.BlankLine();
            }
        }

        private static void CheckWhetherNewLogRequired(uint clientID, uint clientTransactionID, uint serverTransactionID)
        {
            if (TL.Enabled) // We are logging so we have to close the current log and start a new one if we have moved to a new day
            {
                DateTime now = DateTime.Now;

                //TL.LogMessage("CheckWhetherNewLogRequired", $"Next roll-over time: {NextRolloverTime}, Current time: {now}");

                if (now > NextTraceLogRolloverTime) // We have moved into the next log period so close the current log and start another
                {
                    // Close the current logger
                    TL.LogMessage(clientID, clientTransactionID, serverTransactionID, "EndOfDay", "Closing this log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));
                    TL.Enabled = false;
                    TL.Dispose();
                    TL = null;

                    // Start a new logger
                    TL = new TraceLoggerPlus("", SERVER_TRACELOGGER_NAME)
                    {
                        LogFilePath = TraceFolder,
                        Enabled = true, // Enable the trace logger
                        IpAddressTraceState = LogClientIPAddress // Set the current state of the "include client IP address in trace lines" flag
                    };

                    TL.LogMessage(clientID, clientTransactionID, serverTransactionID, "StartOfDay", "Opening a new log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));

                    NextTraceLogRolloverTime = NextTraceLogRolloverTime.AddDays(1.0); // Update the next roll-over time so that it can be tested on the next logging call
                }
            }
        }

        private static void WriteConfigurationToLog()
        {
            LogBlankLine(0, 0, 0);
            LogMessage(0, 0, 0, "Configuration", "Variables");
            LogMessage(0, 0, 0, "Trace State", TraceState.ToString());
            LogMessage(0, 0, 0, "Debug Trace State", DebugTraceState.ToString());
            LogMessage(0, 0, 0, "Server IP Address", ServerIPAddressString);
            LogMessage(0, 0, 0, "Server IP Port", ServerPortNumber.ToString());
            LogMessage(0, 0, 0, "Start Connected", StartWithDevicesConnected.ToString());
            LogMessage(0, 0, 0, "Access Log Enabled", AccessLogEnabled.ToString());
            LogMessage(0, 0, 0, "Log Request To Screen", ScreenLogRequests.ToString());
            LogMessage(0, 0, 0, "Log Response To Screen", ScreenLogResponses.ToString());
            LogMessage(0, 0, 0, "Management Interface", ManagementInterfaceEnabled.ToString());
            LogMessage(0, 0, 0, "Start With API Enabled", StartWithApiEnabled.ToString());
            LogMessage(0, 0, 0, "Use Separate Threads", RunDriversOnSeparateThreads.ToString());
            LogMessage(0, 0, 0, "Log Client IP Address", LogClientIPAddress.ToString());
            LogMessage(0, 0, 0, "Include Exceptions", IncludeDriverExceptionInJsonResponse.ToString());
            LogMessage(0, 0, 0, "Location", RemoteServerLocation);
            LogMessage(0, 0, 0, "Alpaca Discovery", AlpacaDiscoveryEnabled.ToString());
            LogMessage(0, 0, 0, "Alpaca Unique ID", AlpacaUniqueId);
            LogMessage(0, 0, 0, "Alpaca Discovery Port", AlpacaDiscoveryPort.ToString());
            LogMessage(0, 0, 0, "Maximum Devices", MaximumNumberOfDevices.ToString());
            LogMessage(0, 0, 0, "IpV4 Enabled", IpV4Enabled.ToString());
            LogMessage(0, 0, 0, "IpV6 Enabled", IpV6Enabled.ToString());
            LogMessage(0, 0, 0, "Rollover Logs Enabled", RolloverLogsEnabled.ToString());
            LogMessage(0, 0, 0, "Rollover Time", RolloverTime.TimeOfDay.ToString());
            LogMessage(0, 0, 0, "Use UTC in Logs", UseUtcTimeInLogs.ToString());

            LogBlankLine(0, 0, 0);

            LogMessage(0, 0, 0, "CORS Support Enabled", CorsSupportIsEnabled.ToString());
            LogMessage(0, 0, 0, "CORS Max Age", CorsMaxAge.ToString());
            LogMessage(0, 0, 0, "CORS Credentials", CorsCredentialsPermitted.ToString());
            foreach (string origin in CorsPermittedOrigins)
            {
                LogMessage(0, 0, 0, "CORS Origin", origin);
            }

            LogBlankLine(0, 0, 0);

            foreach (string deviceName in ServerDeviceNames)
            {
                LogMessage(0, 0, 0, deviceName, $"Device Type               = {ConfiguredDevices[deviceName].DeviceType}");
                LogMessage(0, 0, 0, deviceName, $"ProgID                    = {ConfiguredDevices[deviceName].ProgID}");
                LogMessage(0, 0, 0, deviceName, $"Description               = {ConfiguredDevices[deviceName].Description}");
                LogMessage(0, 0, 0, deviceName, $"Device Number             = {ConfiguredDevices[deviceName].DeviceNumber}");
                LogMessage(0, 0, 0, deviceName, $"Allow Connected Set False = {ConfiguredDevices[deviceName].AllowConnectedSetFalse}");
                LogMessage(0, 0, 0, deviceName, $"Allow Connected Set True  = {ConfiguredDevices[deviceName].AllowConnectedSetTrue}");
                LogMessage(0, 0, 0, deviceName, $"Allow Concurrent Access   = {ConfiguredDevices[deviceName].AllowConcurrentAccess}");
                LogBlankLine(0, 0, 0);
            }
        }

        /// <summary>
        /// Display a message on the screen log
        /// </summary>
        /// <param name="screenMessage">Message to be displayed</param>
        /// <remarks>The log will be limited to a total length of SCREEN_LOG_MAXIMUM_LENGTH and the new message must be less than or equal to SCREEN_LOG_MAXIMUM_MESSAGE_LENGTH characters in length</remarks>
        internal void LogToScreen(string screenMessage)
        {
            // Protect against attempting to log a message while the application is being closed or disposed.
            if (txtLog != null)
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
            string cleanedMessage = Regex.Replace(message, @"[^\u0020-\u007E]", string.Empty);
            return cleanedMessage;
        }

        /// <summary>
        /// Releases all drivers and shuts down the Remote Server
        /// </summary>
        private void ShutdownTask()
        {
            if (this.InvokeRequired)
            {
                LogMessage(0, 0, 0, "ShutdownTask", "Invoke required...");
                this.BeginInvoke(new Action(() => ShutdownTask()));
                LogMessage(0, 0, 0, "ShutdownTask", "Completed Invoke.");
            }
            else
            {
                LogMessage(0, 0, 0, "ShutdownTask", $"Sleeping to allow shutdown command response to be returned to client");
                Thread.Sleep(500);

                LogMessage(0, 0, 0, "ShutdownTask", $"Closing Remote Server form");
                this.Close();
                LogMessage(0, 0, 0, "ShutdownTask", $"Closed Remote Server form");
            }
        }

        /// <summary>
        /// Return HTML or plain text version of the error message depending on whether the client accepts text/html
        /// </summary>
        /// <param name="requestData">The client request</param>
        /// <param name="htmlText">HTML formatted text to return to clients that request this</param>
        /// <param name="plainText">Plain text to be returned to clients that do not specifically request html</param>
        /// <returns></returns>
        private string HTMLOrPlainText(RequestData requestData, string htmlText, string plainText)
        {
            // Test whether the client requests HTML messages
            if (requestData.Request.AcceptTypes != null) // There are some accept types so check whether text/html is among them
            {
                if (requestData.Request.AcceptTypes.Contains<string>("text/html"))  // Client can handle HTML encoding so return HTML version of the message
                {
                    return htmlText;
                }
                else // Client can not handle html encoding so return plain text
                {
                    return plainText;
                }
            }
            else // There are no accept types so assume plain text is appropriate
            {
                return plainText;
            }
        }

        private string HTMLOrPlainTextContentType(RequestData requestData)
        {
            if (requestData.Request.AcceptTypes != null) // There are some accept types so check whether text/html is among them
            {
                // Test whether the client requests HTML or plain text responses
                if (requestData.Request.AcceptTypes.Contains<string>("text/html"))  // Client can handle HTML encoding so return HTML content type
                {
                    return "text/html; charset=utf-8";
                }
                else // Client can not handle html encoding so return plain text content type
                {
                    return "text/plain; charset=utf-8";
                }
            }
            else // There are no accept types so assume plain text is appropriate
            {
                return "text/plain; charset=utf-8";
            }
        }

        /// <summary>
        /// Returns the size of a structure in managed memory
        /// </summary>
        /// <param name="type">The type of the object whose size is to be determined</param>
        /// <returns>
        /// GetManagedSize() returns the size of a structure whose type is 'type', as stored in managed memory.
        /// For any reference type this will simply return the size of a pointer (4 or 8).
        /// </returns>
        public static int GetManagedSize(Type type)
        {
            // all this just to invoke one op code with no arguments!
            var method = new DynamicMethod("GetManagedSizeImpl", typeof(uint), new Type[0]); //, typeof(TypeExtensions), false);

            ILGenerator gen = method.GetILGenerator();

            gen.Emit(OpCodes.Sizeof, type);
            gen.Emit(OpCodes.Ret);

            var func = (Func<uint>)method.CreateDelegate(typeof(Func<uint>));
            return checked((int)func());
        }

        /// <summary>
        /// Forces a long lived item (generation 2) and a large item (generation 3) garbage collection and waits for completion.
        /// </summary>
        /// <returns>The time taken by the garbage collection in milli-seconds.</returns>
        public static double ForceGarbageCollection()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            GC.Collect(2, GCCollectionMode.Forced, true);
            return sw.Elapsed.TotalMilliseconds;
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
                TL.Enabled = TraceState; // Set the trace state here so that it can be used below

                TraceFolder = driverProfile.GetValue<string>(SERVER_LOG_FOLDER_PROFILENAME, string.Empty, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ASCOM");
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
                LogClientIPAddress = driverProfile.GetValue<bool>(LOG_CLIENT_IPADDRESS_PROFILENAME, string.Empty, LOG_CLIENT_IPADDRESS_DEFAULT);
                TL.IpAddressTraceState = LogClientIPAddress; // Persist the IP address trace state to the TraceLogger so that it can be used to format lines as required
                IncludeDriverExceptionInJsonResponse = driverProfile.GetValue<bool>(INCLUDE_DRIVEREXCEPTION_IN_JSON_RESPONSE_PROFILENAME, string.Empty, INCLUDE_DRIVEREXCEPTION_IN_JSON_RESPONSE_DEFAULT);
                RemoteServerLocation = driverProfile.GetValue<string>(REMOTE_SERVER_LOCATION_PROFILENAME, string.Empty, REMOTE_SERVER_LOCATION_DEFAULT);
                CorsPermittedOrigins.FromConcatenatedString(driverProfile.GetValue<string>(CORS_PERMITTED_ORIGINS_PROFILENAME, string.Empty, CORS_PERMITTED_ORIGINS_DEFAULT), SharedConstants.CORS_SERIALISATION_SEPARATOR);
                CorsSupportIsEnabled = driverProfile.GetValue<bool>(CORS_SUPPORT_ENABLED_PROFILENAME, string.Empty, CORS_SUPPORT_ENABLED_DEFAULT);
                CorsMaxAge = driverProfile.GetValue<decimal>(CORS_MAX_AGE_PROFILENAME, string.Empty, CORS_MAX_AGE_DEFAULT);
                CorsCredentialsPermitted = driverProfile.GetValue<bool>(CORS_CREDENTIALS_PERMITTED_PROFILENAME, string.Empty, CORS_CREDENTIALS_PERMITTED_DEFAULT);
                AlpacaDiscoveryEnabled = driverProfile.GetValue<bool>(ALPACA_DISCOVERY_ENABLED_PROFILENAME, string.Empty, ALPACA_DISCOVERY_ENABLED_DEFAULT);
                AlpacaUniqueId = driverProfile.GetValue<string>(ALPACA_UNIQUE_ID_PROFILENAME, string.Empty, ALPACA_UNIQUE_ID_DEFAULT);
                AlpacaDiscoveryPort = driverProfile.GetValue<decimal>(ALPACA_DISCOVERY_PORT_PROFILENAME, string.Empty, ALPACA_DISCOVERY_PORT_DEFAULT);
                MaximumNumberOfDevices = driverProfile.GetValue<int>(MAXIMUM_NUMBER_OF_DEVICES_PROFILENAME, string.Empty, MAXIMUM_NUMBER_OF_DEVICES_DEFAULT);
                IpV4Enabled = driverProfile.GetValue<bool>(IPV4_ENABLED_PROFILENAME, string.Empty, IPV4_ENABLED_DEFAULT);
                IpV6Enabled = driverProfile.GetValue<bool>(IPV6_ENABLED_PROFILENAME, string.Empty, IPV6_ENABLED_DEFAULT);
                RolloverLogsEnabled = driverProfile.GetValue<bool>(ROLLOVER_LOGS_ENABLED_PROFILENAME, string.Empty, ROLLOVER_LOGS_ENABLED_DEFAULT);
                RolloverTime = driverProfile.GetValue<DateTime>(ROLLOVER_TIME_PROFILENAME, string.Empty, ROLLOVER_TIME_DEFAULT);
                UseUtcTimeInLogs = driverProfile.GetValue<bool>(USE_UTC_TIME_IN_LOGS_PROFILENAME, string.Empty, USE_UTC_TIME_IN_LOGS_ENABLED_DEFAULT);
                MinimiseToSystemTray = driverProfile.GetValue<bool>(MINIMISE_TO_SYSTEM_TRAY_PROFILENAME, string.Empty, MINIMISE_TO_SYSTEM_TRAY_DEFAULT);
                ConfirmExit = driverProfile.GetValue<bool>(CONFIRM_EXIT_PROFILENAME, string.Empty, CONFIRM_EXIT_DEFAULT);
                StartMinimised = driverProfile.GetValue<bool>(MINIMISE_ON_START_PROFILENAME, string.Empty, MINIMISE_ON_START_DEFAULT);

                // Set the next log roll-over time using the persisted roll-over time value
                SetNextRolloverTime();

                // Clear collections before repopulating
                ServerDeviceNumbers.Clear();
                ServerDeviceNames.Clear();

                // Populate the server device numbers and names based on the MaximumNumberOfDevices value
                for (int i = 0; i < MaximumNumberOfDevices; i++)
                {
                    ServerDeviceNumbers.Add(i.ToString());
                    ServerDeviceNames.Add($"ServedDevice{i:00}");
                }

                // Clear collection before repopulating
                ConfiguredDevices.Clear();

                foreach (string deviceName in ServerDeviceNames)
                {
                    // Backward compatibility fix for early releases that only hosted 10 devices
                    string revisedDeviceName = FixDeviceName(deviceName);

                    string deviceType = driverProfile.GetValue<string>(DEVICETYPE_PROFILENAME, revisedDeviceName, DEVICETYPE_DEFAULT);
                    string progID = driverProfile.GetValue<string>(PROGID_PROFILENAME, revisedDeviceName, PROGID_DEFAULT);
                    string description = driverProfile.GetValue<string>(DESCRIPTION_PROFILENAME, revisedDeviceName, DESCRIPTION_DEFAULT);
                    int deviceNumber = Convert.ToInt32(driverProfile.GetValue<int>(DEVICENUMBER_PROFILENAME, revisedDeviceName, DEVICENUMBER_DEFAULT));
                    bool allowConnectedSetFalse = Convert.ToBoolean(driverProfile.GetValue<bool>(ALLOW_CONNECTED_SET_FALSE_PROFILENAME, revisedDeviceName, ALLOW_CONNECTED_SET_FALSE_DEFAULT));
                    bool allowConnectedSetTrue = Convert.ToBoolean(driverProfile.GetValue<bool>(ALLOW_CONNECTED_SET_TRUE_PROFILENAME, revisedDeviceName, ALLOW_CONNECTED_SET_TRUE_DEFAULT));
                    bool allowConcurrentAccess = Convert.ToBoolean(driverProfile.GetValue<bool>(ALLOW_CONCURRENT_ACCESS_PROFILENAME, revisedDeviceName, ALLOW_CONCURRENT_ACCESS_DEFAULT));
                    string uniqueID = driverProfile.GetValue<string>(DEVICE_UNIQUE_ID_PROFILENAME, revisedDeviceName, SharedConstants.DEVICE_NOT_CONFIGURED);

                    // Upgrade earlier data stores that did not have the UniqueID field by assigning a global ID if this device is configured but has no global ID
                    if ((deviceType != SharedConstants.DEVICE_NOT_CONFIGURED) & (uniqueID == SharedConstants.DEVICE_NOT_CONFIGURED))
                    {
                        uniqueID = Guid.NewGuid().ToString().ToUpperInvariant(); // Assign a new unique ID
                        driverProfile.SetValue<string>(DEVICE_UNIQUE_ID_PROFILENAME, revisedDeviceName, uniqueID);
                    }

                    ConfiguredDevices[deviceName] = new ConfiguredDevice(deviceType, progID, description, deviceNumber, allowConnectedSetFalse, allowConnectedSetTrue, allowConcurrentAccess, uniqueID);
                }
            }
        }

        private static void SetNextRolloverTime()
        {
            // Set the next rollover time based on the retrieved RolloverTime value
            DateTime todaysRolloverTime = DateTime.Today.Add(RolloverTime.TimeOfDay); // Calculate today's rollover time

            // Check whether we are ahead or behind today's roll-over time
            if (todaysRolloverTime > DateTime.Now) // Today's roll-time is later than now so we can just wait for us to reach it
            {
                NextTraceLogRolloverTime = todaysRolloverTime;
                NextAccessLogRolloverTime = todaysRolloverTime;
            }
            else // Today's roll-time is earlier than now so we must set the next roll-time to tomorrow's roll-time
            {
                NextTraceLogRolloverTime = todaysRolloverTime.AddDays(1.0);
                NextAccessLogRolloverTime = todaysRolloverTime;
            }
        }

        /// <summary>
        /// Turn device names containing ServedDevice0X form into ServedDeviceX form
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        private static string FixDeviceName(string deviceName)
        {
            if (deviceName.Contains("ServedDevice0"))
            {
                return deviceName.Substring(0, 12) + deviceName.Substring(13);
            }
            else
            {
                return deviceName;
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
                driverProfile.SetValue<string>(SERVER_LOG_FOLDER_PROFILENAME, string.Empty, TraceFolder);
                driverProfile.SetValueInvariant<bool>(SERVER_TRACE_LEVEL_PROFILENAME, string.Empty, TraceState);
                driverProfile.SetValueInvariant<bool>(SERVER_DEBUG_TRACE_PROFILENAME, string.Empty, DebugTraceState);
                driverProfile.SetValue<string>(SERVER_IPADDRESS_PROFILENAME, string.Empty, ServerIPAddressString);
                driverProfile.SetValueInvariant<decimal>(SERVER_PORTNUMBER_PROFILENAME, string.Empty, ServerPortNumber);
                driverProfile.SetValueInvariant<bool>(SERVER_AUTOCONNECT_PROFILENAME, string.Empty, StartWithDevicesConnected);
                driverProfile.SetValueInvariant<bool>(SERVER_ACCESS_LOG_PROFILENAME, string.Empty, AccessLogEnabled);
                driverProfile.SetValueInvariant<bool>(SCREEN_LOG_REQUESTS_PROFILENAME, string.Empty, ScreenLogRequests);
                driverProfile.SetValueInvariant<bool>(SCREEN_LOG_RESPONSES_PROFILENAME, string.Empty, ScreenLogResponses);
                driverProfile.SetValueInvariant<bool>(MANAGEMENT_INTERFACE_ENABLED_PROFILENAME, string.Empty, ManagementInterfaceEnabled);
                driverProfile.SetValueInvariant<bool>(START_WITH_API_ENABLED_PROFILENAME, string.Empty, StartWithApiEnabled);
                driverProfile.SetValueInvariant<bool>(RUN_DRIVERS_ON_SEPARATE_THREADS_PROFILENAME, string.Empty, RunDriversOnSeparateThreads);
                driverProfile.SetValueInvariant<bool>(LOG_CLIENT_IPADDRESS_PROFILENAME, string.Empty, LogClientIPAddress);
                TL.IpAddressTraceState = LogClientIPAddress; // Persist the IP address trace state to the TraceLogger so that it can be used to format lines as required
                driverProfile.SetValueInvariant<bool>(INCLUDE_DRIVEREXCEPTION_IN_JSON_RESPONSE_PROFILENAME, string.Empty, IncludeDriverExceptionInJsonResponse);
                driverProfile.SetValue<string>(REMOTE_SERVER_LOCATION_PROFILENAME, string.Empty, RemoteServerLocation);
                driverProfile.SetValue<string>(CORS_PERMITTED_ORIGINS_PROFILENAME, string.Empty, CorsPermittedOrigins.ToConcatenatedString(SharedConstants.CORS_SERIALISATION_SEPARATOR));
                driverProfile.SetValueInvariant<bool>(CORS_SUPPORT_ENABLED_PROFILENAME, string.Empty, CorsSupportIsEnabled);
                driverProfile.SetValueInvariant<decimal>(CORS_MAX_AGE_PROFILENAME, string.Empty, CorsMaxAge);
                driverProfile.SetValueInvariant<bool>(CORS_CREDENTIALS_PERMITTED_PROFILENAME, string.Empty, CorsCredentialsPermitted);
                driverProfile.SetValueInvariant<bool>(ALPACA_DISCOVERY_ENABLED_PROFILENAME, string.Empty, AlpacaDiscoveryEnabled);
                driverProfile.SetValue<string>(ALPACA_UNIQUE_ID_PROFILENAME, string.Empty, AlpacaUniqueId);
                driverProfile.SetValueInvariant<decimal>(ALPACA_DISCOVERY_PORT_PROFILENAME, string.Empty, AlpacaDiscoveryPort);
                driverProfile.SetValueInvariant<int>(MAXIMUM_NUMBER_OF_DEVICES_PROFILENAME, string.Empty, MaximumNumberOfDevices);
                driverProfile.SetValueInvariant<bool>(IPV4_ENABLED_PROFILENAME, string.Empty, IpV4Enabled);
                driverProfile.SetValueInvariant<bool>(IPV6_ENABLED_PROFILENAME, string.Empty, IpV6Enabled);
                driverProfile.SetValueInvariant<bool>(ROLLOVER_LOGS_ENABLED_PROFILENAME, string.Empty, RolloverLogsEnabled);
                driverProfile.SetValueInvariant<DateTime>(ROLLOVER_TIME_PROFILENAME, string.Empty, RolloverTime);
                driverProfile.SetValueInvariant<bool>(USE_UTC_TIME_IN_LOGS_PROFILENAME, string.Empty, UseUtcTimeInLogs);
                driverProfile.SetValueInvariant<bool>(MINIMISE_TO_SYSTEM_TRAY_PROFILENAME, string.Empty, MinimiseToSystemTray);
                driverProfile.SetValueInvariant<bool>(CONFIRM_EXIT_PROFILENAME, string.Empty, ConfirmExit);
                driverProfile.SetValueInvariant<bool>(MINIMISE_ON_START_PROFILENAME, string.Empty, StartMinimised);

                // Update the next roll-over time in case the time has changed
                //TL.LogMessage("WriteProfile", $"NextRolloverTime Before: {NextRolloverTime}");
                SetNextRolloverTime();
                //TL.LogMessage("WriteProfile", $"NextRolloverTime After: {NextRolloverTime}");

                foreach (string deviceName in ServerDeviceNames)
                {
                    // Backward compatibility fix for early releases that only hosted 10 devices
                    string revisedDeviceName = FixDeviceName(deviceName);

                    driverProfile.SetValue<string>(DEVICETYPE_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].DeviceType.ToString());
                    driverProfile.SetValue<string>(PROGID_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].ProgID.ToString());
                    driverProfile.SetValue<string>(DESCRIPTION_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].Description.ToString());
                    driverProfile.SetValue<string>(DEVICENUMBER_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].DeviceNumber.ToString());
                    driverProfile.SetValue<bool>(ALLOW_CONNECTED_SET_FALSE_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].AllowConnectedSetFalse);
                    driverProfile.SetValue<bool>(ALLOW_CONNECTED_SET_TRUE_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].AllowConnectedSetTrue);
                    driverProfile.SetValue<bool>(ALLOW_CONCURRENT_ACCESS_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].AllowConcurrentAccess);

                    // Update unique ID if necessary
                    if (ConfiguredDevices[deviceName].DeviceType == SharedConstants.DEVICE_NOT_CONFIGURED)// Invalidate global IDs when devices are set to "none"
                    {
                        ConfiguredDevices[deviceName].UniqueID = SharedConstants.DEVICE_NOT_CONFIGURED;
                    }
                    else // Create a global ID if none currently exists
                    {
                        if (ConfiguredDevices[deviceName].UniqueID == SharedConstants.DEVICE_NOT_CONFIGURED) // Global ID does not exist so generate one
                        {
                            ConfiguredDevices[deviceName].UniqueID = Guid.NewGuid().ToString().ToUpperInvariant();
                        }
                    }
                    driverProfile.SetValue<string>(DEVICE_UNIQUE_ID_PROFILENAME, revisedDeviceName, ConfiguredDevices[deviceName].UniqueID.ToString());
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

            LogMessage(0, 0, 0, "SetupButton", string.Format("Stopping RemoteServer"));
            StopRESTServer(); // Shut down access while we use the Setup screen
            LogMessage(0, 0, 0, "SetupButton", string.Format("Disconnecting devices"));
            DisconnectDevices(); // Disconnect all devices so we can use their Setup screens if necessary

            LogMessage(0, 0, 0, "SetupButton", string.Format("Loading Setup form"));
            SetupForm frm = new SetupForm();
            DialogResult outcome = frm.ShowDialog();
            LogMessage(0, 0, 0, "SetupButton", string.Format("Setup dialogue outcome: {0}", outcome.ToString()));

            // Update use of UTC time in case this was changed in the setup dialogue
            if (outcome == DialogResult.OK)
            {
                TL.UseUtcTime = UseUtcTimeInLogs;
                AccessLog.UseUtcTime = UseUtcTimeInLogs;
            }

            // Log new configuration
            WriteConfigurationToLog();

            frm.Dispose(); // Dispose of the setup form

            // Start with new configuration
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
                LogMessage(0, 0, 0, "SetupButton", string.Format("Starting Remote server"));
                StartRESTServer();
            }
            else
            {
                LogMessage(0, 0, 0, "SetupButton", string.Format("Not starting Remote server because it wasn't running in the first place."));
            }
        }

        /// <summary>
        /// Handle server form exit button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close(); // CloseServer();
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
        /// Handle the form closing event, cancelling the close if the user says "no" whenasked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check whether the server is configured to ask for shutdown confirmation
            if (ConfirmExit) // Confirmation is required
            {
                // Ask the user whether they want to close the remote server
                DialogResult result = MessageBox.Show("Are you sure you want to close the Remote Server?", "ASCOM Remote Server", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                // An OK result means the user wants to shut down the Remote Server so allow the form to close, otherwise cancel the close.
                if (result != DialogResult.OK)
                    e.Cancel = true;
            }
            else // No confirmation is required so go ahead and let the application shut down.
            {
                // No action required
            }

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

            if (WindowState != FormWindowState.Minimized) // Window is visible so update control positions
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
                int titleLeft;
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

                //Control Group 3 - Remote Server Status, Stop and Start
                LblRESTStatus.Location = new Point(controlLeftPosition, chkLogResponses.Top + controlSpacing - 5);
                BtnStopRESTServer.Location = new Point(controlLeftPosition - 1, LblRESTStatus.Top + 30);
                BtnStartRESTServer.Location = new Point(controlLeftPosition + 90, LblRESTStatus.Top + 30);

                // Control Group 4 - Device Status, Disconnect and Connect
                LblDriverStatus.Location = new Point(controlLeftPosition, BtnStopRESTServer.Top + controlSpacing + 3);
                BtnDisconnectDevices.Location = new Point(controlLeftPosition - 1, LblDriverStatus.Top + 30);
                BtnConnectDevices.Location = new Point(controlLeftPosition + 90, LblDriverStatus.Top + 30);

                // Control Group 5 - Setup button
                BtnSetup.Location = new Point(controlCentrePosition, BtnDisconnectDevices.Top + controlSpacing + 2);

                // Control Group 6 - Exit button
                BtnExit.Location = new Point(controlCentrePosition, BtnSetup.Top + controlSpacing + 2);

                // Save the current form state so it can be restored after the application is next minimised
                formWindowState = this.WindowState;

                // Show the application in the task bar
                this.ShowInTaskbar = true;

                // Show the form in ALT/TAB
                this.FormBorderStyle = FormBorderStyle.Sizable;
            }
            else // WIndow is minimised so minimise to system tray if configured to do so
            {
                // Test whether the application should minimise to the task bar or to the system tray
                if (MinimiseToSystemTray) // Minimise to system tray
                {
                    // Hide the application
                    this.Hide();

                    // Hide the application from the task bar
                    this.ShowInTaskbar = false;

                    // Hide the form from ALT/TAB
                    this.FormBorderStyle = FormBorderStyle.SizableToolWindow;

                    // Make the system tray icon visible
                    notifyIcon.Visible = true;
                }
                else // Minimise to task bar
                {
                    // Show the application in the task bar
                    this.ShowInTaskbar = true;

                    // Show the form in ALT/TAB
                    this.FormBorderStyle = FormBorderStyle.Sizable;
                }
            }
        }

        /// <summary>
        /// Event handler for a double click on the system tray icon that appears when the application is minimised to the system tray
        /// </summary>
        /// <param name="sender">Notifying object</param>
        /// <param name="e">Event arguments</param>
        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreForm();
        }

        /// <summary>
        /// Handle a click to the system tray context menu title
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemTrayTitle_Click(object sender, EventArgs e)
        {
            RestoreForm();
        }

        /// <summary>
        /// Handle click on the system tray exit menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemTrayCloseServer_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// System tray icon single so update context menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            systemTrayMenuItems.Items["NumberOfDevices"].Text = $"Number of devices: {ConfiguredDevices.Count}";
            systemTrayMenuItems.Items["Ipv4"].Text = $"IP v4 enabled: {IpV4Enabled}";
            systemTrayMenuItems.Items["IPv6"].Text = $"IP v6 enabled: {IpV6Enabled}";
            systemTrayMenuItems.Items["IPAddress"].Text = $"IP Address: {(ServerIPAddressString == "+" ? "All IP addresses" : ServerIPAddressString)}";
            systemTrayMenuItems.Items["Port"].Text = $"Listening on port: {ServerPortNumber}";
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
            RequestData requestData = new RequestData(); // Blank value to make logging work

            // Get the result context from the client's call
            try
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - Request received. Is thread pool thread: {1}. Is background thread: {2}.", Thread.CurrentThread.ManagedThreadId.ToString(), Thread.CurrentThread.IsThreadPoolThread, Thread.CurrentThread.IsBackground));
                context = listener.EndGetContext(result); // Get the context object
            }
            catch (NullReferenceException) // httpListener is null because we are closing down or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - EndGetContext - httpListener is null so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }
            catch (ObjectDisposedException) // httpListener is disposed because the Remote server has been stopped or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - EndGetContext - httpListener is disposed so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }

            catch (Exception ex)
            {
                LogException1(requestData, "APIRequestCallback", ex.ToString()); // Log the exception
                if (context != null) // We have a context object but also an exception so return the error message to the client with a 500 status code
                {
                    requestData.Request = context.Request;
                    requestData.Response = context.Response;
                    Return500Error(requestData, ex.Message);
                    return;
                }
                else // No context object so not possible to return an error to the client, just log the error and increment the error counter
                {
                    LogException1(requestData, "APIRequestCallback", string.Format("Thread {0} - EndGetContext exception: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString()));
                    Interlocked.Increment(ref numberOfConsecutiveErrors);
                }
            }

            // Set up the next call back            
            try
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - Setting up new call back to wait for next request ", Thread.CurrentThread.ManagedThreadId.ToString()));
                httpListener.BeginGetContext(new AsyncCallback(RestRequestReceivedHandler), httpListener);
            }
            catch (NullReferenceException) // httpListener is null because we are closing down or because of some other error so just log the event
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - BeginGetContext - httpListener is null so taking no action and just returning.", Thread.CurrentThread.ManagedThreadId.ToString()));
                return;
            }
            catch (Exception ex)
            {
                LogException1(requestData, "APIRequestCallback", ex.ToString()); // Log the exception
                if (context != null) // We have a context object but also an exception so return the error message to the client with a 500 status code
                {
                    requestData.Request = context.Request;
                    requestData.Response = context.Response;
                    Return500Error(requestData, ex.Message);
                    return;
                }
                else // No context object so not possible to return an error to the client
                {
                    LogException1(requestData, "APIRequestCallback", string.Format("Thread {0} - BeginGetContext exception: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), ex.ToString()));
                    Interlocked.Increment(ref numberOfConsecutiveErrors);
                    return;
                }
            }

            Interlocked.Exchange(ref numberOfConsecutiveErrors, 0); // Reset the consecutive errors counter to zero

            // Test whether this request has come in on a supported IP version, if not then ignore it
            IPEndPoint localIpEndPoint = (IPEndPoint)context.Request.LocalEndPoint;

            if ((localIpEndPoint.AddressFamily == AddressFamily.InterNetworkV6) & (!IpV6Enabled))
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"IPv6 is not enabled on the remote server, this request will be dropped.");
                return;
            }

            if ((localIpEndPoint.AddressFamily == AddressFamily.InterNetwork) & (!IpV4Enabled))
            {
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"IPv4 is not enabled on the remote server, this request will be dropped.");
                return;
            }

            // Test whether the user has configured the server to respond on all IP addresses, if so process it without further checks
            if ((ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG) | (ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_WEAK))
            {
                // User has configured all IP addresses so no filtering is required
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"The configured server IP address is + or * so we will process this request.");
            }
            else
            {
                // User has configured a specific IP address so test whether this request is to the configured IP address
                if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"Request received on endpoint: {localIpEndPoint} - Enabled endpoint: {new IPEndPoint(IPAddress.Parse(ServerIPAddressString), (int)ServerPortNumber)}");
                if (localIpEndPoint.ToString() != new IPEndPoint(IPAddress.Parse(ServerIPAddressString), (int)ServerPortNumber).ToString()) // IP Addresses don't match so we just ignore the request
                {
                    if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"  The endpoint is NOT enabled - This request will be dropped.");

                    return;
                }
                else // IP addresses do match so we can process this request
                {
                    if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", $"  The endpoint is ENABLED - This request will be processed");
                }
            }

            // Now process this request, any exceptions are handled by the ProcessRequest method itself
            if (DebugTraceState) LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - Processing received message.", Thread.CurrentThread.ManagedThreadId.ToString()));
            ProcessRestRequest(context);

            // Shut down the listener and close down device drivers if the maximum number of errors has been reached
            if (numberOfConsecutiveErrors == MAX_ERRORS_BEFORE_CLOSE)
            {
                LogMessage1(requestData, "APIRequestCallback", string.Format("Thread {0} - Maximum number of errors ({1}) reached, closing server and device drivers.", Thread.CurrentThread.ManagedThreadId, MAX_ERRORS_BEFORE_CLOSE));

                // Clear down the listener
                StopRESTServer();

                // Clear the devices
                DisconnectDevices();
            }
        }  // Event handler for new REST requests

        /// <summary>
        /// Processes the request received by the server
        /// </summary>
        /// <param name="context">Context object that contains the request and response objects.</param>
        private void ProcessRestRequest(HttpListenerContext context)
        {
            // Local convenience variables to hold this transaction's information
            uint clientID = 0;
            uint clientTransactionID = 0;
            uint serverTransactionID = 0;
            NameValueCollection suppliedParameters;
            HttpListenerRequest request;
            HttpListenerResponse response;
            RequestData requestData = new RequestData();

            try
            {
                IncrementConcurrencyCounter();

                suppliedParameters = new NameValueCollection(); // Create a collection to hold the supplied parameters
                request = context.Request;
                response = context.Response;
                serverTransactionID = GetServerTransactionID();
                requestData.Request = request;
                requestData.Response = response;
                requestData.ServerTransactionID = serverTransactionID;

                response.Headers.Add(HttpResponseHeader.Server, "ASCOM Rest API Server -");

                // Log the request 
                LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"{request.HttpMethod} URL: {request.Url.PathAndQuery}, Protocol: HTTP/{request.ProtocolVersion}, Thread: {Thread.CurrentThread.ManagedThreadId}");

                // Create a collection of supplied parameters: query variables in the URL string for HTTP GET requests and form parameters from the request body for HTTP PUT requests.

                switch (requestData.Request.HttpMethod.ToUpperInvariant())
                {
                    // Process query parameters from an HTTP GET    
                    case "GET":
                        suppliedParameters.Add(request.QueryString); // Add the query string parameters to the collection

                        // List query string parameters
                        foreach (string key in suppliedParameters)
                        {
                            LogMessage1(requestData, "Query Parameter", $"{key} = {suppliedParameters[key]}");
                        }
                        break;

                    // Process form parameters from an HTTP PUT
                    case "PUT":
                        if (request.HasEntityBody) // Add any parameters supplied in the form body
                        {
                            string formParameters;
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) // Extract the aggregated parameter string from the form within the request
                            {
                                formParameters = reader.ReadToEnd();
                            }
                            if (formParameters == null) formParameters = ""; // Handle the possibility that we get a null value instead of an empty string

                            string[] rawParameters = formParameters.Split('&'); // Parse the aggregated parameter string into an array of key / value pair strings
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Form parameters string: '{formParameters}'Form parameters string length: {formParameters.Length}, Raw parameters array size: {rawParameters.Length}");

                            foreach (string parameter in rawParameters) // Parse each key / value pair string into its key and value and add these to the parameters collection
                            {
                                string[] keyValuePair = parameter.Split('=');
                                if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Found form parameter string: '{parameter}' whose KeyValuePair array size is: {keyValuePair.Length}");

                                string key = keyValuePair[0].Trim(); // Extract the key value
                                if (!string.IsNullOrEmpty(key)) // The key has a name so now extract the value
                                {
                                    string value = ""; // Initialise a variable to hold the value
                                    if (keyValuePair.Length > 1) // The key does have a value
                                    {
                                        value = value = HttpUtility.UrlDecode(keyValuePair[1].Trim()); // Extract the value so long as one exists 
                                    }
                                    else // The key does not have a value
                                    {
                                        LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"No parameter value was found for parameter {parameter} - an empty string will be assumed.");
                                    }

                                    // Add the parameter key and value to the parameter list
                                    suppliedParameters.Add(key, value);

                                    // Log the form parameter if debug tracing
                                    LogMessage1(requestData, "Form Parameter", $"{key} = {value}");
                                }
                                else // The key is null or empty so ignore it
                                {
                                    if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Ignoring parameter with no name");
                                }
                            }
                        }
                        break;

                    default:

                        break;
                }

                // Add the query or body parameters to the request data
                requestData.SuppliedParameters = suppliedParameters;

                // Extract the caller's IP address into hostIPAddress
                string clientIpAddress;
                if (request.Headers[X_FORWARDED_FOR] != null)
                {
                    clientIpAddress = request.Headers[X_FORWARDED_FOR]; // The request was fronted by an Apache web server
                }
                else
                {
                    clientIpAddress = request.RemoteEndPoint.ToString(); // The request came straight to this application
                }

                // Create an access log entry

                if (AccessLog.Enabled) // We are logging so we have to close the current log and start a new one if we have moved to a new day
                {
                    DateTime now = DateTime.Now;

                    if (now > NextAccessLogRolloverTime) // We have moved into the next log period so close the current log and start another
                    {
                        // Close the current logger
                        AccessLog.LogMessage(clientID, clientTransactionID, serverTransactionID, "EndOfDay", "Closing this log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));
                        AccessLog.Enabled = false;
                        AccessLog.Dispose();
                        AccessLog = null;

                        // Start a new logger
                        AccessLog = new TraceLoggerPlus("", ACCESSLOG_TRACELOGGER_NAME)
                        {
                            LogFilePath = TraceFolder,
                            Enabled = true, // Enable the trace logger
                            IpAddressTraceState = LogClientIPAddress // Set the current state of the "include client IP address in trace lines" flag
                        };

                        AccessLog.LogMessage(clientID, clientTransactionID, serverTransactionID, "StartOfDay", "Opening a new log because a new day has started. " + now.ToString("dddd d MMMM yyyy HH:mm:ss"));

                        NextAccessLogRolloverTime = NextTraceLogRolloverTime.AddDays(1.0); // Update the next roll-over time so that it can be tested on the next logging call
                    }

                    //Write the access log entry
                    AccessLog.LogMessage("    " + clientIpAddress, string.Format("{0} {1}", request.HttpMethod, request.Url.AbsolutePath));
                }

                if (ScreenLogRequests) // Incoming requests are logged to the screen
                {
                    if (LogClientIPAddress) LogToScreen($"{clientIpAddress} {request.HttpMethod} {request.Url.AbsolutePath}"); // Include the client IP address if required
                    else LogToScreen($"{request.HttpMethod} {request.Url.AbsolutePath}"); // Otherwise log the request without client IP address
                }
                requestData.ClientIpAddress = clientIpAddress;

                // Extract the client ID number from the supplied URI / Form, if present
                string clientIDString = suppliedParameters[SharedConstants.CLIENTID_PARAMETER_NAME];
                if (clientIDString != null) // Some value was supplied for this parameter
                {
                    // Parse the integer value out or throw a 400 error if the value is not an integer
                    if (!uint.TryParse(clientIDString, out clientID))
                    {
                        LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}, Concurrent requests: {3}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString(), numberOfConcurrentTransactions));
                        Return400Error(requestData, "Client ID is not an integer: " + suppliedParameters[SharedConstants.CLIENTID_PARAMETER_NAME]);
                        return;
                    }
                }
                requestData.ClientID = clientID;

                // Extract the client transaction ID from the supplied URI / Form, if present
                string clientTransactionIDString = suppliedParameters[SharedConstants.CLIENTTRANSACTION_PARAMETER_NAME];
                if (clientTransactionIDString != null) // Some value was supplied for this parameter
                {
                    // Parse the integer value out or throw a 400 error if the value is not an integer
                    if (!uint.TryParse(clientTransactionIDString, out clientTransactionID))
                    {
                        LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));
                        Return400Error(requestData, "Client transaction ID is not an integer: " + suppliedParameters[SharedConstants.CLIENTTRANSACTION_PARAMETER_NAME]);
                        return;
                    }
                }
                requestData.ClientTransactionID = clientTransactionID;

                if (DebugTraceState) // List headers and detailed parameter list if in debug mode
                {
                    foreach (string key in request.Headers.AllKeys)
                    {
                        LogMessage1(requestData, "RequestReceived", string.Format("Header {0} = {1}", key, request.Headers[key]));
                    }
                    foreach (string key in suppliedParameters.AllKeys)
                    {
                        LogMessage1(requestData, "RequestReceived", string.Format("Parameter {0} = {1}", key, suppliedParameters[key]));
                    }
                } // Log supplied parameters and headers

                // CORS Support
                if (CorsSupportIsEnabled)
                {
                    if (TL.DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "CORS support is enabled");
                    if (!string.IsNullOrEmpty(request.Headers.Get(CORS_ORIGIN_HEADER)))
                    {
                        // CHECK CORS PERMISSIONS
                        if (CorsPermittedOrigins.Contains(CORS_PERMISSION_ALL_ORIGINS)) // If the CORS origin wild-card value is configured return the wild card "permit all origins" permission
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Permitted origins contains {CORS_PERMISSION_ALL_ORIGINS}, setting {CORS_ALLOW_ORIGIN_HEADER} = {CORS_PERMISSION_ALL_ORIGINS}");
                            response.Headers.Add(CORS_ALLOW_ORIGIN_HEADER, CORS_PERMISSION_ALL_ORIGINS);
                        }
                        else if (CorsPermittedOrigins.Contains(CORS_PERMISSION_REFLECT_SUPPLIED_ORIGIN)) // Return the origin presented by the client
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Permitted origins contains {CORS_PERMISSION_REFLECT_SUPPLIED_ORIGIN}, setting {CORS_ALLOW_ORIGIN_HEADER} = {request.Headers.Get(CORS_ORIGIN_HEADER)}");
                            response.Headers.Add(CORS_ALLOW_ORIGIN_HEADER, request.Headers.Get(CORS_ORIGIN_HEADER));
                        }
                        else if (
                            CorsPermittedOrigins.Contains(request.Headers.Get(CORS_ORIGIN_HEADER)) &&
                            ((request.Headers.Get(CORS_ORIGIN_HEADER) != CORS_PERMISSION_ALL_ORIGINS)) &&
                            ((request.Headers.Get(CORS_ORIGIN_HEADER) != CORS_PERMISSION_REFLECT_SUPPLIED_ORIGIN))
                            ) // Check whether the supplied origin is in the list of permitted origins, ignoring "*" and "=" for obvious reasons
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Permitted origins contains {request.Headers.Get(CORS_ORIGIN_HEADER)}, setting {CORS_ALLOW_ORIGIN_HEADER} = {request.Headers.Get(CORS_ORIGIN_HEADER)}");
                            response.Headers.Add(CORS_ALLOW_ORIGIN_HEADER, request.Headers.Get(CORS_ORIGIN_HEADER));
                        }
                        else // Reject the request if the origin is not permitted by the configured origins rules
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"Supplied origin \"{request.Headers.Get(CORS_ORIGIN_HEADER)}\" is not in the permitted list - setting {CORS_ALLOW_ORIGIN_HEADER} = {CorsPermittedOrigins[0]}");
                            ReturnEmpty200Success(requestData);
                            return; // Finish processing this request by returning and letting the thread end
                        }

                        // HANDLE CORS CREDENTIALS
                        if (CorsCredentialsPermitted)
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "CORS credential support is enabled");
                            response.Headers.Add(CORS_ALLOW_CREDENTIALS_HEADER, CORS_ALLOW_CREDENTIALS);
                            response.Headers.Set(CORS_ALLOW_ORIGIN_HEADER, request.Headers.Get(CORS_ORIGIN_HEADER));
                        }

                        // HANDLE CACHING RESPONSE
                        if (response.Headers.Get(CORS_ALLOW_ORIGIN_HEADER) == CORS_PERMISSION_ALL_ORIGINS) // Add a Vary header to assist HTTP cache control if there could be different responses for different origins
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "All host wild-card is not  in effect, including a Vary header set to Origin");
                            response.Headers.Add(CORS_VARY_HEADER, CORS_ORIGIN_HEADER);
                        }

                        if (request.HttpMethod.ToUpperInvariant() == "OPTIONS") // This is a CORS pre-flight request so we need to set some specific headers
                        {
                            // Set the Access-Control-Allow-Methods and Access-Control-Max-Age headers
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, $"OPTIONS method found - This is a CORS PRE_FLIGHT request: {CORS_ALLOWED_METHODS_HEADER} = {CORS_ALLOWED_METHODS}, {CORS_MAX_AGE_HEADER} = {CorsMaxAge}");
                            response.Headers.Add(CORS_ALLOWED_METHODS_HEADER, CORS_ALLOWED_METHODS);
                            response.Headers.Add(CORS_MAX_AGE_HEADER, CorsMaxAge.ToString());
                            ReturnEmpty200Success(requestData);
                            return; // Finish processing here so return and let the thread end
                        }
                        else // Log this as a CORS simple request and allow the request to move forward to be processed
                        {
                            if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "This is a CORS SIMPLE request");
                        }
                    }
                    else // No or empty Origin header so log and process the request
                    {
                        if (DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "Origin header is absent or empty, the API request will be processed.");
                    }
                } // Pre-process requests from CORS aware clients if CORS support is enabled
                else // CORS support is disabled
                {
                    if (TL.DebugTraceState) LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, "CORS support is disabled");

                }

                if (request.Url.AbsolutePath.Trim().StartsWith(SharedConstants.API_URL_BASE)) // Process requests whose URIs start with /api
                {
                    // Return a 403 error if the API is not enabled through the management console
                    if (!apiIsEnabled)
                    {
                        LogMessage1(requestData, SharedConstants.REQUEST_RECEIVED_STRING, string.Format("{0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));
                        Return403Error(requestData, API_INTERFACE_NOT_ENABLED_MESSAGE);
                        return;
                    }

                    // Split the supplied URI into its elements demarked by the / character and remove any leading / trailing space characters from each element
                    // Element [0] will be "api"
                    // Element [1] will be the API version (whole number prefixed by V e.g. V1)
                    // Element [2] will be device type
                    // Element [3] will be the device number within the device type collection
                    // Element [4] will be a Method
                    string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);
                    requestData.Elements = elements;

                    // Basic error checking - We must have received 5 elements, now in the elements array, in order to have received a valid API request so check this here:
                    if (elements.Length != 5)
                    {
                        Return400Error(requestData, "Incorrect API format - Received: " + request.Url.AbsolutePath + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                        return;
                    } // We don't have 5 elements so return a 400 error
                    else // We have received the required 5 elements in the URI
                    {
                        for (int i = 0; i < elements.Length; i++) // Remove leading and trailing space characters from each element
                        {
                            elements[i] = elements[i].Trim();
                        } // Clean the supplied parameters

                        switch (elements[URL_ELEMENT_API_VERSION]) // Process each API version as necessary (at 8/8/17 only V1 is implemented)
                        {
                            case SharedConstants.API_VERSION_V1: // OK so we have a V1 request
                                if ((ServerDeviceNumbers.Contains(elements[URL_ELEMENT_DEVICE_NUMBER])) & (elements[URL_ELEMENT_DEVICE_NUMBER] != "")) // OK so we have a valid device number
                                {
                                    string deviceKey = string.Format("{0}/{1}", elements[URL_ELEMENT_DEVICE_TYPE], elements[URL_ELEMENT_DEVICE_NUMBER]); // Create the device key from the supplied device type and device number parameters

                                    requestData.DeviceKey = deviceKey; // Save the device key for use throughout the remainder of the application

                                    // Check whether the device is configured on the Remote Server
                                    if (ActiveObjects.ContainsKey(deviceKey)) // Device is configured on the server, now check whether it initialised OK
                                    {
                                        if (ActiveObjects[deviceKey].InitialisedOk) // The device did initialise OK
                                        {
                                            // Test whether this device is configured for concurrent request processing
                                            // If not permitted, wait until we get a lock on the device's synchronisation lock object, otherwise continue without waiting
                                            if (!ActiveObjects[deviceKey].AllowConcurrentAccess) // Concurrent processing is not permitted so wait for the synchronisation lock
                                            {
                                                Monitor.Enter(ActiveObjects[deviceKey].CommandLock);
                                                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Device only supports serialised access - command lock acquired for device {deviceKey} on REST thread {Thread.CurrentThread.ManagedThreadId}");
                                            }  // Concurrent processing is not enabled 
                                            else
                                            {
                                                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Device supports concurrent access - no command lock required for {deviceKey} from REST thread {Thread.CurrentThread.ManagedThreadId}");
                                            } // Concurrent processing is enabled

                                            // Process the command within a try block so that "finally" can be used to release the Monitor synchronisation object if it was set because the device can only handle one command at a time
                                            try
                                            {
                                                // Process the request on this thread or pass to the Windows Form hosting the driver when drivers are configured to run on separate threads
                                                // The driver is hosted on its own form to separate it from other drivers with the intention of improving the remote Server overall resilience in case a driver dies
                                                // The driver form will create a thread to run the command and return it to this thread which will wait for the command to complete using Thread.Join
                                                // This mechanic leaves the form message loop running so that concurrent commands to the driver can be processed even if an earlier slow running command has not completed.
                                                if (RunDriversOnSeparateThreads) // Each driver is running in its own Form with its own message loop
                                                {
                                                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"ProcessRequestAsync - Sending command to {deviceKey} on REST thread {Thread.CurrentThread.ManagedThreadId}");
                                                    DriverCommandDelegate driverCommandDelegate = new DriverCommandDelegate(ActiveObjects[deviceKey].DriverHostForm.DriverCommand); // Create a delegate for this request

                                                    Thread driverThread = (Thread)ActiveObjects[deviceKey].DriverHostForm.Invoke(driverCommandDelegate, requestData); // Send the command to the host form, which will return a thread where the command is running

                                                    if (driverThread != null) // A thread was returned so wait for it to complete
                                                    {
                                                        int threadId = driverThread.ManagedThreadId; // Get the thread ID for reporting purposes
                                                        driverThread.Join(); // Wait for the worker thread to complete. By using Thread.Join other COM requests can be processed
                                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"ProcessRequestAsync - Command completed for {deviceKey} using WORKER thread {threadId} on REST thread {Thread.CurrentThread.ManagedThreadId}");
                                                    } // A thread was returned by the driver host form
                                                    else // No thread was returned because of a catastrophic failure in the host form
                                                    {
                                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"ProcessRequestAsync - No thread returned from DriverCommand for {deviceKey} on REST thread {Thread.CurrentThread.ManagedThreadId}");
                                                    } // No thread was returned by the driver host for so log the error
                                                } // Drivers are running on separate threads with independent forms and message queues
                                                else // Driver is running on the UI thread so just process the command
                                                {
                                                    ProcessDriverCommand(requestData); // Process the device command
                                                } // Drivers are all running on the main UI thread
                                            }
                                            finally // Ensure that the command lock is released if set
                                            {
                                                if (!ActiveObjects[deviceKey].AllowConcurrentAccess)
                                                {
                                                    Monitor.Exit(ActiveObjects[deviceKey].CommandLock); // Release the command lock object if a lock was used
                                                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Device only supports serialised access - command lock released for device {deviceKey} from REST thread {Thread.CurrentThread.ManagedThreadId}");
                                                } // Concurrent access is not enabled so release the synchronisation lock obtained earlier
                                                else // Specified is configured but threw an error when created or when Connected was set True so return the error message
                                                {
                                                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Device supports concurrent access - no command lock to release for {deviceKey} from REST thread {Thread.CurrentThread.ManagedThreadId}");
                                                } // Concurrent operation is permitted so just log progress
                                            }
                                        } // The device did initialise OK
                                        else // Specified is configured but threw an error when created or when Connected was set True so return the error message
                                        {
                                            Return400Error(requestData, string.Format("Device {0} did not start correctly and is unavailable: {1}", deviceKey, ActiveObjects[deviceKey].InitialisationErrorMessage));
                                        } // Handle cases where the device did not initialise correctly
                                    } // OK, we have a valid device
                                    else // Specified device is not configured so return an error message
                                    {
                                        Return400Error(requestData, string.Format("Device {0} is not configured on the Remote Server", deviceKey));
                                    } // Handle an invalid device
                                } // We have a valid device number
                                else
                                {
                                    Return400Error(requestData, "Unsupported or invalid integer device number: " + elements[URL_ELEMENT_DEVICE_NUMBER] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                } // Invalid device number provided so return a 400 error
                                break;

                            default: // Handle invalid API version numbers
                                Return400Error(requestData, "Unsupported API version: " + elements[URL_ELEMENT_API_VERSION] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                break;
                        } // Process using the correct API version

                    } // We have 5 elements so process the command

                    LogBlankLine(clientID, clientTransactionID, serverTransactionID);
                } // Process standard Alpaca device API requests

                else if (request.Url.AbsolutePath.Trim().StartsWith(SharedConstants.ALPACA_DEVICE_MANAGEMENT_URL_BASE)) // Process standard Alpaca management calls
                {
                    // Split the supplied URI into its elements demarked by the / character and remove any leading / trailing space characters from each element
                    // Element [0] will be "server"
                    // Element [1] will be the API version (whole number prefixed by V e.g. V1)
                    // Element [2] will be the configuration command
                    string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);

                    // Handle the api versions command here because it does not fall into the normal management command format
                    if (elements[URL_ELEMENT_API_VERSION].Trim() == SharedConstants.ALPACA_DEVICE_MANAGEMENT_APIVERSIONS) // Handle the api versions command to return a list of supported api versions
                    {
                        // Return the array of supported version numbers
                        IntArray1DResponse intArrayResponseClass = new IntArray1DResponse(clientTransactionID, serverTransactionID, SharedConstants.MANAGEMENT_SUPPORTED_INTERFACE_VERSIONS)
                        {
                            DriverException = null,
                            SerializeDriverException = false
                        };
                        string intArrayResponseJson = JsonConvert.SerializeObject(intArrayResponseClass);

                        if (elements.Length < 5) // Format the number of elements to 5 so that message logging will work correctly
                        {
                            // We have an array of less than 5 elements, now resize it to 5 elements so that we can add the command into the equivalent position that it occupies for a "device API" call.
                            // This is necessary so that the logging commands will work for both device API and MANAGEMENT commands
                            Array.Resize<string>(ref elements, 5);
                            elements[URL_ELEMENT_DEVICE_NUMBER] = "0";
                            elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_API_VERSION]; // Copy the command name to the device method field
                            requestData.Elements = elements;
                        }

                        SendResponseValueToClient(requestData, null, intArrayResponseJson);
                    }
                    else // The command should follow the expected three parameter format of an Alpaca management command
                    {
                        // Basic error checking - We must have received 3 elements, now in the elements array, in order to have received a valid management API request so check this here:
                        if (elements.Length != 3)
                        {
                            Return400Error(requestData, "Incorrect API format - Received: " + request.Url.AbsolutePath + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                            return;
                        } // Invalid management command so return an error
                        else // We have received the required 3 elements in the URI
                        {
                            for (int i = 0; i < elements.Length; i++)
                            {
                                elements[i] = elements[i].Trim();// Remove leading and trailing space characters
                                LogMessage1(requestData, "ManagmentCommand", string.Format("Received element {0} = {1}", i, elements[i]));
                            }

                            // We have an array of size 3, now resize it to 5 elements so that we can add the command into the equivalent position that it occupies for a "device API" call.
                            // This is necessary so that the logging commands will work for both device API and MANAGEMENT commands
                            Array.Resize<string>(ref elements, 5);
                            elements[URL_ELEMENT_DEVICE_NUMBER] = "0";
                            elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_SERVER_COMMAND]; // Copy the command name to the device method field
                            requestData.Elements = elements;

                            // Only permit processing if access has been granted through the setup dialogue
                            if (ManagementInterfaceEnabled | AlpacaDiscoveryEnabled)
                            {
                                switch (elements[URL_ELEMENT_API_VERSION])
                                {
                                    case SharedConstants.API_VERSION_V1: // OK so we have a V1 request
                                        try // Confirm that the command requested is available on this server
                                        {
                                            string commandName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(elements[URL_ELEMENT_SERVER_COMMAND].ToLowerInvariant()); // Capitalise the first letter of the command

                                            switch (request.HttpMethod.ToUpperInvariant())
                                            {
                                                case "GET": // Read methods
                                                    switch (elements[URL_ELEMENT_SERVER_COMMAND])
                                                    {
                                                        // Alpaca device management interface standard responses

                                                        case SharedConstants.ALPACA_DEVICE_MANAGEMENT_DESCRIPTION:
                                                            // Create the remote server description and return it to the client in the proscribed format
                                                            AlpacaDeviceDescription remoteServerDescription = new AlpacaDeviceDescription(SharedConstants.ALPACA_DEVICE_MANAGEMENT_SERVERNAME,
                                                                                                                                          SharedConstants.ALPACA_DEVICE_MANAGEMENT_MANUFACTURER,
                                                                                                                                          Assembly.GetEntryAssembly().GetName().Version.ToString(),
                                                                                                                                          RemoteServerLocation);

                                                            AlpacaDescriptionResponse descriptionResponse = new AlpacaDescriptionResponse(clientTransactionID, serverTransactionID, remoteServerDescription)
                                                            {
                                                                DriverException = null,
                                                                SerializeDriverException = false
                                                            };

                                                            string descriptionResponseJson = JsonConvert.SerializeObject(descriptionResponse);
                                                            SendResponseValueToClient(requestData, null, descriptionResponseJson);
                                                            break;

                                                        case SharedConstants.ALPACA_DEVICE_MANAGEMENT_CONFIGURED_DEVICES:
                                                            List<AlpacaConfiguredDevice> alpacaConfiguredDevices = new List<AlpacaConfiguredDevice>(); // Create an empty list to hold the list of configured devices 

                                                            // Populate the list with configured devices
                                                            foreach (KeyValuePair<string, ConfiguredDevice> configuredDevice in ConfiguredDevices)
                                                            {
                                                                if (configuredDevice.Value.DeviceType != SharedConstants.DEVICE_NOT_CONFIGURED) // Only include configured devices, ignoring unconfigured device slots
                                                                {
                                                                    AlpacaConfiguredDevice alpacaConfiguredDevice = new AlpacaConfiguredDevice(configuredDevice.Value.Description,
                                                                                                                                               configuredDevice.Value.DeviceType,
                                                                                                                                               configuredDevice.Value.DeviceNumber,
                                                                                                                                               configuredDevice.Value.UniqueID);
                                                                    alpacaConfiguredDevices.Add(alpacaConfiguredDevice);
                                                                }
                                                            }

                                                            AlpacaConfiguredDevicesResponse alpacaConfigurationResponse = new AlpacaConfiguredDevicesResponse(clientTransactionID, serverTransactionID, alpacaConfiguredDevices)
                                                            {
                                                                DriverException = null,
                                                                SerializeDriverException = IncludeDriverExceptionInJsonResponse
                                                            };
                                                            ;
                                                            string alpacaConfigurationResponseJson = JsonConvert.SerializeObject(alpacaConfigurationResponse);
                                                            SendResponseValueToClient(requestData, null, alpacaConfigurationResponseJson);
                                                            break;

                                                        default:
                                                            Return400Error(requestData, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                                            break;
                                                    }
                                                    break;
                                                case "PUT": // Write or action methods
                                                    switch (elements[URL_ELEMENT_SERVER_COMMAND])
                                                    {
                                                        // No commands yet so return a default command not supported to everything
                                                        default:
                                                            Return400Error(requestData, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    Return400Error(requestData, "Unsupported http verb: " + request.HttpMethod);
                                                    break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Return400Error(requestData, string.Format("Exception processing {0} command \r\n {1}", elements[URL_ELEMENT_SERVER_COMMAND], ex.ToString()));
                                        }

                                        break; // End of valid api version numbers
                                    default:
                                        Return400Error(requestData, "Unsupported API version: " + elements[URL_ELEMENT_API_VERSION] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                        break;
                                }
                            }
                            else // The management interface is not enabled so return an error
                            {
                                LogMessage1(requestData, "Management", MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE);
                                Return403Error(requestData, MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE);
                            }
                        } // Valid management command
                    } // Handle all other management requests
                } // Process standard Alpaca device management API requests

                else if (request.Url.AbsolutePath.Trim().StartsWith(SharedConstants.ALPACA_DEVICE_SETUP_URL_BASE.TrimEnd(FORWARD_SLASH))) // Process standard Alpaca HTML management calls to "/setup" and "/setup/"
                {
                    LogMessage(0, 0, 0, "Received URL", request.Url.AbsolutePath);
                    string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);
                    foreach (string element in elements)
                    {
                        LogMessage(0, 0, 0, "Setup Element", element);
                    }
                    switch (elements.Length)
                    {
                        case 1: // Just setup
                            {
                                Array.Resize<string>(ref elements, 5);
                                elements[1] = SETUP_DEFAULT_INDEX_PAGE_NAME;
                                elements[2] = "";
                                elements[URL_ELEMENT_DEVICE_NUMBER] = "0";
                                elements[URL_ELEMENT_METHOD] = SETUP_DEFAULT_INDEX_PAGE_NAME; // Copy the command name to the device method field
                                requestData.Elements = elements;
                                ReturnHTMLPageOrImage(requestData, SETUP_DEFAULT_INDEX_PAGE_NAME);
                                break;
                            }
                        case 2: // setup/filename
                            {
                                Array.Resize<string>(ref elements, 5);
                                elements[2] = "";
                                elements[URL_ELEMENT_DEVICE_NUMBER] = "0";
                                elements[URL_ELEMENT_METHOD] = elements[1]; // Copy the command name to the device method field
                                requestData.Elements = elements;
                                ReturnHTMLPageOrImage(requestData, elements[1]);
                                break;
                            }
                        case 3: // setup/something/something-else - not a recognised command format
                        case 4: // setup/something/something-else/another-something - not a recognised command format
                            {
                                LogMessage(0, 0, 0, "Setup", $"Could not find requested file {request.Url.AbsolutePath}");
                                Return404Error(requestData, $"Could not find requested file {request.Url.AbsolutePath}");
                                break;
                            }
                        case 5: // setup/vx/device/device-number/setup
                            {
                                if (elements[4] == "setup") // This is a valid device setup URL
                                {
                                    requestData.Elements = elements;
                                    ReturnHTMLPageOrImage(requestData, SETUP_DEVICE_DEFAULT_INDEX_PAGE_NAME);
                                }
                                else // This is not a valid setup URL so return a 404 NOT FOUND response
                                {
                                    LogMessage(0, 0, 0, "Setup", $"Could not find requested page {request.Url.AbsolutePath}");
                                    Return404Error(requestData, $"The requested page is not present on the Remote Server: {request.Url.AbsolutePath}");
                                }
                                break;
                            }
                        case 6: // setup/vx/device/device-number/setup/index.html
                            {
                                requestData.Elements = elements;
                                if (elements[5] == SETUP_DEFAULT_INDEX_PAGE_NAME)
                                {
                                    ReturnHTMLPageOrImage(requestData, SETUP_DEVICE_DEFAULT_INDEX_PAGE_NAME);
                                }
                                else
                                {
                                    LogMessage(0, 0, 0, "Setup", $"Could not find requested file {request.Url.AbsolutePath}");
                                    Return404Error(requestData, $"Could not find requested file {request.Url.AbsolutePath}");
                                }
                                break;
                            }
                        default: // 7 or more elements - just return a not found message
                            {
                                LogMessage(0, 0, 0, "Setup", $"Could not find requested file {request.Url.AbsolutePath}");
                                Return404Error(requestData, $"Could not find requested file {request.Url.AbsolutePath}");
                                break;
                            }
                    }
                } // Process standard Alpaca HTML management requests

                else if (request.Url.AbsolutePath.Trim().StartsWith(SharedConstants.REMOTE_SERVER_MANAGEMENT_URL_BASE)) // Process server requests
                {
                    // Split the supplied URI into its elements demarked by the / character and remove any leading / trailing space characters from each element
                    // Element [0] will be "server"
                    // Element [1] will be the API version (whole number prefixed by V e.g. V1)
                    // Element [2] will be the configuration command
                    string[] elements = request.Url.AbsolutePath.Trim(FORWARD_SLASH).Split(FORWARD_SLASH);

                    // Basic error checking - We must have received 3 elements, now in the elements array, in order to have received a valid API request so check this here:
                    if (elements.Length != 3)
                    {
                        Return400Error(requestData, "Remote management - incorrect API format - Received: " + request.Url.AbsolutePath + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                        return;
                    }
                    else // We have received the required 3 elements in the URI
                    {
                        for (int i = 0; i < elements.Length; i++)
                        {
                            elements[i] = elements[i].Trim();// Remove leading and trailing space characters
                            LogMessage1(requestData, "ManagmentCommand", string.Format("Received element {0} = {1}", i, elements[i]));
                        }

                        // We have an array of size 3, now resize it to 5 elements so that we can add the command into the equivalent position that it occupies for a "device API" call.
                        // This is necessary so that the logging commands will work for both device API and MANAGEMENT commands
                        Array.Resize<string>(ref elements, 5);
                        elements[URL_ELEMENT_DEVICE_NUMBER] = "0";
                        elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_SERVER_COMMAND]; // Copy the command name to the device method field
                        requestData.Elements = elements;

                        // Only permit processing if access has been granted through the setup dialogue either to the management interface alone or if discovery is enabled
                        if (ManagementInterfaceEnabled | AlpacaDiscoveryEnabled)
                        {
                            switch (elements[URL_ELEMENT_API_VERSION])
                            {
                                case SharedConstants.API_VERSION_V1: // OK so we have a V1 request
                                    try // Confirm that the command requested is available on this server
                                    {
                                        string commandName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(elements[URL_ELEMENT_SERVER_COMMAND].ToLowerInvariant()); // Capitalise the first letter of the command

                                        switch (request.HttpMethod.ToUpperInvariant())
                                        {
                                            case "GET": // Read methods
                                                switch (elements[URL_ELEMENT_SERVER_COMMAND])
                                                {
                                                    // INT Get Values
                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_GET_CONCURRENT_CALLS:
                                                        // Returns the number of concurrent device calls. 
                                                        // This call will have incremented the concurrent call counter in its own right so the value returned by this call is one less than the numberOfConcurrentTransactions counter value,
                                                        // which will be the number of concurrent device calls.
                                                        IntResponse intResponseClass = new IntResponse(clientTransactionID, serverTransactionID, numberOfConcurrentTransactions - 1)
                                                        {
                                                            DriverException = null,
                                                            SerializeDriverException = IncludeDriverExceptionInJsonResponse
                                                        };
                                                        string intResponseJson = JsonConvert.SerializeObject(intResponseClass);
                                                        SendResponseValueToClient(requestData, null, intResponseJson);
                                                        break;

                                                    // STRING Get Values
                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_GET_PROFILE:
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
                                                            ProfileResponse profileResponse = new ProfileResponse(clientTransactionID, serverTransactionID, profileDevices)
                                                            {
                                                                DriverException = null,
                                                                SerializeDriverException = IncludeDriverExceptionInJsonResponse
                                                            };
                                                            string profileResponseJson = JsonConvert.SerializeObject(profileResponse);
                                                            SendResponseValueToClient(requestData, null, profileResponseJson);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Return500Error(requestData, "Unexpected exception in command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + ex.ToString());
                                                        }
                                                        break;

                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_GET_CONFIGURATION:
                                                        ConfigurationResponse configurationResponse = new ConfigurationResponse(clientTransactionID, serverTransactionID, ConfiguredDevices)
                                                        {
                                                            DriverException = null,
                                                            SerializeDriverException = IncludeDriverExceptionInJsonResponse
                                                        };
                                                        ;
                                                        string configurationResponseJson = JsonConvert.SerializeObject(configurationResponse);
                                                        SendResponseValueToClient(requestData, null, configurationResponseJson);
                                                        break;

                                                    default:
                                                        Return400Error(requestData, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                                        break;
                                                }
                                                break;
                                            case "PUT": // Write or action methods
                                                switch (elements[URL_ELEMENT_SERVER_COMMAND])
                                                {
                                                    // MANGEMENT_API_ENABLED removed because it won't work if the management api is disabled
                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_SHUTDOWN_SERVER:
                                                        LogMessage1(requestData, "ManagementShutdown", string.Format("Shutting down server - getting management lock"));
                                                        lock (managementCommandLock) // Make sure that this command can only run one at a time!
                                                        {
                                                            LogMessage1(requestData, "ManagementShutdown", $"Got lock - API is currently enabled: {apiIsEnabled}");

                                                            // Disable the device API
                                                            apiIsEnabled = false;

                                                            LogMessage1(requestData, "ManagementShutdown", $"Starting shutdown task");
                                                            // This is a "fire and forget" task so the IAsyncresult return value is discarded
                                                            _ = Task.Run(() => ShutdownTask());
                                                            LogMessage1(requestData, "ManagementShutdown", $"Completed shutdown task");

                                                            SendEmptyResponseToClient(requestData, null);
                                                        }
                                                        break;

                                                    // Restart the server by closing current drivers and reloading them
                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_RESTART_SERVER:
                                                        LogMessage1(requestData, "Management Restart", string.Format("Restarting server - getting management lock"));
                                                        lock (managementCommandLock) // Make sure that this command can only run one at a time!
                                                        {
                                                            bool originalAPiIsEnabledState = apiIsEnabled;
                                                            LogMessage1(requestData, "Management Restart", string.Format("Got lock - API is currently enabled: {0}", apiIsEnabled));
                                                            // Disable the device API
                                                            apiIsEnabled = false;
                                                            LogMessage1(requestData, "Management Restart", string.Format("Unloading drivers - devices are connected: {0}", devicesAreConnected));
                                                            if (devicesAreConnected) DisconnectDevices();
                                                            LogMessage1(requestData, "Management Restart", string.Format("Reloading drivers"));
                                                            WaitFor(MANGEMENT_RESTART_WAIT_TIME); // Wait for current device activity to complete
                                                            ConnectDevices();
                                                            apiIsEnabled = originalAPiIsEnabledState;
                                                            LogMessage1(requestData, "Management Restart", string.Format("Restored API enabled state: {0}, command completed", apiIsEnabled));
                                                            SendEmptyResponseToClient(requestData, null);
                                                        }
                                                        break;

                                                    case SharedConstants.REMOTE_SERVER_MANGEMENT_GET_CONFIGURATION:
                                                        lock (managementCommandLock) // Make sure that this command can only run one at a time!
                                                        {
                                                            Return400Error(requestData, "Command not implemented: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                                        }
                                                        break;

                                                    default:
                                                        Return400Error(requestData, "Unsupported Command: " + elements[URL_ELEMENT_SERVER_COMMAND] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                                        break;
                                                }
                                                break;
                                            default:
                                                Return400Error(requestData, "Unsupported http verb: " + request.HttpMethod);
                                                break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Return400Error(requestData, string.Format("Exception processing {0} command \r\n {1}", elements[URL_ELEMENT_SERVER_COMMAND], ex.ToString()));
                                    }

                                    break; // End of valid api version numbers
                                default:
                                    Return400Error(requestData, "Unsupported API version: " + elements[URL_ELEMENT_API_VERSION] + " " + HTMLOrPlainText(requestData, CORRECT_SERVER_FORMAT_STRING_HTML, CORRECT_SERVER_FORMAT_STRING_PLAIN));
                                    break;
                            }
                        }
                        else // The management interface is not enabled so return an error
                        {
                            LogMessage1(requestData, "Management", MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE);
                            Return403Error(requestData, MANAGEMENT_INTERFACE_NOT_ENABLED_MESSAGE);
                        }
                    }
                } // Process Remote Server management extension requests

                else // A URI that did not start with /api/, /management, /setup or /configuration/ was requested
                {
                    LogMessage1(requestData, "Request", string.Format("Non API call - {0} URL: {1}, Thread: {2}", request.HttpMethod, request.Url.PathAndQuery, Thread.CurrentThread.ManagedThreadId.ToString()));
                    string returnMessage = HTMLOrPlainText(requestData, UNRECOGNISED_URI_MESSAGE_HTML, UNRECOGNISED_URI_MESSAGE_PLAIN);

                    foreach (KeyValuePair<string, ConfiguredDevice> device in ConfiguredDevices)
                    {
                        if (device.Value.ProgID != SharedConstants.DEVICE_NOT_CONFIGURED)
                        {
                            returnMessage += $"{device.Value.DeviceType} {device.Value.DeviceNumber}: {device.Value.Description} ({device.Value.ProgID})" + HTMLOrPlainText(requestData, "<br>", "\r\n");
                        }
                    }
                    TransmitResponse(requestData, HTMLOrPlainTextContentType(requestData), HttpStatusCode.OK, "200 OK", returnMessage);
                } // Not a valid Remote Server request
            }
            catch (Exception ex) // Something serious has gone wrong with the ASCOM Remote server itself so report this to the user
            {
                LogException(clientID, clientTransactionID, serverTransactionID, "Request", ex.ToString());
                Return500Error(requestData, "Internal server error: " + ex.ToString());
            }
            finally
            {
                DecrementConcurrencyCounter(); // Decrement the concurrent transactions counter in a thread safe manner
            }
        } // Top level request handler for all request types

        internal void ProcessDriverCommand(RequestData requestData)
        {
            try
            {
                // Create three shortcut variables that are local to this thread
                device = ActiveObjects[requestData.DeviceKey].DeviceObject; // Try and access the device. If it does not exist in the active devices collection then a KeyNotFound exception is generated and handled below
                allowConnectedSetFalse = ActiveObjects[requestData.DeviceKey].AllowConnectedSetFalse; // If we get here then the user has requestData.Requested a device that does exist
                allowConnectedSetTrue = ActiveObjects[requestData.DeviceKey].AllowConnectedSetTrue;
                allowConcurrentAccess = ActiveObjects[requestData.DeviceKey].AllowConcurrentAccess;

                // Log a message showing the thread number on which we are running
                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Processing driver command on WORKER thread {Thread.CurrentThread.ManagedThreadId}");

                switch (requestData.Request.HttpMethod.ToUpperInvariant()) // Handle GET and PUT requestData.Requests
                {
                    case "GET": // Read and return data methods
                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                        {
                            #region Common methods
                            // Common methods are indicated in ReturnXXX methods by having the device type parameter set to "*" rather than the name of one of the ASCOM device types
                            // STRING Get Values
                            case "description":
                            case "driverinfo":
                            case "driverversion":
                            case "name":
                                ReturnString("*", requestData);
                                break;

                            case "supportedactions":
                                ReturnStringList("*", requestData);
                                break;

                            // SHORT Get Values
                            case "interfaceversion":
                                ReturnShort("*", requestData);
                                break;

                            // BOOL Get Values
                            case "connected":
                                ReturnBool("*", requestData);
                                break;
                            #endregion
                            default: // Not a common method so check for device specific methods
                                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE])
                                {
                                    case "telescope": // OK so we have a Telescope requestData.Request
                                        #region Telescope
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
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
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            // SHORT Get Values
                                            case "slewsettletime":
                                                ReturnShort(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

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
                                                ReturnDouble(requestData); break;

                                            //DATETIME Get values
                                            case "utcdate":
                                                ReturnDateTime(requestData); break;

                                            //ENUM TYPES Get values
                                            case "equatorialsystem":
                                                ReturnEquatorialSystem(requestData); break;
                                            case "alignmentmode":
                                                ReturnAlignmentMode(requestData); break;
                                            case "trackingrate":
                                                ReturnTrackingRate(requestData); break;
                                            case "sideofpier":
                                                ReturnSideofPier(requestData); break;

                                            //TRACKINGRATES Get value
                                            case "trackingrates":
                                                ReturnTrackingRates(requestData); break;
                                            #endregion

                                            #region Methods
                                            // METHODS
                                            case "axisrates":
                                                ReturnAxisRates(requestData);
                                                break;
                                            case "destinationsideofpier":
                                                ReturnDestinationSideOfPier(requestData);
                                                break;
                                            case "canmoveaxis":
                                                ReturnCanMoveAxis(requestData);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "camera":
                                        #region Camera
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
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
                                                ReturnShort(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            // INT Get Values
                                            case "cameraxsize":
                                            case "cameraysize":
                                            case "maxadu":
                                            case "numx":
                                            case "numy":
                                            case "startx":
                                            case "starty":
                                            case "offset":
                                            case "offsetmin":
                                            case "offsetmax":
                                                ReturnInt(requestData); break;
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
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
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
                                            case "subexposureduration":
                                                ReturnDouble(requestData); break;
                                            //STRING Get Values
                                            case "lastexposurestarttime":
                                            case "sensorname":
                                                ReturnString(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //CAMERSTATES Get Values
                                            case "camerastate":
                                                ReturnCameraStates(requestData); break;
                                            //IMAGEARRAY Get Values
                                            case "imagearray":
                                            case "imagearrayvariant":
                                                ReturnImageArray(requestData); break;
                                            case "imagearraybase64":
                                                ReturnImageArrayBase64(requestData); break;
                                            case "imagearraybytes":
                                                ReturnImageArrayBytes(requestData); break;
                                            //STRING LIST Get Values
                                            case "gains":
                                            case "readoutmodes":
                                            case "offsets":
                                                ReturnStringList(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //SENSORTYPE Get Values
                                            case "sensortype":
                                                ReturnSensorType(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "covercalibrator":
                                        #region CoverCalibrator
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            #region CoverCalibrator Properties
                                            // INT Get Values
                                            case "brightness":
                                            case "maxbrightness":
                                                ReturnInt(requestData); break;

                                            //ENUM TYPES Get values
                                            case "calibratorstate":
                                                ReturnCalibratorState(requestData); break;

                                            case "coverstate":
                                                ReturnCoverState(requestData); break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "dome":
                                        #region Dome
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
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
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            // DOUBLE Get Values
                                            case "altitude":
                                            case "azimuth":
                                                ReturnDouble(requestData); break;
                                            case "shutterstatus": //SensorState
                                                ReturnShutterStatus(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "filterwheel":
                                        #region Filter Wheel
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // INT ARRAY Get Values
                                            case "focusoffsets":
                                                ReturnIntArray(requestData); break;

                                            // SHORT Get Values
                                            case "position":
                                                ReturnShort(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //STRING ARRAY Get Values
                                            case "names":
                                                ReturnStringArray(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "focuser":
                                        #region Focuser
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            #region Focuser Properties
                                            // BOOL Get Values
                                            case "absolute":
                                            case "ismoving":
                                            case "tempcompavailable":
                                            case "tempcomp":
                                            case "link":
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            // INT Get Values
                                            case "maxincrement":
                                            case "maxstep":
                                            case "position":
                                                ReturnInt(requestData); break;

                                            //DOUBLE Get Values
                                            case "stepsize":
                                            case "temperature":
                                                ReturnDouble(requestData); break;

                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "observingconditions":
                                        #region ObservingConditions
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
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
                                                ReturnDouble(requestData); break;
                                            // STRING Get Values
                                            case "sensordescription":
                                                ReturnString(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "rotator":
                                        #region Rotator
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // BOOL Get Values
                                            case "canreverse":
                                            case "ismoving":
                                            case "reverse":
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //FLOAT Get Values
                                            case "position":
                                            case "stepsize":
                                            case "targetposition":
                                            case "mechanicalposition":
                                                ReturnFloat(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "safetymonitor":
                                        #region SafetyMonitor
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // BOOL Get Values
                                            case "issafe":
                                                ReturnBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "switch":
                                        #region Switch
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // BOOL Get Values
                                            case "canwrite":
                                            case "getswitch":
                                                ReturnShortIndexedBool(requestData); break;
                                            // SHORT Get Values
                                            case "maxswitch":
                                                ReturnShort(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            // DOUBLE Get Values
                                            case "getswitchvalue":
                                            case "maxswitchvalue":
                                            case "minswitchvalue":
                                            case "switchstep":
                                                ReturnShortIndexedDouble(requestData); break;
                                            // STRING Get Values
                                            case "getswitchdescription":
                                            case "getswitchname":
                                                ReturnShortIndexedString(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, GET_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion

                                    default:// End of valid device types
                                        Return400Error(requestData, "Unsupported Device Type: " + requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                        break;
                                }
                                break;
                        }
                        break;
                    case "PUT": // Write or action methods
                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                        {
                            #region Common methods
                            // Process common methods shared by all drivers
                            case "commandblind":
                                CallMethod("*", requestData);
                                break;
                            case "commandbool":
                                ReturnBool("*", requestData);
                                break;
                            case "commandstring":
                                ReturnString("*", requestData);
                                break;
                            case "action":
                                ReturnString("*", requestData);
                                break;
                            case "connected":
                                WriteBool("*", requestData);
                                break;
                            #endregion
                            default:
                                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE])
                                {
                                    case "telescope": // OK so we have a Telescope requestData.Request
                                        #region Telescope
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            #region Telescope Properties
                                            //BOOL Set values
                                            case "tracking":
                                            case "doesrefraction":
                                                WriteBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //SHORT Set Values
                                            case "slewsettletime":
                                                WriteShort(requestData); break;

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
                                                WriteDouble(requestData); break;
                                            //DATETIME Set values
                                            case "utcdate":
                                                WriteDateTime(requestData); break;

                                            //ENUM TYPES Set values
                                            case "trackingrate":
                                                WriteTrackingRate(requestData); break;
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
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "camera":
                                        #region Camera
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // METHODS
                                            case "abortexposure":
                                            case "pulseguide":
                                            case "startexposure":
                                            case "stopexposure":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //SHORT Set values
                                            case "binx":
                                            case "biny":
                                            case "gain":
                                            case "readoutmode":
                                                WriteShort(requestData); break;
                                            //INT Set values
                                            case "numx":
                                            case "numy":
                                            case "startx":
                                            case "starty":
                                            case "offset":
                                                WriteInt(requestData); break;
                                            //DOUBLE Set values
                                            case "setccdtemperature":
                                            case "subexposureduration":
                                                WriteDouble(requestData); break;
                                            //BOOL Set values
                                            case "cooleron":
                                            case "fastreadout":
                                                WriteBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "covercalibrator":
                                        #region CoverCalibrator
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            #region CoverCalibrator Methods
                                            // METHODS
                                            case "calibratoroff":
                                            case "calibratoron":
                                            case "closecover":
                                            case "haltcover":
                                            case "opencover":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "dome":
                                        #region Dome
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
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
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //BOOL Set values
                                            case "slaved":
                                                WriteBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "filterwheel":
                                        #region Filter Wheel
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            //SHORT Set values
                                            case "position":
                                                WriteShort(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "focuser":
                                        #region Focuser
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            #region Focuser Properties
                                            //BOOL Set values
                                            case "tempcomp":
                                            case "link":
                                                WriteBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            #endregion

                                            #region Focuser Methods
                                            // METHODS
                                            case "halt":
                                            case "move":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData);
                                                break;
                                            #endregion

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break;
                                    #endregion
                                    case "observingconditions":
                                        #region ObservingConditions
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // METHODS
                                            case "refresh":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //DOUBLE Set values
                                            case "averageperiod":
                                                WriteDouble(requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "rotator":
                                        #region Rotator
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // METHODS
                                            case "halt":
                                            case "move":
                                            case "moveabsolute":
                                            case "sync":
                                            case "movemechanical":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;
                                            //BOOL Set values
                                            case "reverse":
                                                WriteBool(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion
                                    case "switch":
                                        #region Switch
                                        switch (requestData.Elements[URL_ELEMENT_METHOD])
                                        {
                                            // METHODS
                                            case "setswitchname":
                                            case "setswitch":
                                            case "setswitchvalue":
                                                CallMethod(requestData.Elements[URL_ELEMENT_DEVICE_TYPE], requestData); break;

                                            //UNKNOWN METHOD CALL
                                            default:
                                                Return400Error(requestData, PUT_UNKNOWN_METHOD_MESSAGE + requestData.Elements[URL_ELEMENT_METHOD] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                                break;
                                        }
                                        break; // End of valid device types
                                    #endregion

                                    default:
                                        Return400Error(requestData, "Unsupported Device Type: " + requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + " " + HTMLOrPlainText(requestData, CORRECT_API_FORMAT_STRING_HTML, CORRECT_API_FORMAT_STRING_PLAIN));
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        Return400Error(requestData, $"Invalid http verb: {requestData.Request.HttpMethod}, the Alpaca protocol only uses GET and PUT.");
                        break;
                } // Handle GET and PUT requestData.Requests
            }
            catch (KeyNotFoundException ex)
            {
                LogException1(requestData, "ProcessDriverCommand", ex.ToString());
                Return400Error(requestData, $"The requested device \"{requestData.Elements[URL_ELEMENT_DEVICE_TYPE]} {requestData.Elements[URL_ELEMENT_DEVICE_NUMBER]}\" does not exist on this server. Supplied URI: {requestData.Request.Url.AbsolutePath}");
            }
        } // Driver request handler

        #endregion

        #region API response methods

        // Return server /setup HTML responses to clients
        private void ReturnHTMLPageOrImage(RequestData requestData, string htmlPageName)
        {
            byte[] bytesToSend; // Array to hold the encoded message
            string indexPage;

            try
            {
                LogMessage1(requestData, "Web - HTTP 200 Success", htmlPageName);
                if (ScreenLogResponses) LogToScreen(string.Format("HTTP 200 Success - ClientId: {0}, ClientTransactionID: {1} - {2}", requestData.ClientID, requestData.ClientTransactionID, htmlPageName));

                switch (htmlPageName) // Construct the relevant page or return the requested file
                {
                    case SETUP_DEFAULT_INDEX_PAGE_NAME: // Synthesise the main setup index page and turn it into bytes
                        {
                            if (DebugTraceState) LogMessage1(requestData, "Web - Encoding setup page", "Starting");

                            indexPage =
                                "<!DOCTYPE html>" +
                                "<html>" +
                                    "<head>" +
                                        "<title> ASCOM Remote Server</title>" +
                                        "<style>" +
                                            "body {" +
                                                "background-color: black;" +
                                                "text-align: center;" +
                                                "color: white;" +
                                                "font-family: sans-serif;" +
                                                "font-size: 12pt;" +
                                            "}" +
                                            "table.center {" +
                                                "margin-left:auto; " +
                                                "margin-right:auto;" +
                                            "}" +

                                            "table, th, td {" +
                                                "border: 1px solid white;" +
                                            "}" +
                                            "th, td {" +
                                                "padding: 8px;" +
                                            "}" +
                                            "td.boldyellow {" +
                                              "font-weight: bold;" +
                                              "color: yellow;" +
                                            "}" +
                                            "h2.lightgreen {" +
                                              "color: lightgreen;" +
                                            "}" +
                                        "</style>" +
                                        "<link rel=\"shortcut icon\" href=\"/setup/ascomicon.ico\" type=\"image/x-icon\" />" +
                                   "</head>" +
                                    "<body>" +
                                        "<h1>ASCOM Remote Server</h1>" +
                                        "<h2 class=\"lightgreen\">Located at " + RemoteServerLocation + "</h2>" +
                                        "<br>" +
                                        "<p><b>The following devices are configured on this Remote Server:</b></p>" +
                                        "<table class=\"center\">" +
                                            "<thead>" +
                                                "<tr>" +
                                                    "<td class=\"boldyellow\">Device Type</td>" +
                                                    "<td class=\"boldyellow\">Device Number</td>" +
                                                    "<td class=\"boldyellow\">Description</td>" +
                                                    "<td class=\"boldyellow\">ProgID</td>" +
                                                    "<td class=\"boldyellow\">Initialised OK</td>" +
                                                "</tr>" +
                                            "</thead>" +
                                            "<tbody>";

                            foreach (KeyValuePair<string, ConfiguredDevice> configuredDevice in ConfiguredDevices)
                            {
                                if (configuredDevice.Value.DeviceType != "None")
                                {
                                    indexPage +=
                                                "<tr>" +
                                                    "<td>" + configuredDevice.Value.DeviceType + "</td>" +
                                                    "<td>" + configuredDevice.Value.DeviceNumber + "</td>" +
                                                    "<td>" + configuredDevice.Value.Description + "</td>" +
                                                    "<td>" + configuredDevice.Value.ProgID + "</td>" +
                                                    "<td>" + ActiveObjects[configuredDevice.Value.DeviceKey].InitialisedOk + "</td>" +
                                                "</tr>";
                                }
                            }

                            indexPage +=
                                            "</tbody>" +
                                        "</table>" +
                                        "<br>" +
                                        "<h3>Please use the Remote Server's setup GUI to change the configuration</h3>" +
                                        "<br>" +
                                        "<img src = \"/setup/ASCOMAlpacaMidRes.jpg\"alt=\"AlpacaLogo\" style=\"width:200px\">" +
                                        "<p>Version </h3>" + Assembly.GetEntryAssembly().GetName().Version.ToString() +
                                    "</body>" +
                                "</html>";

                            bytesToSend = Encoding.UTF8.GetBytes(indexPage); // Convert the message to be returned into UTF8 bytes that can be sent over the wire
                            if (DebugTraceState) LogMessage1(requestData, "Web - Encoding setup page", "Completed");
                            break;
                        }
                    case SETUP_DEVICE_DEFAULT_INDEX_PAGE_NAME: // Synthesise the individual device setup index page and turn it into bytes
                        {
                            if (DebugTraceState) LogMessage1(requestData, "Web - Encoding individual device setup page", "Starting");

                            indexPage =
                                "<!DOCTYPE html>" +
                                "<html>" +
                                    "<head>" +
                                        "<title> ASCOM Remote Server Device Setup</title>" +
                                        "<style>" +
                                            "body {" +
                                                "background-color: black;" +
                                                "text-align: center;" +
                                                "color: white;" +
                                                "font-family: sans-serif;" +
                                                "font-size: 12pt;" +
                                            "}" +
                                            "h2.lightgreen {color: lightgreen;}" +
                                            "h3.yellow {color: yellow;}" +
                                            "h3.red {color: red;}" +
                                        "</style>" +
                                        "<link rel=\"shortcut icon\" href=\"/setup/ascomicon.ico\" type=\"image/x-icon\" />" +
                                    "</head>" +
                                    "<body>" +
                                        "<h1>ASCOM Remote Server</h1>" +
                                        "<h2 class=\"lightgreen\">Located at " + RemoteServerLocation + "</h2>" +
                                        "<br>";

                            string deviceKey = $"{requestData.Elements[URL_ELEMENT_DEVICE_TYPE]}/{requestData.Elements[URL_ELEMENT_DEVICE_NUMBER]}"; // Create the device key from the supplied device type and device number parameters
                            if (ActiveObjects.ContainsKey(deviceKey)) // The specified device does exist so return a "can't configure this" response
                            {
                                // Get the configured device information for this device
                                ConfiguredDevice configuredDevice = ConfiguredDevices[ActiveObjects[deviceKey].ConfiguredDeviceKey];
                                indexPage +=
                                        $"<h3 class=\"yellow\">The {configuredDevice.Description} {configuredDevice.DeviceType} device can't be configured through the Remote Server web interface</h3>" +
                                        "<p>Please use the device's \"Properties\" button in the Remote Server Setup screen</p>";
                            }
                            else // The specified device does not exist so return a message to that effect
                            {
                                indexPage +=
                                        $"<h3 class=\"red\">The specified device: \"{requestData.Elements[URL_ELEMENT_DEVICE_TYPE]} {requestData.Elements[URL_ELEMENT_DEVICE_NUMBER]}\" is not configured on this Remote Server</h3>";
                            }

                            indexPage +=
                                        "<br>" +
                                        "<br>" +
                                        "<img src = \"/setup/ASCOMAlpacaMidRes.jpg\"alt=\"AlpacaLogo\" style=\"width:200px\">" +
                                        "<p>Version </h3>" + Assembly.GetEntryAssembly().GetName().Version.ToString() +
                                    "</body>" +
                                "</html>";

                            bytesToSend = Encoding.UTF8.GetBytes(indexPage); // Convert the message to be returned into UTF8 bytes that can be sent over the wire
                            if (DebugTraceState) LogMessage1(requestData, "Web - Encoding setup page", "Completed");
                            break;
                        }
                    default:// Read the file from disk
                        {
                            if (DebugTraceState) LogMessage1(requestData, "Web - Read file", "Starting");
                            bytesToSend = File.ReadAllBytes(".\\" + htmlPageName);
                            if (DebugTraceState) LogMessage1(requestData, "Web - Read file", "Completed");
                            break;
                        }
                }
                try
                {
                    if (htmlPageName.ToLowerInvariant().Contains(".html"))
                    {
                        requestData.Response.ContentType = "text/html; charset=utf-8";
                    }
                    else if ((htmlPageName.ToLowerInvariant().Contains(".jpeg")) || (htmlPageName.ToLowerInvariant().Contains(".jpg")))
                    {
                        requestData.Response.ContentType = "image/jpeg";
                        requestData.Response.Headers.Add("Cache-Control", "public, max-age=86400");
                    }
                    else if (htmlPageName.ToLowerInvariant().Contains(".ico"))
                    {
                        requestData.Response.ContentType = "image/x-icon";
                        requestData.Response.Headers.Add("Cache-Control", "public, max-age=86400");
                    }
                    else
                    {
                        throw new InvalidValueException($"Unknown content type in file {htmlPageName}");
                    }
                    requestData.Response.StatusCode = (int)HttpStatusCode.OK; // Set the response status and status code
                    requestData.Response.StatusDescription = "200 - Success";

                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Web - Before setting response bytes - Length: {0:n0}, Response is null: {1}", bytesToSend.Length, requestData.Response == null));
                    requestData.Response.ContentLength64 = bytesToSend.Length;

                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Web - Before writing {0:n0} bytes to output stream", requestData.Response.ContentLength64));
                    requestData.Response.OutputStream.Write(bytesToSend, 0, bytesToSend.Length);

                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "Web - After writing bytes to output stream");
                    requestData.Response.OutputStream.Close();
                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "Web - After closing output stream");
                }
                catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
                {
                    LogException1(requestData, "ListenerException", string.Format("Web - Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
                }

            }
            catch (FileNotFoundException ex)
            {
                LogException(0, 0, 0, $"Could not find requested file {htmlPageName}", ex.ToString());
                Return404Error(requestData, $"Could not find requested file {htmlPageName}: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, $"Exception while returning file {htmlPageName}", ex.ToString());
                Return500Error(requestData, $"Remote Server internal error while returning requested file {htmlPageName}: {ex.Message}");
            }
        }

        // Return an HTTP 200 success response with an empty body to clients
        private void ReturnEmpty200Success(RequestData requestData)
        {
            try
            {
                LogMessage1(requestData, "HTTP 200 Success", "returned to client");
                TransmitResponse(requestData, "text/html; charset=utf-8", HttpStatusCode.OK, "400 - Success", "");
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "Exception while returning empty HTTP 200 Success", ex.ToString());
            }
        }

        // Return server error responses to clients
        private void Return400Error(RequestData requestData, string message)
        {

            try
            {
                LogMessage1(requestData, "HTTP 400 Error", message);
                LogToScreen(string.Format("ERROR - ClientId: {0}, ClientTransactionID: {1} - {2}", requestData.ClientID, requestData.ClientTransactionID, message));

                TransmitResponse(requestData, HTMLOrPlainTextContentType(requestData), HttpStatusCode.BadRequest, "400 - Unable to process request - see response content for details", CleanMessage(message));
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "Exception while returning HTTP 400 Error", ex.ToString());
            }
        }

        private void Return403Error(RequestData requestData, string message)
        {
            try
            {
                LogMessage1(requestData, "HTTP 403 Error", message);
                LogToScreen(string.Format("ERROR - ClientId: {0}, ClientTransactionID: {1} - {2}", requestData.ClientID, requestData.ClientTransactionID, message));

                TransmitResponse(requestData, HTMLOrPlainTextContentType(requestData), HttpStatusCode.Forbidden, "403 " + CleanMessage(message), "403 " + CleanMessage(message));
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "Exception while returning HTTP 403 Error", ex.ToString());
            }
        }

        private void Return404Error(RequestData requestData, string message)
        {
            try
            {
                LogMessage1(requestData, "HTTP 404 Error", message);
                LogToScreen($"ERROR - ClientId: {requestData.ClientID}, ClientTransactionID: {requestData.ClientTransactionID} - {message}");

                TransmitResponse(requestData, HTMLOrPlainTextContentType(requestData), HttpStatusCode.NotFound, "404 " + CleanMessage(message), "404 " + CleanMessage(message));
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "Exception while returning HTTP 404 Error", ex.ToString());
            }
        }

        internal void Return500Error(RequestData requestData, string errorMessage)
        {
            try
            {
                LogMessage1(requestData, "HTTP 500 Error", errorMessage);
            }
            catch (Exception ex)
            {
                LogException(0, 0, 0, "Exception while returning HTTP 500 Error", ex.ToString());
            }
            TransmitResponse(requestData, HTMLOrPlainTextContentType(requestData), HttpStatusCode.InternalServerError, "500 " + CleanMessage(errorMessage), "500 " + CleanMessage(errorMessage));
        }

        // Return device responses to clients
        private void ReturnBool(string deviceType, RequestData requestData)
        {
            bool deviceResponse = false;
            string command;
            bool raw;
            Exception exReturn = null;

            try
            {
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    #region Common methods
                    case "*.connected":
                        deviceResponse = device.Connected; break;
                    #endregion

                    #region Telescope Methods
                    case "*.commandbool":
                        command = GetParameter<string>(requestData, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(requestData, SharedConstants.RAW_PARAMETER_NAME);
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
                    case "focuser.link":
                        deviceResponse = device.Link; break;

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
                        LogMessage1(requestData, "ReturnBool", "Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnBool - Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            BoolResponse responseClass = new BoolResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void ReturnShortIndexedBool(RequestData requestData)
        {
            bool deviceResponse = false;
            Exception exReturn = null;

            try
            {
                short index = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);

                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    #region Switch Methods

                    case "switch.canwrite":
                        deviceResponse = device.CanWrite(index); break;
                    case "switch.getswitch":
                        deviceResponse = device.GetSwitch(index); break;

                    #endregion

                    default:
                        LogMessage1(requestData, "ReturnShortIndexedBool", "Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnShortIndexedBool - Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            BoolResponse responseClass = new BoolResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void WriteBool(string deviceType, RequestData requestData)
        {
            bool boolValue;
            Exception exReturn = null;

            try
            {
                boolValue = GetParameter<bool>(requestData, requestData.Elements[URL_ELEMENT_METHOD]);
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // COMMON METHODS
                    case "*.connected":
                        if (boolValue == true) // We are being asked to set Connected to True
                        {
                            if (allowConnectedSetTrue)
                            {
                                LogMessage1(requestData, "Connected Set True", "Changing Connected state because the \"Allow Connected Set True\" setting is true");
                                device.Connected = boolValue;
                            }
                            else
                            {
                                LogMessage1(requestData, "Connected Set True", "Ignoring Connected state change because the \"Allow Connected Set True\" setting is false");
                            }
                        }
                        else // We are being asked to set Connected to False
                        {
                            if (allowConnectedSetFalse)
                            {
                                LogMessage1(requestData, "Connected Set False", "Changing Connected state because the \"Allow Connected Set False\" setting is true");
                                device.Connected = boolValue;
                            }
                            else
                            {
                                LogMessage1(requestData, "Connected Set False", "Ignoring Connected state change because the \"Allow Connected Set False\" setting is false");
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
                    case "focuser.link":
                        device.Link = boolValue; break;

                    //ROTATOR
                    case "rotator.reverse":
                        device.Reverse = boolValue; break;

                    default:
                        LogMessage1(requestData, "WriteBool", "Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("WriteBool - Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnString(string deviceType, RequestData requestData)
        {
            string deviceResponse = "";
            string command;
            string parameters;
            bool raw;
            Exception exReturn = null;
            Stopwatch sw = new Stopwatch();
            string responseJson = "";
            Stopwatch swOverall = new Stopwatch();
            swOverall.Start();

            try
            {
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // COMMON METHODS
                    case "*.action":
                        command = GetParameter<string>(requestData, SharedConstants.ACTION_COMMAND_PARAMETER_NAME);
                        parameters = GetParameter<string>(requestData, SharedConstants.ACTION_PARAMETERS_PARAMETER_NAME);

                        deviceResponse = device.Action(command, parameters);
                        break;

                    case "*.description":
                        deviceResponse = device.Description; break;
                    case "*.driverinfo":
                        deviceResponse = device.DriverInfo; break;
                    case "*.driverversion":
                        deviceResponse = device.DriverVersion; break;
                    case "*.name":
                        deviceResponse = device.Name; break;
                    case "*.commandstring":
                        command = GetParameter<string>(requestData, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(requestData, SharedConstants.RAW_PARAMETER_NAME);
                        deviceResponse = device.CommandString(command, raw);
                        break;

                    // CAMERA
                    case "camera.lastexposurestarttime":
                        deviceResponse = device.LastExposureStartTime; break;
                    case "camera.sensorname":
                        deviceResponse = device.SensorName; break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.sensordescription":
                        string stringParamValue = GetParameter<string>(requestData, SharedConstants.SENSORNAME_PARAMETER_NAME);
                        deviceResponse = device.SensorDescription(stringParamValue); break;

                    default:
                        LogMessage1(requestData, "ReturnString", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnString - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            sw.Restart();
            StringResponse responseClass = new StringResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };

            responseJson = JsonConvert.SerializeObject(responseClass);
            //responseJson = string.Format("{{\"ClientTransactionID\":36,\"ServerTransactionID\":36,\"ErrorNumber\":0,\"ErrorMessage\":\"\",\"Value\":\"{0}\"}}", deviceResponse);

            LogMessage1(requestData, "ReturnString", $"Serialise string time: {sw.ElapsedMilliseconds}ms.");
            sw.Restart();
            SendResponseValueToClient(requestData, exReturn, responseJson);
            LogMessage1(requestData, "ReturnString", $"Transmission time to client: {sw.ElapsedMilliseconds}ms. Overall time: {swOverall.ElapsedMilliseconds}ms.");

        }

        private static void ReturnImageArrayBytes(RequestData requestData)
        {
            Stopwatch sw = new Stopwatch();
            Stopwatch swOverall = new Stopwatch();

            Array imageArray;
            byte[] imageArrayBytes;

            swOverall.Start();
            sw.Start();

            try
            {
#if IMAGEARRAYTEST
                int numX = (int)ActiveObjects[requestData.DeviceKey].DeviceObject.NumX;
                int numY = (int)ActiveObjects[requestData.DeviceKey].DeviceObject.NumY;
                int[,] returnArray = new int[numX, numY];

                for (int i = 0; i < numX; i++)
                {
                    for (int j = 0; j < numY; j++)
                    {
                        returnArray[i, j] = i + j;
                    }
                }
                imageArray = returnArray;
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"***** TEST IMAGE RETURNED INSTEAD OF CAMERA IMAGE *****.");
#else
                imageArray = (Array)ActiveObjects[requestData.DeviceKey].DeviceObject.ImageArray;       //.LastImageArray;
#endif
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"TIME TO GET IMAGE ARRAY: {sw.ElapsedMilliseconds}ms.");

                // Convert the supplied array to a byte[]
                sw.Restart();
                imageArrayBytes = imageArray.ToByteArray(1, requestData.ClientTransactionID, requestData.ServerTransactionID, 0, "");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"TIME TO CONVERT MAGE ARRAY TO BYTE[]: {sw.ElapsedMilliseconds}ms.");
                imageArray = null;

                // Force a garbage collection of large objects to free memory
                GC.Collect(2, GCCollectionMode.Forced, true);

                ArrayMetadataV1 arrayMetadataV1 = imageArrayBytes.GetMetadataV1();
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Converted byte array - Version: {arrayMetadataV1.MetadataVersion}, Image element type: {arrayMetadataV1.ImageElementType}, Transmission element type: {arrayMetadataV1.TransmissionElementType}, Rank: {arrayMetadataV1.Rank}, Dimension 1: {arrayMetadataV1.Dimension1}, Dimension 2: {arrayMetadataV1.Dimension2}, Dimension 3: {arrayMetadataV1.Dimension3}");
                sw.Restart();

            }
            catch (Exception ex)
            {
                // Set the error number to a non-zero value and add the error message as the data payload.
                imageArrayBytes = AlpacaTools.ErrorMessageToByteArray(1, requestData.ClientTransactionID, requestData.ServerTransactionID, ExceptionHelpers.ErrorCodeFromException(ex), ex.Message);
                LogException1(requestData, "Exception creating image byte array", ex.ToString());
            }

            try
            {
                requestData.Response.ContentType = "application/octet-stream";
                requestData.Response.StatusCode = (int)HttpStatusCode.OK; // Set the response status and status code
                requestData.Response.StatusDescription = $"OK";

                // Condition requestData.Element so that the logging lines below will work correctly
                if (requestData.Elements == null) requestData.Elements = new string[5] { "", "", "", "", "SendResponseToClient" };
                if (requestData.Elements.Length < 5)
                {
                    string[] elements = requestData.Elements;
                    Array.Resize<string>(ref elements, 5);
                    elements[3] = "";
                    elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_SERVER_COMMAND];
                    requestData.Elements = elements;
                }

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Completed Encoding.GetBytes, array length: {0:n0}", imageArrayBytes.Length));

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before setting response bytes - Length: {0:n0}, Response is null: {1}", imageArrayBytes.Length, requestData.Response == null));
                requestData.Response.ContentLength64 = imageArrayBytes.Length;

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before writing {0:n0} bytes to output stream", requestData.Response.ContentLength64));
                requestData.Response.OutputStream.Write(imageArrayBytes, 0, imageArrayBytes.Length);

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After writing bytes to output stream");
                requestData.Response.OutputStream.Close();
                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After closing output stream");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"IMAGEARRAYBYTES TRANSMISSION TIME: {sw.ElapsedMilliseconds}ms.");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"IMAGEARRAYBYTES OVERALL TIME: {swOverall.ElapsedMilliseconds}ms.");

            }
            catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
            {
                LogException1(requestData, "ListenerException", string.Format("Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
            }

            // Force release of the large byte array
            imageArrayBytes = null;
            GC.Collect();
        }

        private void ReturnShortIndexedString(RequestData requestData)
        {
            string deviceResponse = "";
            Exception exReturn = null;

            try
            {
                short index = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);

                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    #region Switch Methods

                    case "switch.getswitchdescription":
                        deviceResponse = device.GetSwitchDescription(index); break;
                    case "switch.getswitchname":
                        deviceResponse = device.GetSwitchName(index); break;

                    #endregion

                    default:
                        LogMessage1(requestData, "ReturnShortIndexedString", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnShortIndexedString - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringResponse responseClass = new StringResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void ReturnStringArray(RequestData requestData)
        {
            string[] deviceResponse = new string[1];
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // FILTER WHEEL
                    case "filterwheel.names":
                        deviceResponse = device.Names; break;

                    default:
                        LogMessage1(requestData, "ReturnStringArray", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnStringArray - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringArrayResponse responseClass = new StringArrayResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void ReturnStringList(string deviceType, RequestData requestData)
        {
            ArrayList deviceResponse;
            List<string> responseList = new List<string>();
            Exception exReturn = null;

            try
            {
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
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

                    case "camera.offsets":
                        deviceResponse = (ArrayList)device.Offsets;
                        foreach (string offset in deviceResponse)
                        {
                            responseList.Add(offset);
                        }
                        break;

                    default:
                        LogMessage1(requestData, "ReturnStringList", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnStringList - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            StringListResponse responseClass = new StringListResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, responseList)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnDouble(RequestData requestData)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
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
                    case "camera.subexposureduration":
                        deviceResponse = device.SubExposureDuration; break;
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
                        string stringParamValue = GetParameter<string>(requestData, SharedConstants.SENSORNAME_PARAMETER_NAME);
                        deviceResponse = device.TimeSinceLastUpdate(stringParamValue); break;

                    default:
                        LogMessage1(requestData, "ReturnDouble", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnDouble - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void ReturnShortIndexedDouble(RequestData requestData)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;

            try
            {
                short index = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);

                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
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
                        LogMessage1(requestData, "ReturnShortIndexedDouble", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnShortIndexedDouble - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void WriteDouble(RequestData requestData)
        {
            double doubleValue;
            Exception exReturn = null;

            try
            {
                doubleValue = GetParameter<double>(requestData, requestData.Elements[URL_ELEMENT_METHOD]);
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // TELESCOPE
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
                    case "camera.subexposureduration":
                        device.SubExposureDuration = doubleValue; break;
                    // OBSERVINGCONDITIONS
                    case "observingconditions.averageperiod":
                        device.AveragePeriod = doubleValue; break;

                    default:
                        LogMessage1(requestData, "WriteDouble", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("WriteDouble - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnFloat(RequestData requestData)
        {
            double deviceResponse = 0.0;
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // ROTATOR
                    case "rotator.position":
                        deviceResponse = (double)device.Position; break;
                    case "rotator.stepsize":
                        deviceResponse = (double)device.StepSize; break;
                    case "rotator.targetposition":
                        deviceResponse = (double)device.TargetPosition; break;
                    case "rotator.mechanicalposition":
                        deviceResponse = (double)device.MechanicalPosition; break;

                    default:
                        LogMessage1(requestData, "ReturnFloat", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnFloat - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DoubleResponse responseClass = new DoubleResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnShort(string deviceType, RequestData requestData)
        {
            short deviceResponse = 0;
            Exception exReturn = null;

            try
            {
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
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
                        LogMessage1(requestData, "ReturnShort", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnShort - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            ShortResponse responseClass = new ShortResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void WriteShort(RequestData requestData)
        {
            short shortValue;
            Exception exReturn = null;

            try
            {
                shortValue = GetParameter<short>(requestData, requestData.Elements[URL_ELEMENT_METHOD]);
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
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
                        LogMessage1(requestData, "WriteShort", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("WriteShort - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnInt(RequestData requestData)
        {
            int deviceResponse = 0;
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // COVERCALIBRATOR
                    case "covercalibrator.brightness":
                        deviceResponse = device.Brightness; break;
                    case "covercalibrator.maxbrightness":
                        deviceResponse = device.MaxBrightness; break;
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
                    case "camera.offset":
                        deviceResponse = device.Offset; break;
                    case "camera.offsetmin":
                        deviceResponse = device.OffsetMin; break;
                    case "camera.offsetmax":
                        deviceResponse = device.OffsetMax; break;

                    default:
                        LogMessage1(requestData, "ReturnInt", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnInt - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void ReturnIntArray(RequestData requestData)
        {
            int[] deviceResponse = new int[1];
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // FILTER WHEEL
                    case "filterwheel.focusoffsets":
                        deviceResponse = device.FocusOffsets; break;

                    default:
                        LogMessage1(requestData, "ReturnIntArray", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnIntArray - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntArray1DResponse responseClass = new IntArray1DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void WriteInt(RequestData requestData)
        {
            int intValue;
            Exception exReturn = null;

            try
            {
                intValue = GetParameter<int>(requestData, requestData.Elements[URL_ELEMENT_METHOD]);
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // CAMERA
                    case "camera.numx":
                        device.NumX = intValue; break;
                    case "camera.numy":
                        device.NumY = intValue; break;
                    case "camera.offset":
                        device.Offset = intValue; break;
                    case "camera.startx":
                        device.StartX = intValue; break;
                    case "camera.starty":
                        device.StartY = intValue; break;

                    default:
                        LogMessage1(requestData, "WriteInt", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("WriteInt - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnDateTime(RequestData requestData)
        {
            DateTime deviceResponse = DateTime.Now;
            Exception exReturn = null;

            try
            {
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    case "telescope.utcdate":
                        deviceResponse = device.UTCDate; break;

                    default:
                        LogMessage1(requestData, "ReturnDateTime", "Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnDateTime - Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            DateTimeResponse responseClass = new DateTimeResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);

        }
        private void WriteDateTime(RequestData requestData)
        {
            DateTime dateTimeValue;
            Exception exReturn = null;

            try
            {
                dateTimeValue = GetParameter<DateTime>(requestData, SharedConstants.UTCDATE_PARAMETER_NAME);
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "Converted DateTime value (UTC): " + dateTimeValue.ToUniversalTime().ToString(SharedConstants.ISO8601_DATE_FORMAT_STRING));
                switch (requestData.Elements[URL_ELEMENT_DEVICE_TYPE] + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    case "telescope.utcdate":
                        device.UTCDate = dateTimeValue.ToUniversalTime(); break;

                    default:
                        LogMessage1(requestData, "WriteDateTime", "Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("WriteDateTime - Unsupported method: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnTrackingRates(RequestData requestData)
        {
            IEnumerable deviceResponse = null;
            Exception exReturn = null;

            try
            {
                deviceResponse = (System.Collections.IEnumerable)device.TrackingRates;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            TrackingRatesResponse responseClass = new TrackingRatesResponse(requestData.ClientTransactionID, requestData.ServerTransactionID)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };

            // Initialise a list to hold returned tracking rate values and add any tracking rate values that have been returned
            List<DriveRates> rates = new List<DriveRates>();

            if (!(deviceResponse is null)) // Avoid processing a null return value because the foreach code will fail 
            {
                foreach (DriveRates rate in deviceResponse)
                {
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Rate = {0}", rate.ToString()));
                    rates.Add(rate);
                }
            }

            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Number of rates: {0}", rates.Count));
            responseClass.Value = rates;
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);

        }

        private void ReturnEquatorialSystem(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);

        }

        private void ReturnAlignmentMode(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnTrackingRate(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }
        private void WriteTrackingRate(RequestData requestData)
        {
            DriveRates driveRateValue; ;
            Exception exReturn = null;

            try
            {
                driveRateValue = (DriveRates)GetParameter<int>(requestData, requestData.Elements[URL_ELEMENT_METHOD]);
                device.TrackingRate = driveRateValue;
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            SendEmptyResponseToClient(requestData, exReturn);
        }

        private void ReturnSideofPier(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnCameraStates(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnShutterStatus(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnSensorType(RequestData requestData)
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

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnCalibratorState(RequestData requestData)
        {
            CalibratorStatus deviceResponse = CalibratorStatus.Unknown;
            Exception exReturn = null;

            try
            {
                deviceResponse = (CalibratorStatus)device.CalibratorState;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);

        }

        private void ReturnCoverState(RequestData requestData)
        {
            CoverStatus deviceResponse = CoverStatus.Unknown;
            Exception exReturn = null;

            try
            {
                deviceResponse = (CoverStatus)device.CoverState;
            }
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);

        }

        /// <summary>
        /// Return Base64 serialised image data to the client
        /// </summary>
        /// <param name="requestData"></param>
        /// <remarks>
        /// The last provided image data that was saved by the imagearray and imagearrayvariant method calls is binary serialised then compressed and finally returned to the client
        /// </remarks>
        private void ReturnImageArrayBase64(RequestData requestData)
        {
            Array imageArray = null;
            Stopwatch sw = new Stopwatch();
            long lastTime;
            byte[] imageArrayBytes;
            int imageArrayElementSize = 0;
            string imageArrayElementType;
            int imageArrayBytesLength = 0;
            int base64StringLength = 0;

            SharedConstants.ImageArrayCompression compressionType = SharedConstants.ImageArrayCompression.None; // Flag to indicate what type of compression the client supports - initialised to indicate a default of no compression

            sw.Start();

            // Determine whether the client supports compressed responses by testing the Accept-Encoding header, if present. GZip compression will be favoured over Deflate if the client accepts both methods
            string[] acceptEncoding = requestData.Request.Headers.GetValues("Accept-Encoding"); // Get the Accept-Encoding header, if present
            if (acceptEncoding != null) // There is an Accept-Encoding header so check whether it has the compression modes that we support
            {
                if (acceptEncoding[0].ToLowerInvariant().Contains("deflate")) compressionType = SharedConstants.ImageArrayCompression.Deflate; // Test
                if (acceptEncoding[0].ToLowerInvariant().Contains("gzip")) compressionType = SharedConstants.ImageArrayCompression.GZip;
            }
            if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Response compression type: {compressionType}");

            switch (requestData.Elements[URL_ELEMENT_METHOD])
            {
                case "imagearraybase64":
                    imageArray = (Array)ActiveObjects[requestData.DeviceKey].LastImageArray;
                    break;
            }
            long timeAssignImage = sw.ElapsedMilliseconds; lastTime = sw.ElapsedMilliseconds; // Record the duration

            if (imageArray != null)
            {
                imageArrayElementSize = GetManagedSize(imageArray.GetType().GetElementType()); // Find the size of each array element from the array element type
                imageArrayBytes = new byte[imageArray.Length * imageArrayElementSize]; // Size the byte array as the product of the element size and the number of elements
                imageArrayElementType = imageArray.GetType().GetElementType().Name;
                imageArrayBytesLength = imageArrayBytes.Length;
            }
            else
            {
                imageArrayBytes = new byte[0];
                imageArrayElementType = "No array elements";
            }
            long timeBCreateByteArray = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

            if (imageArray != null) Buffer.BlockCopy(imageArray, 0, imageArrayBytes, 0, imageArrayBytes.Length);
            long timeBlockCopy = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

            // Release image array memory
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            imageArray = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            ActiveObjects[requestData.DeviceKey].LastImageArray = null;
            GC.Collect();

            string base64String = Convert.ToBase64String(imageArrayBytes, 0, imageArrayBytes.Length, Base64FormattingOptions.None);
            base64StringLength = base64String.Length;
            long timeToConvertToBase64 = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

            // Release image array bytes memory
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            imageArrayBytes = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            double duration = ForceGarbageCollection();
            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Garbage collection time: {duration}ms.");

            byte[] bytesToSend = Encoding.ASCII.GetBytes(base64String); // Convert the message to be returned into UTF8 bytes that can be sent over the wire
            long timeToConvertBase64StringToByteArray = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration
            int numberOfUncompressedBytes = bytesToSend.Length;

            // Release base 64 string memory
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            base64String = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            GC.Collect();

            if ((compressionType == SharedConstants.ImageArrayCompression.GZip) || (compressionType == SharedConstants.ImageArrayCompression.GZipOrDeflate))
            {
                using (var compressedDataStream = new MemoryStream()) // Create a memory stream
                {
                    using (var gZipStream = new GZipStream(compressedDataStream, CompressionMode.Compress, true)) // Wrap the compressed data stream in a GZip stream
                    {
                        gZipStream.Write(bytesToSend, 0, bytesToSend.Length); // Write the JSON byte array to the GZip stream and hence to the compressed data stream
                    }
                    requestData.Response.AddHeader("Content-Encoding", "gzip");
                    bytesToSend = compressedDataStream.ToArray(); // Get the compressed bytes from the stream into a byte array
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Number of uncompressed bytes: {numberOfUncompressedBytes}, Number of compressed bytes: {bytesToSend.Length:n0}bytes.");
                }
            }
            long timeToCompressResponse = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

            // Set chunked transfer mode if the requested protocol is HTTP/1.1 or later
            try
            {
                if (requestData.Response.ProtocolVersion.CompareTo(new Version(1, 1)) >= 0) // Protocol is HTTP/1.1 or later
                {
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"### Protocol is HTTP/1.1 or later - Setting response.SendChuncked = true");
                    requestData.Response.SendChunked = true;
                }
                else // Protocol is HTTP/1.0
                {
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"### Protocol is HTTP/1.0 - NOT setting chuncked transfer mode");
                }
            }
            catch (Exception ex)
            {
                // Something went wrong when evaluating the HTTP protocol version or when setting chunked transfer mode. This is not considered fatal here because
                // we are able to use the request object "as-is" without changing it. If there is a significant issue, the request will fail later in the process and will be dealt with then.
                LogException1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Exception while evaluating HTTP protocol version or setting chunked mode\r\n{ex}");
            }

            requestData.Response.AddHeader(SharedConstants.BASE64_HANDOFF_HEADER, SharedConstants.BASE64_HANDOFF_SUPPORTED); // Add a header indicating that the content is base64 serialised 
            requestData.Response.ContentType = "image/tiff"; // Must use image/tiff to ensure fast data transmission. All other content types are slower e.g. text/plain takes 8 seconds while image/tiff takes 1 second.
            requestData.Response.ContentLength64 = bytesToSend.Length;
            requestData.Response.OutputStream.Write(bytesToSend, 0, bytesToSend.Length);
            requestData.Response.OutputStream.Close();
            long timeReturnDataToClient = sw.ElapsedMilliseconds - lastTime; // Record the duration

            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"### Base64 response sent to client. " +
                $"Image array element type: {imageArrayElementType}, " +
                $"Image array element size: {imageArrayElementSize:n0}, " +
                $"Image array bytes length: {imageArrayBytesLength:n0}, " +
                $"Base64 string length: {base64StringLength:n0}, " +
                $"Base64 bytes to send: {bytesToSend.Length:n0} bytes, " +
                $"Timings - Overall: {sw.ElapsedMilliseconds}, Time to assign image pointer: {timeAssignImage}ms, Create byte array: {timeBCreateByteArray}ms, Copy image to byte array: {timeBlockCopy}ms, " +
                $"Convert to base64: {timeToConvertToBase64}ms, Convert base64string to byte array: {timeToConvertBase64StringToByteArray}ms. " +
                $"Time to compress base64 string: {timeToCompressResponse}ms, " +
                $"Return data to client: {timeReturnDataToClient}ms.");

            // Release memory so that it can be collected
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            bytesToSend = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            GC.Collect();
        }

        /// <summary>
        /// Return ImageArray and ImageArrayVariant data
        /// </summary>
        /// <param name="requestData"></param>
        /// <remarks>
        /// The default transfer mode is by encoding of the image data as a JSON number array. This is very slow for imagearrayvariant data because the array needs to be converted from object [,] or object [,,] to a short, int or double array type
        /// in order for JSON serialisation to create a number array rather than an object containing number values. To achieve this every element must be read individually and converted, which is very time consuming on large arrays.
        ///
        /// Two optimisations are available if the client supports them:
        /// 1) COMPRESSION
        /// The JSON string can be compressed with either the GZip or Deflate algorithms. The client indicates support by setting the request "Accept-Encoding" header to "gzip" or "deflate". If both are set, GZip will be used.
        /// 2) .NET BINARY FORMATTINF and COMPRESSION
        /// The image array can be serialised using .NET binary serialisation. The client indicates support for this by setting the request "BinarySerialisation" header to "true". When set, the imagearray/imagearrayvariant call returns
        /// with all elements set, apart from the image array data "Value" parameter. This makes the response very quick and, internally, the remote server retains a pointer to the image data returned by the driver. The client then makes an HTTP GET 
        /// call to the remote server to the relevant reserved method imagearraybinary or imagearrayvariantbinary. These calls return the image data serialised with the .net System.Runtime.Serialization.Formatters.Binary.BinaryFormatter 
        /// and then compressed with Gzip to reduce transmission size. The BinaryFormatter format is internal to .NET and is not published, but is compatible between different releases of .NET so this approach is only of value when 
        /// the client is written in .NET
        /// </remarks>
        private void ReturnImageArray(RequestData requestData)
        {
            Array deviceResponse;
            dynamic responseClass = new IntArray2DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID); // Initialise here so that there is a class ready to convey back an error message
            Exception exReturn = null;
            long lastTime = 0;

            // Release memory used by the previous image before acquiring the next
            ActiveObjects[requestData.DeviceKey].LastImageArray = null;
            GC.Collect();

            // These flags indicate whether the client supports optimised, faster transfer modes for camera image data
            SharedConstants.ImageArrayCompression compressionType = SharedConstants.ImageArrayCompression.None; // Flag to indicate what type of compression the client supports - initialised to indicate a default of no compression
            bool base64HandoffRequested = false; // Flag to indicate whether the client supports base64 serialisation
            bool imageBytesRequested = false; // Flag to indicate whether the client requests that the image be transferred as a byte array

            Stopwatch sw = new Stopwatch(); // Create a stopwatch to time the process

            // Determine whether the client supports compressed responses by testing the Accept-Encoding header, if present. GZip compression will be favoured over Deflate if the client accepts both methods
            string[] acceptEncoding = requestData.Request.Headers.GetValues("Accept-Encoding"); // Get the Accept-Encoding header, if present
            if (acceptEncoding != null) // There is an Accept-Encoding header so check whether it has the compression modes that we support
            {
                if (acceptEncoding[0].ToLowerInvariant().Contains("deflate")) compressionType = SharedConstants.ImageArrayCompression.Deflate; // Test
                if (acceptEncoding[0].ToLowerInvariant().Contains("gzip")) compressionType = SharedConstants.ImageArrayCompression.GZip;
            }
            if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Response compression type: {compressionType}");

            try
            {
                // Determine whether the client supports base64 hand-off transfer, if so, this will be used
                if (requestData.Request.Headers[SharedConstants.BASE64_HANDOFF_HEADER] == SharedConstants.BASE64_HANDOFF_SUPPORTED) // Client supports base64 hand-off
                {
                    base64HandoffRequested = true;
                    requestData.Response.AddHeader(SharedConstants.BASE64_HANDOFF_HEADER, SharedConstants.BASE64_HANDOFF_SUPPORTED); // Add a header indicating to the client that a binary formatted image is available for faster processing
                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Base64 encoding supported - Header {SharedConstants.BASE64_HANDOFF_SUPPORTED} = {requestData.Request.Headers[SharedConstants.BASE64_HANDOFF_SUPPORTED]}");
                }
            }
            catch (Exception ex)
            {
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Exception testing for base64 hand-off header:\r\n{ex}");
            }

            try
            {
                // Determine whether the client request that the image be returned as a byte array
                if (requestData.Request.Headers[SharedConstants.ACCEPT_HEADER_NAME].ToLowerInvariant().Contains(SharedConstants.IMAGE_BYTES_MIME_TYPE)) // Client supports image bytes transfer
                {
                    imageBytesRequested = true;
                    if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Image bytes requested - Received header {requestData.Request.Headers[SharedConstants.ACCEPT_HEADER_NAME]}");
                }
            }
            catch (Exception ex)
            {
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Exception testing for image bytes header:\r\n{ex}");
            }

            sw.Start(); // Start the timing stopwatch
            try
            {
                switch (requestData.Elements[URL_ELEMENT_METHOD])
                {
                    case "imagearray":
                        // Get the image array from the device
                        deviceResponse = device.ImageArray;

                        if (deviceResponse != null)
                        {
                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("ImageArray Rank: {0}, Length: {1:n0}", deviceResponse.Rank, deviceResponse.Length));

                            switch (deviceResponse.Rank)
                            {
                                case 2:
                                    // Prefer the ImageBytes mechanic if requested because it is fastest
                                    if (imageBytesRequested)
                                    {
                                        ReturnImageBytes(requestData, ref deviceResponse);
                                        return;
                                    }

                                    // Fall back to base64-handoff if requested
                                    else if (base64HandoffRequested)
                                    {
                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                        responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                        {
                                            ClientTransactionID = requestData.ClientTransactionID,
                                            ServerTransactionID = requestData.ServerTransactionID,
                                            Rank = deviceResponse.Rank,
                                            Type = (int)ImageArrayElementTypes.Int32,
                                            Dimension0Length = deviceResponse.GetLength(0)
                                        };
                                        if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                        if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                    }

                                    // Finally fall back to JSON encoding of the array elements
                                    else
                                    {
                                        responseClass = new IntArray2DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID);
                                        responseClass.Value = (int[,])deviceResponse;
                                    }
                                    break;

                                case 3:
                                    // Prefer the ImageBytes mechanic if requested because it is fastest
                                    if (imageBytesRequested)
                                    {
                                        ReturnImageBytes(requestData, ref deviceResponse);
                                        return;
                                    }

                                    // Fall back to base64-handoff if requested
                                    else if (base64HandoffRequested)
                                    {
                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                        responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                        {
                                            ClientTransactionID = requestData.ClientTransactionID,
                                            ServerTransactionID = requestData.ServerTransactionID,
                                            Rank = deviceResponse.Rank,
                                            Type = (int)ImageArrayElementTypes.Int32,
                                            Dimension0Length = deviceResponse.GetLength(0)
                                        };
                                        if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                        if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                    }

                                    // Finally fall back to JSON encoding of the array elements
                                    else
                                    {
                                        responseClass = new IntArray3DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID);
                                        responseClass.Value = (int[,,])deviceResponse;
                                    }
                                    break;

                                default:
                                    throw new InvalidParameterException("ReturnImageArray received array of Rank " + deviceResponse.Rank + ", this is not currently supported.");
                            }
                        }
                        ActiveObjects[requestData.DeviceKey].LastImageArray = deviceResponse;
                        break;

                    case "imagearrayvariant":
                        deviceResponse = device.ImageArrayVariant;
                        string arrayType = deviceResponse.GetType().Name;
                        string elementType = "";
                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Received array of Rank {0} of Length {1:n0} and type {2}", deviceResponse.Rank, deviceResponse.Length, deviceResponse.GetType().Name));

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
                        LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Array elements are of type: {0}", elementType));

                        switch (deviceResponse.Rank)
                        {
                            case 2:
                                switch (elementType)
                                {
                                    case "Int16":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new ShortArray2DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, Library.Array2DToShort(deviceResponse));
                                        }
                                        break;

                                    case "Int32":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new IntArray2DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID);
                                            responseClass.Value = Library.Array2DToInt(deviceResponse);
                                        }
                                        break;

                                    case "Double":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new DoubleArray2DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, Library.Array2DToDouble(deviceResponse));
                                        }
                                        break;

                                    default:
                                        throw new InvalidValueException("ReturnImageArray: Received an unsupported return array type: " + arrayType + ", with elements of type: " + elementType);
                                }
                                break;
                            case 3:
                                switch (elementType)
                                {
                                    case "Int16":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new ShortArray3DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, Library.Array3DToShort(deviceResponse));
                                        }
                                        break;

                                    case "Int32":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new IntArray3DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID);
                                            responseClass.Value = Library.Array3DToInt(deviceResponse);
                                        }
                                        break;

                                    case "Double":
                                        // Prefer the ImageBytes mechanic if requested because it is fastest
                                        if (imageBytesRequested)
                                        {
                                            ReturnImageBytes(requestData, ref deviceResponse);
                                            return;
                                        }

                                        // Fall back to base64-handoff if requested
                                        else if (base64HandoffRequested)
                                        {
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Preparing base64 hand off response"));
                                            responseClass = new Base64ArrayHandOffResponse() // Create a populated response class with array dimensions but that doesn't have a "Value" member
                                            {
                                                ClientTransactionID = requestData.ClientTransactionID,
                                                ServerTransactionID = requestData.ServerTransactionID,
                                                Rank = deviceResponse.Rank,
                                                Type = (int)ImageArrayElementTypes.Int32,
                                                Dimension0Length = deviceResponse.GetLength(0)
                                            };
                                            if (responseClass.Rank > 1) responseClass.Dimension1Length = deviceResponse.GetLength(1); // Set higher array dimensions if present
                                            if (responseClass.Rank > 2) responseClass.Dimension2Length = deviceResponse.GetLength(2);
                                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Base64Encoded - Completed base64 hand off response"));
                                        }

                                        // Finally fall back to JSON encoding of the array elements
                                        else
                                        {
                                            responseClass = new DoubleArray3DResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, Library.Array3DToDouble(deviceResponse));
                                        }
                                        break;

                                    default:
                                        throw new InvalidValueException("ReturnImageArray: Received an unsupported return array type: " + arrayType + ", with elements of type: " + elementType);
                                }
                                break;
                            default:
                                throw new InvalidParameterException("Received array of Rank " + deviceResponse.Rank + ", this is not currently supported.");
                        }
                        //ActiveObjects[requestData.DeviceKey].LastImageArrayVariant = deviceResponse;
                        break;

                    default:
                        LogMessage1(requestData, "ReturnImageArray", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("ReturnImageArray - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            catch (Exception ex)
            {
                LogException(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, "ReturnImageArray Exception", ex.ToString());
                exReturn = ex;
            }

            responseClass.DriverException = exReturn;
            responseClass.SerializeDriverException = IncludeDriverExceptionInJsonResponse;

            long timeDriver = sw.ElapsedMilliseconds;
            try
            {
                requestData.Response.ContentType = "application/json; charset=utf-8";
                requestData.Response.StatusCode = (int)HttpStatusCode.OK; // Set the response status and status code
                requestData.Response.StatusDescription = "200 OK";

                string responseJson;
                byte[] jsonBytes;
                byte[] compressedBytes;
                long timeCreateCompressedStream;
                long timeCreateCompressedByteArray;
                long timeReturnDataToClient;

                if (base64HandoffRequested)  // Client supports base64 encoding
                {
                    // Write the response back to the client using a stream
                    JsonSerializer serializer = new JsonSerializer();
                    StreamWriter streamWriter = new StreamWriter(requestData.Response.OutputStream);

                    using (JsonWriter writer = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(writer, responseClass);
                    }

                    requestData.Response.OutputStream.Close();
                    sw.Stop();

                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Base64Encoded - Image array properties response sent to client - ImageArray Rank: {responseClass.Rank} Type: {responseClass.Type} - Driver response time: {timeDriver}ms, Overall response time: {sw.ElapsedMilliseconds}ms.");
                }
                else
                {
                    switch (compressionType)
                    {
                        case SharedConstants.ImageArrayCompression.GZip:
                        case SharedConstants.ImageArrayCompression.Deflate:
                            responseJson = JsonConvert.SerializeObject(responseClass);
                            long timeJsonSerialisation = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

                            jsonBytes = Encoding.UTF8.GetBytes(responseJson);
                            long timeJsonBytes = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

                            using (var compressedDataStream = new MemoryStream()) // Create a memory stream
                            {
                                if (compressionType == SharedConstants.ImageArrayCompression.GZip) // Compress using the GZip algorithm
                                {
                                    //using (var gZipStream = new GZipStream(compressedDataStream, CompressionMode.Compress, true)) // Wrap the compressed data stream in a GZip stream
                                    using (var gZipStream = new GZipStream(compressedDataStream, CompressionLevel.Fastest, true)) // Wrap the compressed data stream in a GZip stream
                                    {
                                        gZipStream.Write(jsonBytes, 0, jsonBytes.Length); // Write the JSON byte array to the GZip stream and hence to the compressed data stream
                                    }
                                    requestData.Response.AddHeader("Content-Encoding", "gzip");
                                }
                                else // Compress using the Deflate algorithm
                                {
                                    using (var deflateStream = new DeflateStream(compressedDataStream, CompressionMode.Compress, true)) // Wrap the compressed data stream in a Deflate stream
                                    {
                                        deflateStream.Write(jsonBytes, 0, jsonBytes.Length); // Write the JSON byte array to the Deflate stream and hence to the compressed data stream
                                    }
                                    requestData.Response.AddHeader("Content-Encoding", "deflate");
                                }
                                timeCreateCompressedStream = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration
                                compressedBytes = compressedDataStream.ToArray(); // Get the compressed bytes from the stream into a byte array
                                timeCreateCompressedByteArray = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration
                            }

                            // Write the compressed bytes to the response output stream and close the stream
                            requestData.Response.ContentLength64 = compressedBytes.Length;
                            requestData.Response.OutputStream.Write(compressedBytes, 0, compressedBytes.Length);
                            requestData.Response.OutputStream.Close();
                            timeReturnDataToClient = sw.ElapsedMilliseconds - lastTime; lastTime = sw.ElapsedMilliseconds; // Record the duration

                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Response sent to client with compression type: {compressionType}. " +
                                $"Driver image size: {responseJson.Length:n0}bytes, compressed: {compressedBytes.Length:n0}. " +
                                $"Timings - Overall: {sw.ElapsedMilliseconds}, Driver: {timeDriver}, JSON serialisation: {timeJsonSerialisation}, Convert to JSON bytes: {timeJsonBytes}, " +
                                $"Create compressed stream: {timeCreateCompressedStream} + Convert stream to array: {timeCreateCompressedByteArray}, Return data to client: {timeReturnDataToClient}");
                            break;

                        case SharedConstants.ImageArrayCompression.None:
                            // Write the array back to the client using a stream to avoid running out of memory when serialising very large image arrays
                            JsonSerializer serializer1 = new JsonSerializer();

                            using (StreamWriter streamWriter1 = new StreamWriter(requestData.Response.OutputStream))
                            {
                                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"No compression - ReturnImageArray Before writing bytes to output stream ({sw.ElapsedMilliseconds}ms)");

                                using (JsonWriter writer = new JsonTextWriter(streamWriter1))
                                {
                                    serializer1.Serialize(writer, responseClass);
                                }
                            }
                            serializer1 = null;

                            if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"No compression - ReturnImageArray After writing bytes to output stream ({sw.ElapsedMilliseconds}ms)");
                            requestData.Response.OutputStream.Close();
                            sw.Stop();

                            LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"No compression - ReturnImageArray After closing output stream ({sw.ElapsedMilliseconds}ms.)");
                            break;
                    }
                }
                sw = null;
            }
            catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
            {
                LogException1(requestData, "ListenerException", string.Format("ReturnImageArray Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
            }

            // Release memory so that it can be cleaned up
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            deviceResponse = null;
            responseClass = null;
#pragma warning restore IDE0059 // Unnecessary assignment of a value

            GC.Collect();
        }

        private static void ReturnImageBytes(RequestData requestData, ref Array deviceResponse)
        {
            Stopwatch sw = new Stopwatch();
            Stopwatch swOverall = new Stopwatch();

            byte[] imageArrayBytes;

            swOverall.Start();
            sw.Start();

            try
            {
                // Convert the supplied array to a byte[]
                imageArrayBytes = deviceResponse.ToByteArray(1, requestData.ClientTransactionID, requestData.ServerTransactionID, 0, "");
                deviceResponse = null;

                // Force a garbage collection of large objects to free memory
                ForceGarbageCollection();

                ArrayMetadataV1 arrayMetadataV1 = imageArrayBytes.GetMetadataV1();
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"Converted byte array - Netadata version: {arrayMetadataV1.MetadataVersion}, " +
                    $"Error number: {arrayMetadataV1.ErrorNumber}, " +
                    $"Client transaction ID: {arrayMetadataV1.ClientTransactionID}, " +
                    $"Server transaction ID: {arrayMetadataV1.ServerTransactionID}, " +
                    $"Image element type: {arrayMetadataV1.ImageElementType}, " +
                    $"Transmission element type: {arrayMetadataV1.TransmissionElementType}, " +
                    $"Rank: {arrayMetadataV1.Rank}, " +
                    $"Dimension 1: {arrayMetadataV1.Dimension1}, " +
                    $"Dimension 2: {arrayMetadataV1.Dimension2}, " +
                    $"Dimension 3: {arrayMetadataV1.Dimension3}");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"OVERALL IMAGEARRAYBYTES PROCESSING TIME: {sw.ElapsedMilliseconds}ms for client transaction ID: {requestData.ClientIpAddress}.");
                sw.Restart();

            }
            catch (Exception ex)
            {
                // Set the error number to a non-zero value and add the error message as the data payload.
                imageArrayBytes = AlpacaTools.ErrorMessageToByteArray(1, requestData.ClientTransactionID, requestData.ServerTransactionID, ExceptionHelpers.ErrorCodeFromException(ex), ex.Message);
                LogException1(requestData, "Exception creating image byte array", ex.ToString());
            }

            try
            {
                requestData.Response.ContentType = SharedConstants.IMAGE_BYTES_MIME_TYPE;
                requestData.Response.StatusCode = (int)HttpStatusCode.OK; // Set the response status and status code
                requestData.Response.StatusDescription = $"OK";

                // Condition requestData.Element so that the logging lines below will work correctly
                if (requestData.Elements == null) requestData.Elements = new string[5] { "", "", "", "", "SendResponseToClient" };
                if (requestData.Elements.Length < 5)
                {
                    string[] elements = requestData.Elements;
                    Array.Resize<string>(ref elements, 5);
                    elements[3] = "";
                    elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_SERVER_COMMAND];
                    requestData.Elements = elements;
                }

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before setting response bytes - Length: {0:n0}, Response is null: {1}", imageArrayBytes.Length, requestData.Response == null));
                requestData.Response.ContentLength64 = imageArrayBytes.Length;

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before writing {0:n0} bytes to output stream", requestData.Response.ContentLength64));
                requestData.Response.OutputStream.Write(imageArrayBytes, 0, imageArrayBytes.Length);

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After writing bytes to output stream");
                requestData.Response.OutputStream.Close();
                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After closing output stream");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"IMAGEARRAYBYTES TRANSMISSION TIME: {sw.ElapsedMilliseconds}ms.");
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], $"IMAGEARRAYBYTES OVERALL TIME: {swOverall.ElapsedMilliseconds}ms.");

            }
            catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
            {
                LogException1(requestData, "ListenerException", string.Format("Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
            }

            // Force release of the large byte array
            imageArrayBytes = null;

            // Force a garbage collection of large objects to free memory
            ForceGarbageCollection();
            return;
        }

        private void ReturnCanMoveAxis(RequestData requestData)
        {
            bool deviceResponse = false;
            Exception exReturn = null;

            try
            {
                TelescopeAxes axis = (TelescopeAxes)GetParameter<int>(requestData, SharedConstants.AXIS_PARAMETER_NAME);
                deviceResponse = device.CanMoveAxis(axis);
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }
            BoolResponse responseClass = new BoolResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnDestinationSideOfPier(RequestData requestData)
        {
            PierSide deviceResponse = PierSide.pierUnknown;
            Exception exReturn = null;

            try
            {
                double ra = GetParameter<double>(requestData, SharedConstants.RA_PARAMETER_NAME);
                double dec = GetParameter<double>(requestData, SharedConstants.DEC_PARAMETER_NAME);
                deviceResponse = (PierSide)device.DestinationSideOfPier(ra, dec);
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            IntResponse responseClass = new IntResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, (int)deviceResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);

            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void ReturnAxisRates(RequestData requestData)
        {
            dynamic deviceResponse = null;
            Exception exReturn = null;

            try
            {
                TelescopeAxes axis = (TelescopeAxes)GetParameter<int>(requestData, SharedConstants.AXIS_PARAMETER_NAME);
                deviceResponse = device.AxisRates(axis);
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            // Initialise a list to hold returned rate values and add any rate values that have been returned
            List<RateResponse> rateResponse = new List<RateResponse>();

            if (!(deviceResponse is null)) // Avoid processing a null return value because the foreach code will fail 
            {
                foreach (dynamic r in deviceResponse)
                {
                    rateResponse.Add(new RateResponse(r.Minimum, r.Maximum));
                }
            }

            AxisRatesResponse responseClass = new AxisRatesResponse(requestData.ClientTransactionID, requestData.ServerTransactionID, rateResponse)
            {
                DriverException = exReturn,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);

            SendResponseValueToClient(requestData, exReturn, responseJson);
        }

        private void CallMethod(string deviceType, RequestData requestData)
        {
            double ra, dec, az, alt, duration, switchValue;
            string command;
            bool raw, light, switchState;
            string switchName;
            PierSide pierSideValue;
            short switchIndex;
            int positionInt, guideDuration, brightness;
            float positionFloat;
            GuideDirections guideDirection;
            TelescopeAxes axis;

            Exception exReturn = null;

            try
            {
                switch (deviceType + "." + requestData.Elements[URL_ELEMENT_METHOD])
                {
                    // COMMON METHODS
                    case "*.commandblind":
                        command = GetParameter<string>(requestData, SharedConstants.COMMAND_PARAMETER_NAME);
                        raw = GetParameter<bool>(requestData, SharedConstants.RAW_PARAMETER_NAME);
                        device.CommandBlind(command, raw);
                        break;

                    //COVERCALIBRATOR
                    case "covercalibrator.calibratoroff":
                        device.CalibratorOff(); break;
                    case "covercalibrator.calibratoron":
                        brightness = GetParameter<int>(requestData, SharedConstants.BRIGHTNESS_PARAMETER_NAME);
                        device.CalibratorOn(brightness);
                        break;
                    case "covercalibrator.closecover":
                        device.CloseCover(); break;
                    case "covercalibrator.haltcover":
                        device.HaltCover(); break;
                    case "covercalibrator.opencover":
                        device.OpenCover(); break;

                    //TELESCOPE
                    case "telescope.sideofpier":
                        pierSideValue = (PierSide)GetParameter<int>(requestData, SharedConstants.SIDEOFPIER_PARAMETER_NAME);
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
                        ra = GetParameter<double>(requestData, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(requestData, SharedConstants.DEC_PARAMETER_NAME);
                        device.SlewToCoordinates(ra, dec);
                        break;
                    case "telescope.slewtocoordinatesasync":
                        ra = GetParameter<double>(requestData, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(requestData, SharedConstants.DEC_PARAMETER_NAME);
                        device.SlewToCoordinatesAsync(ra, dec);
                        break;
                    case "telescope.slewtoaltaz":
                        az = GetParameter<double>(requestData, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(requestData, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltAz(az, alt);
                        break;
                    case "telescope.slewtoaltazasync":
                        az = GetParameter<double>(requestData, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(requestData, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltAzAsync(az, alt);
                        break;
                    case "telescope.synctoaltaz":
                        az = GetParameter<double>(requestData, SharedConstants.AZ_PARAMETER_NAME);
                        alt = GetParameter<double>(requestData, SharedConstants.ALT_PARAMETER_NAME);
                        device.SyncToAltAz(az, alt);
                        break;
                    case "telescope.synctocoordinates":
                        ra = GetParameter<double>(requestData, SharedConstants.RA_PARAMETER_NAME);
                        dec = GetParameter<double>(requestData, SharedConstants.DEC_PARAMETER_NAME);
                        device.SyncToCoordinates(ra, dec);
                        break;
                    case "telescope.moveaxis":
                        axis = (TelescopeAxes)GetParameter<int>(requestData, SharedConstants.AXIS_PARAMETER_NAME);
                        double rate = GetParameter<double>(requestData, SharedConstants.RATE_PARAMETER_NAME);
                        device.MoveAxis(axis, rate);
                        break;
                    case "telescope.pulseguide":
                        guideDirection = (GuideDirections)GetParameter<int>(requestData, SharedConstants.DIRECTION_PARAMETER_NAME);
                        guideDuration = GetParameter<int>(requestData, SharedConstants.DURATION_PARAMETER_NAME);
                        device.PulseGuide(guideDirection, guideDuration);
                        break;

                    // FOCUSER
                    case "focuser.halt":
                        device.Halt(); break;
                    case "focuser.move":
                        positionInt = GetParameter<int>(requestData, SharedConstants.POSITION_PARAMETER_NAME);
                        device.Move(positionInt);
                        break;

                    //CAMERA
                    case "camera.abortexposure":
                        device.AbortExposure(); break;
                    case "camera.pulseguide":
                        guideDirection = (GuideDirections)GetParameter<int>(requestData, SharedConstants.DIRECTION_PARAMETER_NAME);
                        guideDuration = GetParameter<int>(requestData, SharedConstants.DURATION_PARAMETER_NAME);
                        device.PulseGuide(guideDirection, guideDuration);
                        break;
                    case "camera.startexposure":
                        duration = GetParameter<double>(requestData, SharedConstants.DURATION_PARAMETER_NAME);
                        light = GetParameter<bool>(requestData, SharedConstants.LIGHT_PARAMETER_NAME);
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
                        alt = GetParameter<double>(requestData, SharedConstants.ALT_PARAMETER_NAME);
                        device.SlewToAltitude(alt);
                        break;
                    case "dome.slewtoazimuth":
                        az = GetParameter<double>(requestData, SharedConstants.AZ_PARAMETER_NAME);
                        device.SlewToAzimuth(az);
                        break;
                    case "dome.synctoazimuth":
                        az = GetParameter<double>(requestData, SharedConstants.AZ_PARAMETER_NAME);
                        device.SyncToAzimuth(az);
                        break;

                    // OBSERVINGCONDITIONS
                    case "observingconditions.refresh":
                        device.Refresh(); break;

                    // SWITCH
                    case "switch.setswitchname":
                        switchIndex = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);
                        switchName = GetParameter<string>(requestData, SharedConstants.NAME_PARAMETER_NAME);
                        device.SetSwitchName(switchIndex, switchName);
                        break;
                    case "switch.setswitch":
                        switchIndex = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);
                        switchState = GetParameter<bool>(requestData, SharedConstants.STATE_PARAMETER_NAME);
                        device.SetSwitch(switchIndex, switchState);
                        break;
                    case "switch.setswitchvalue":
                        switchIndex = GetParameter<short>(requestData, SharedConstants.ID_PARAMETER_NAME);
                        switchValue = GetParameter<double>(requestData, SharedConstants.VALUE_PARAMETER_NAME);
                        device.SetSwitchValue(switchIndex, switchValue);
                        break;

                    // ROTATOR
                    case "rotator.halt":
                        device.Halt(); break;
                    case "rotator.move":
                        positionFloat = GetParameter<float>(requestData, SharedConstants.POSITION_PARAMETER_NAME);
                        device.Move(positionFloat);
                        break;
                    case "rotator.moveabsolute":
                        positionFloat = GetParameter<float>(requestData, SharedConstants.POSITION_PARAMETER_NAME);
                        device.MoveAbsolute(positionFloat);
                        break;
                    case "rotator.sync":
                        positionFloat = GetParameter<float>(requestData, SharedConstants.POSITION_PARAMETER_NAME);
                        device.Sync(positionFloat);
                        break;
                    case "rotator.movemechanical":
                        positionFloat = GetParameter<float>(requestData, SharedConstants.POSITION_PARAMETER_NAME);
                        device.MoveMechanical(positionFloat);
                        break;

                    default:
                        LogMessage1(requestData, "CallMethod", "Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                        throw new InvalidValueException("CallMethod - Unsupported requestData.Elements[URL_ELEMENT_METHOD]: " + requestData.Elements[URL_ELEMENT_METHOD]);
                }
            }
            // Handle missing or invalid parameters
            catch (InvalidParameterException ex)
            {
                Return400Error(requestData, ex.Message);
                return;
            }
            // Handle device errors
            catch (Exception ex)
            {
                exReturn = ex;
            }

            SendEmptyResponseToClient(requestData, exReturn);
        }

        private T GetParameter<T>(RequestData requestData, string ParameterName)
        {

            // Make sure a valid Parameter name was passed to us
            if (string.IsNullOrEmpty(ParameterName))
            {
                string errorMessage = string.Format("GetParameter - ParameterName is null or empty, when retrieving an {0} value", typeof(T).Name);
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], errorMessage);
                throw new InvalidParameterException(errorMessage);
            }

            // Check whether a string value of any kind was supplied as the parameter value
            string parameterStringValue = requestData.SuppliedParameters[ParameterName];
            if (parameterStringValue == null)
            {
                string errorMessage = string.Format("GetParameter - The mandatory parameter: {0} is missing or has a null value.", ParameterName);
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], errorMessage);
                throw new InvalidParameterException(errorMessage);
            }

            // Handle string values first because they don't need to be converted into another type 
            if (typeof(T) == typeof(string))
            {
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, parameterStringValue));
                return (T)(object)parameterStringValue;
            }
            // Convert the parameter value into the required type
            switch (typeof(T).Name)
            {
                case "Single":
                    float singleValue;
                    if (!float.TryParse(parameterStringValue, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out singleValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to a floating point value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, singleValue.ToString()));
                    return (T)(object)singleValue;

                case "Double":
                    double doubleValue;
                    if (!double.TryParse(parameterStringValue, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out doubleValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to a floating point value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, doubleValue.ToString()));
                    return (T)(object)doubleValue;

                case "Int16":
                    short shortValue;
                    if (!short.TryParse(parameterStringValue, NumberStyles.Integer | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out shortValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to an Int16 value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, shortValue.ToString()));
                    return (T)(object)shortValue;

                case "Int32":
                    int intValue;
                    if (!int.TryParse(parameterStringValue, NumberStyles.Integer | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out intValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to an Int32 value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, intValue.ToString()));
                    return (T)(object)intValue;

                case "Boolean":
                    bool boolValue;
                    if (!bool.TryParse(parameterStringValue, out boolValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to a boolean value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, boolValue.ToString()));
                    return (T)(object)boolValue;

                case "DateTime":
                    DateTime dateTimeValue;
                    if (!DateTime.TryParse(parameterStringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeValue)) throw new InvalidParameterException($"GetParameter - Supplied argument '{parameterStringValue}' for parameter {ParameterName} can not be converted to a DateTime value");
                    LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("{0} = {1}", ParameterName, dateTimeValue.ToString()));
                    return (T)(object)dateTimeValue;

                default:
                    string errorMessage = $"Unsupported type: {typeof(T).Name} called by {requestData.Elements[URL_ELEMENT_METHOD]}";
                    LogMessage1(requestData, "GetParameter", errorMessage);
                    throw new InvalidParameterException(errorMessage);
            }
        }

        private void SendEmptyResponseToClient(RequestData requestData, Exception ex)
        {
            MethodResponse responseClass = new MethodResponse(requestData.ClientTransactionID, requestData.ServerTransactionID)
            {
                DriverException = ex,
                SerializeDriverException = IncludeDriverExceptionInJsonResponse
            };
            string responseJson = JsonConvert.SerializeObject(responseClass);
            SendResponseValueToClient(requestData, ex, responseJson);
        }

        private void SendResponseValueToClient(RequestData requestData, Exception ex, string jsonResponse)
        {
            if (FORCE_JSON_RESPONSE_TO_LOWER_CASE_TESTING_ONLY & jsonResponse.Length < 1000)
            {
                jsonResponse = jsonResponse.ToLowerInvariant();
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "TESTING - JSON RESPONCE FORCED TO LOWER CASE");
            }

            if (ex == null) // Command ran successfully so return the JSON encoded result
            {
                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("OK - no exception. Thread: {0}, Json: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), (jsonResponse.Length < 1000) ? jsonResponse : jsonResponse.Substring(0, 1000)));
                if (ScreenLogResponses) LogToScreen(string.Format("  OK - JSON: {0}", jsonResponse));
                TransmitResponse(requestData, "application/json; charset=utf-8", HttpStatusCode.OK, "200 OK", jsonResponse);
            }
            else // Some sort of exception was thrown during command execution
            {
                if (ex.GetType() == typeof(InvalidParameterException)) // A required parameter is missing or invalid in the supplied http call
                {
                    if (ScreenLogResponses) LogToScreen(string.Format("  PARAMETER ERROR - ClientId: {0}, ClientTxnID: {1}, ServerTxnID: {2} - {3}", requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, ex.Message));
                    TransmitResponse(requestData, "text/plain; charset=utf-8", HttpStatusCode.BadRequest, "400 " + ex.Message, "400 " + ex.Message);
                }
                else
                {
                    if (ScreenLogResponses) LogToScreen(string.Format("  DEVICE ERROR - ClientId: {0}, ClientTxnID: {1}, ServerTxnID: {2} - {3}", requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, ex.Message));
                    TransmitResponse(requestData, "application/json; charset=utf-8", HttpStatusCode.OK, "200 OK", jsonResponse);
                }

                if (DebugTraceState) LogException1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "Exception: " + ex.ToString());
                else LogException1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "Exception: " + ex.Message);

                LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Thread: {0}, Json: {1}", Thread.CurrentThread.ManagedThreadId.ToString(), jsonResponse));
            }
        }

        // Handle the mechanics of putting the response onto the wire back to the client
        void TransmitResponse(RequestData requestData, string contentType, HttpStatusCode httpStatusCode, string statusDescription, string messageToSend)
        {
            byte[] bytesToSend; // Array to hold the encoded message

            try
            {
                requestData.Response.ContentType = contentType;
                requestData.Response.StatusCode = (int)httpStatusCode; // Set the response status and status code
                requestData.Response.StatusDescription = statusDescription;

                // Condition requestData.Element so that the logging lines below will work correctly
                if (requestData.Elements == null) requestData.Elements = new string[5] { "", "", "", "", "SendResponseToClient" };
                if (requestData.Elements.Length < 5)
                {
                    string[] elements = requestData.Elements;
                    Array.Resize<string>(ref elements, 5);
                    elements[3] = "";
                    elements[URL_ELEMENT_METHOD] = elements[URL_ELEMENT_SERVER_COMMAND];
                    requestData.Elements = elements;
                }

                bytesToSend = Encoding.UTF8.GetBytes(messageToSend); // Convert the message to be returned into UTF8 bytes that can be sent over the wire
                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Completed Encoding.GetBytes, array length: {0:n0}", bytesToSend.Length));

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before setting response bytes - Length: {0:n0}, Response is null: {1}", bytesToSend.Length, requestData.Response == null));
                requestData.Response.ContentLength64 = bytesToSend.Length;

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], string.Format("Before writing {0:n0} bytes to output stream", requestData.Response.ContentLength64));
                requestData.Response.OutputStream.Write(bytesToSend, 0, bytesToSend.Length);

                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After writing bytes to output stream");
                requestData.Response.OutputStream.Close();
                if (DebugTraceState) LogMessage1(requestData, requestData.Elements[URL_ELEMENT_METHOD], "After closing output stream");
            }
            catch (HttpListenerException ex) // Deal with communications errors here but allow any other errors to go through and be picked up by the main error handler
            {
                LogException1(requestData, "ListenerException", string.Format("Communications exception - Error code: {0}, Native error code: {1}\r\n{2}", ex.ErrorCode, ex.NativeErrorCode, ex.ToString()));
            }
        }

        #endregion

    } // End of ServerForm class

} // End of namespace