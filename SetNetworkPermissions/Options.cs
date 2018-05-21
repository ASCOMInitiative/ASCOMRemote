using CommandLine;

namespace ASCOM.Remote
{
    /// <summary>
    /// Specify command line options that can be supplied to this executable through the CommandLine reference
    /// </summary>
    class Options
    {
        [Option('l', "localserverpath", Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the local server executable")]
        public string LocalServerPath { get; set; }
        [Option('r', "remoteserverpath", Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the remote server executable")]
        public string RemoteServerPath { get; set; }
    }
}
