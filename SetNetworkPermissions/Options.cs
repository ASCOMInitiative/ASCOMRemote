using CommandLine;

namespace ASCOM.Remote
{
    /// <summary>
    /// Specify command line options that can be supplied to this executable through the CommandLine reference
    /// </summary>
    class Options
    {
        [Option('l', Program.SET_LOCAL_SERVER_PATH_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the local server executable")]
        public string LocalServerPath { get; set; }

        [Option('a', Program.ENABLE_API_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the API URI ACL")]
        public string SetApiUriAcl { get; set; }

        [Option('m', Program.ENABLE_REMOTE_SERVER_MANAGEMENT_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Remote Server management URI ACL")]
        public string SetRemoteServerManagementUriAcl { get; set; }

        [Option('p', Program.ENABLE_ALPACA_DEVICE_MANAGEMENT_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Alpaca device management URI ACL")]
        public string SetAlpacaManagementUriAcl { get; set; }

        [Option('s', Program.ENABLE_ALPACA_SETUP_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Alpaca HTML setup URI ACL")]
        public string SetAlpacaSetupUriAcl { get; set; }

        [Option('h', Program.ENABLE_HTTP_DOT_SYS_PORT_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Enable the specified firewall port on which HTTP.SYS will listen")]
        public string HttpDotSysPort { get; set; }

        [Option('h', Program.USER_NAME_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the user name for which the HTTP.SYS permission will be enabled")]
        public string UserName { get; set; }
    }
}