using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFirewallHelper;
using ASCOM.Utilities;
using System.Security.Principal;
using CommandLine;
using System.IO;

namespace SetNetworkPemissions
{
    static class Program
    {
        const string LOCAL_SERVER_RULE_NAME_BASE = @"ASCOM Local Device Server";
        const string REMOTE_SERVER_RULE_NAME_BASE = @"ASCOM Remote Device Server";
        const string INBOUND_RULE_NAME = @" - Inbound";
        const string OUTBOUND_RULE_NAME = @" - Outbound";
        public const string NOT_PRESENT_FLAG = "***** PARAMETER NOT PRESENT *****";

        static TraceLogger TL;

        static void Main(string[] args)
        {

            TL = new TraceLogger("", "SetFireWallRules");
            TL.Enabled = true;
            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => ProcessOptions(opts))
                .WithNotParsed<Options>((errs) => HandleParseError(errs));

            TL.Enabled = false;
            TL = null;
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error err in errs)
            {
                //Console.WriteLine("Error stops processing: {0}, {1}", err.StopsProcessing,err.ToString());
            }
        }

        private static void ProcessOptions(Options opts)
        {

            Console.WriteLine("Local server path: {0}", opts.LocalServerPath);
            TL.LogMessage("ProcessOptions", string.Format("Local server  path: {0}", opts.LocalServerPath));
            Console.WriteLine("Remote server path: {0}", opts.RemoteServerPath);
            TL.LogMessage("ProcessOptions", string.Format("Remote server  path: {0}", opts.RemoteServerPath));
            TL.BlankLine();

            try // Make sure that we still try and set the firewall rules even if we bomb out trying to get information on the firewall configuration
            {
                TL.LogMessage("QueryFireWall", string.Format("Firewall version: {0}", FirewallManager.Version.ToString())); // Log the firfewall version in use
                foreach (IProfile profile in FirewallManager.Instance.Profiles)
                {
                    TL.LogMessage("QueryFireWall", string.Format("Found current firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                }

                IFirewall[] thirdPartyFirewalls = FirewallManager.ThirdPartyFirewalls;
                TL.LogMessage("QueryFireWall", string.Format("number of third party firewalls: {0}", thirdPartyFirewalls.Length));
                foreach (IFirewall firewall in thirdPartyFirewalls)
                {
                    TL.LogMessage("QueryFireWall", string.Format("Found third party firewall: {0}", firewall.Name));
                    foreach (IProfile profile in firewall.Profiles)
                    {
                        TL.LogMessage("QueryFireWall", string.Format("Found third party firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                    }
                }
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("QueryFireWall", "Exception: " + ex.ToString());
            }
            TL.BlankLine();

            if (opts.LocalServerPath != NOT_PRESENT_FLAG) SetFireWallRules(opts.LocalServerPath, LOCAL_SERVER_RULE_NAME_BASE);
            if (opts.RemoteServerPath != NOT_PRESENT_FLAG) SetFireWallRules(opts.RemoteServerPath, REMOTE_SERVER_RULE_NAME_BASE);
        }

        private static void SetFireWallRules(string applicationPath, string FirewallRuleName)
        {
            try
            {
                if ((new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator)) // Application is being run with Administrator privilege so go ahead and set the firewall rules
                {
                    // Check whether the specified file exists
                    if (File.Exists(applicationPath)) // The file does exist so process it
                    {
                        string applicationPathFull = Path.GetFullPath(applicationPath);
                        TL.LogMessage("SetFireWallRules", string.Format("Supplied path: {0}, full path: {1}", applicationPath, applicationPathFull));

                        IEnumerable<IRule> query = FirewallManager.Instance.Rules.Where(ruleName => ruleName.Name.ToUpperInvariant().StartsWith(FirewallRuleName.ToUpperInvariant()));
                        List<IRule> queryCopy = query.ToList();
                        foreach (IRule existingRule in queryCopy)
                        {
                            TL.LogMessage("SetFireWallRules", string.Format("Found rule: {0}", existingRule.Name));
                            FirewallManager.Instance.Rules.Remove(existingRule); // Delete the rule
                            TL.LogMessage("SetFireWallRules", string.Format("Deleted rule: {0}", existingRule.Name));
                        }

                        IRule rule = FirewallManager.Instance.CreateApplicationRule(FirewallManager.Instance.GetProfile().Type, FirewallRuleName + OUTBOUND_RULE_NAME, FirewallAction.Allow, applicationPathFull);
                        rule.Direction = FirewallDirection.Outbound;
                        rule.Profiles = FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public;
                        TL.LogMessage("SetFireWallRules", "Created outbound rule");
                        FirewallManager.Instance.Rules.Add(rule);
                        TL.LogMessage("SetFireWallRules", string.Format("Added outbound rule for {0}", applicationPathFull));

                        rule = FirewallManager.Instance.CreateApplicationRule(FirewallManager.Instance.GetProfile().Type, FirewallRuleName + INBOUND_RULE_NAME, FirewallAction.Allow, applicationPathFull);
                        rule.Direction = FirewallDirection.Inbound;
                        rule.Profiles = FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public;
                        TL.LogMessage("SetFireWallRules", "Created inbound rule");
                        FirewallManager.Instance.Rules.Add(rule);
                        TL.LogMessage("SetFireWallRules", string.Format("Added inbound rule for {0}", applicationPathFull));
                    }
                    else
                    {
                        TL.LogMessage("SetFireWallRules", string.Format("The specified file does not exist: {0}", applicationPath));
                        Console.WriteLine("The specified file does not exist: {0}", applicationPath);
                    }
                }
                else
                {
                    TL.LogMessage("SetFireWallRules", "Not running as Administrator so unable to set firewall rules.");
                    Console.WriteLine("Not running as Administrator so unable to set firewall rules.");
                }
                TL.BlankLine();
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetFireWallRules", "Exception: " + ex.ToString());
                Console.WriteLine("SetFireWallRules threw an exception: " + ex.Message);
            }

        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            TraceLogger TL = new TraceLogger("SetNetworkPemissionsException")
            {
                Enabled = true
            };

            TL.LogMessageCrLf("Main", "Unhandled exception: " + exception.ToString());
            TL.Enabled = false;
            TL.Dispose();
            TL = null;

            Environment.Exit(0);
        }
    }

    class Options
    {
        [Option('l', Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the local server executable")]
        public string LocalServerPath { get; set; }
        [Option('r', Required = false, Default = Program.NOT_PRESENT_FLAG, HelpText = "Path to the remote server executable")]
        public string RemoteServerPath { get; set; }
    }
}

