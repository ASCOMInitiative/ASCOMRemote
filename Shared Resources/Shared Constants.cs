using Microsoft.Win32;

namespace ASCOM.Remote
{
    public static class SharedConstants
    {
        // HTTP Parameter names shared by driver and remote server
        //public static string HTTP_PARAMETER_NAME = "ParameterValue";
        public static string RA_PARAMETER_NAME = "RightAscension";
        public static string DEC_PARAMETER_NAME = "Declination";
        public static string ALT_PARAMETER_NAME = "Altitude";
        public static string AZ_PARAMETER_NAME = "Azimuth";
        public static string AXIS_PARAMETER_NAME = "Axis";
        public static string RATE_PARAMETER_NAME = "Rate";
        public static string DIRECTION_PARAMETER_NAME = "Direction";
        public static string DURATION_PARAMETER_NAME = "Duration";
        public static string CLIENTID_PARAMETER_NAME = "ClientID";
        public static string CLIENTTRANSACTION_PARAMETER_NAME = "ClientTransactionID";
        public static string COMMAND_PARAMETER_NAME = "Command";
        public static string RAW_PARAMETER_NAME = "Raw";
        public static string LIGHT_PARAMETER_NAME = "Light";
        public static string ACTION_COMMAND_PARAMETER_NAME = "Action";
        public static string ACTION_PARAMETERS_PARAMETER_NAME = "Parameters";
        public static string ID_PARAMETER_NAME = "ID";
        public static string STATE_PARAMETER_NAME = "State";
        public static string NAME_PARAMETER_NAME = "Name";
        public static string VALUE_PARAMETER_NAME = "Value";
        public static string POSITION_PARAMETER_NAME = "Position";
        public static string SIDEOFPIER_PARAMETER_NAME = "SideOfPier";
        public static string UTCDATE_PARAMETER_NAME = "UTCDate";
        public static string SENSORNAME_PARAMETER_NAME = "SensorName";

        public static string ISO8601_DATE_FORMAT_STRING = "yyyy-MM-ddTHH:mm:ss.fffffff";

        public const string LOCALHOST_NAME = "localhost";
        public const string LOCALHOST_ADDRESS = "127.0.0.1"; // Get the localhost loop back address

        // Constants shared by Remote Client Drivers and the ASCOM REST Server
        public const string API_URL_BASE = "/api/"; // This constant must always be lower case to make the logic tests work properly 
        public const string API_VERSION_V1 = "v1"; // This constant must always be lower case to make the logic tests work properly
        public const string MANAGEMENT_URL_BASE = "/server/"; // This constant must always be lower case to make the logic tests work properly 

        // Remote server management API interface constants
        public const string MANGEMENT_PROFILE = "profile";
        public const string MANGEMENT_CONFIGURATION = "configuration";
        public const string MANGEMENT_API_ENABLED = "enabled";
        public const string MANGEMENT_CONCURRENT_CALLS = "concurrency";
        public const string MANGEMENT_RESTART = "restart";

        // Client driver profile persistence constants
        public const string TRACE_LEVEL_PROFILENAME = "Trace Level"; public const string CLIENT_TRACE_LEVEL_DEFAULT = "True";
        public const string DEBUG_TRACE_PROFILENAME = "Include Debug Trace"; public const string DEBUG_TRACE_DEFAULT = "True";
        public const string IPADDRESS_PROFILENAME = "IP Address"; public const string IPADDRESS_DEFAULT = SharedConstants.LOCALHOST_ADDRESS;
        public const string PORTNUMBER_PROFILENAME = "Port Number"; public const string PORTNUMBER_DEFAULT = "11111";
        public const string REMOTE_DEVICE_NUMBER_PROFILENAME = "Remote Device Number"; public const string REMOTE_DEVICE_NUMBER_DEFAULT = "0";
        public const string SERVICE_TYPE_PROFILENAME = "Service Type"; public const string SERVICE_TYPE_DEFAULT = "http";
        public const string ESTABLISH_CONNECTION_TIMEOUT_PROFILENAME = "Establish Connection Timeout"; public const string ESTABLISH_CONNECTION_TIMEOUT_DEFAULT = "10";
        public const string STANDARD_SERVER_RESPONSE_TIMEOUT_PROFILENAME = "Standard Server Response Timeout"; public const string STANDARD_SERVER_RESPONSE_TIMEOUT_DEFAULT = "10";
        public const string LONG_SERVER_RESPONSE_TIMEOUT_PROFILENAME = "Long Server Response Timeout"; public const string LONG_SERVER_RESPONSE_TIMEOUT_DEFAULT = "120";
        public const string USERNAME_PROFILENAME = "User Name"; public const string USERNAME_DEFAULT = "";
        public const string PASSWORD_PROFILENAME = "Password"; public const string PASSWORD_DEFAULT = "";
        public const string MANAGE_CONNECT_LOCALLY_PROFILENAME = "Manage Connect Locally"; public const string MANAGE_CONNECT_LOCALLY_DEFAULT = "False";

        // Driver naming constants
        public const string DRIVER_DISPLAY_NAME = "ASCOM Remote Client";
        public const string DRIVER_PROGID_BASE = "ASCOM.Remote";
        public const string NOT_CONNECTED_MESSAGE = "is not connected."; // This is appended to the driver display name + driver number and displayed when the driver is not connected
        public const string TRACELOGGER_NAME_FORMAT_STRING = "Remote{0}.{1}";

        // Enum to describe Camera.ImageArray and ImageArrayVCariant array types
        public enum ImageArrayElementTypes
        {
            Unknown = 0,
            Short = 1,
            Int = 2,
            Double = 3
        }

        // Registry key where the Web Server configuration will be stored
        public const RegistryHive ASCOM_REMOTE_CONFIGURATION_HIVE = RegistryHive.CurrentUser;
        public const string ASCOM_REMOTE_CONFIGURATION_KEY = @"Software\ASCOM Remote";

        public const string REQUEST_RECEIVED_STRING = "RequestReceived";

        public const string DEVICE_NOT_CONFIGURED = "None"; // ProgID value indicating no device configured

    }
}
