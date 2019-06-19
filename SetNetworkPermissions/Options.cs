using CommandLine;

namespace ASCOM.Remote
{
    /// <summary>
    /// Specify command line options that can be supplied to this executable through the CommandLine reference
    /// </summary>
    class Options
    {
        [Option('l', SharedConstants.SET_LOCAL_SERVER_PATH, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the local server executable")]
        public string LocalServerPath { get; set; }
        [Option('a', SharedConstants.ENABLE_API_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the API URI ACL")]
        public string SetApiUriAcl { get; set; }
        [Option('m', SharedConstants.ENABLE_REMOTE_SERVER_MANAGEMENT_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Remote Server management URI ACL")]
        public string SetRemoteServerManagementUriAcl { get; set; }
        [Option('p', SharedConstants.ENABLE_ALPACA_DEVICE_MANAGEMENT_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Alpaca device management URI ACL")]
        public string SetAlpacaManagementUriAcl { get; set; }
        [Option('s', SharedConstants.ENABLE_ALPACA_SETUP_URI_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Set the Alpaca HTML setup URI ACL")]
        public string SetAlpacaSetupUriAcl { get; set; }
        [Option('h', SharedConstants.ENABLE_HTTP_DOT_SYS_PORT_COMMAND_NAME, Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Enable the specified firewall port on which HTTP.SYS will listen")]
        public string HttpDotSysPort { get; set; }
    }
}