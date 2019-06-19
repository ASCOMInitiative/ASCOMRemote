using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

using CommandLine;
using WindowsFirewallHelper;

using ASCOM.Utilities;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Reflection;

namespace ASCOM.Remote
{
    static class Program
    {
        const string LOCAL_SERVER_RULE_NAME_BASE = @"ASCOM Local Device Server";
        const string REMOTE_SERVER_RULE_NAME_BASE = @"ASCOM Remote Device Server";
        const string INBOUND_RULE_NAME = @" - Inbound";
        const string OUTBOUND_RULE_NAME = @" - Outbound";
        public const string NOT_PRESENT_FLAG = "***** PARAMETER NOT PRESENT *****";

        static TraceLogger TL;

        [HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                TL = new TraceLogger("", "SetNetworkPermissions");
                TL.Enabled = true;

                Version version = Assembly.GetEntryAssembly().GetName().Version;
                TL.LogMessage("SetNetworkPermissions", string.Format("Version {0}, Run on {1}", version.ToString(), DateTime.Now.ToString("dddd d MMMM yyyy HH:mm:ss")));
                TL.BlankLine();

                foreach (string arg in args)
                {
                    TL.LogMessage("SetNetworkPermissions", $"Received argument: {arg}");
                }
                TL.BlankLine();

                CommandLine.Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(opts => ProcessOptions(opts))
                    .WithNotParsed<Options>((errs) => HandleParseError(errs));

                TL.LogMessage("SetNetworkPermissions", "Command line processing completed.");

                TL.Enabled = false;
                TL = null;
            }
            catch (Exception ex)
            {
                TraceLogger TL = new TraceLogger("SetNetworkPermissionsMainException")
                {
                    Enabled = true
                };
                TL.LogMessageCrLf("Main", "Unhandled exception: " + ex.ToString());
                TL.Enabled = false;
                TL.Dispose();
                TL = null;
            }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error err in errs)
            {
                TL.LogMessage("HandleParseError", $"Error stopped processing: {err.StopsProcessing}, {err.ToString()}");
            }
        }

        private static void ProcessOptions(Options opts)
        {

            TL.LogMessage("ProcessOptions", string.Format("Local server path: {0}", opts.LocalServerPath));
            TL.LogMessage("ProcessOptions", string.Format("Remote server path: {0}", opts.RemoteServerPath));
            TL.LogMessage("ProcessOptions", string.Format("API URI: {0}", opts.SetApiUriAcl));
            TL.LogMessage("ProcessOptions", string.Format("Remote server management URI: {0}", opts.SetRemoteServerManagementUriAcl));
            TL.LogMessage("ProcessOptions", string.Format("Alpaca device management URI: {0}", opts.SetAlpacaManagementUriAcl));
            TL.LogMessage("ProcessOptions", string.Format("Alpaca device HTML setup URI: {0}", opts.SetAlpacaSetupUriAcl));
            TL.BlankLine();

            // Set firewall rules depending on which command line parameter was supplied
            if (opts.LocalServerPath != NOT_PRESENT_FLAG) SetFireWallRules(opts.LocalServerPath, LOCAL_SERVER_RULE_NAME_BASE);
            if (opts.RemoteServerPath != NOT_PRESENT_FLAG) SetFireWallRules(opts.RemoteServerPath, REMOTE_SERVER_RULE_NAME_BASE);

            // Set http.sys ACLs as needed
            if (opts.SetApiUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetApiUriAcl);
            if (opts.SetRemoteServerManagementUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetRemoteServerManagementUriAcl);
            if (opts.SetAlpacaManagementUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetAlpacaManagementUriAcl);
            if (opts.SetAlpacaSetupUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetAlpacaSetupUriAcl);
        }

        private static void SetFireWallRules(string applicationPath, string FirewallRuleName)
        {
            try // Make sure that we still try and set the firewall rules even if we bomb out trying to get information on the firewall configuration
            {
                TL.LogMessage("QueryFireWall", string.Format("Firewall version: {0}", FirewallManager.Version.ToString())); // Log the firewall version in use
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
                        TL.LogMessage("SetFireWallRules", "Successfully created outbound rule");
                        FirewallManager.Instance.Rules.Add(rule);
                        TL.LogMessage("SetFireWallRules", string.Format("Successfully added outbound rule for {0}", applicationPathFull));

                        rule = FirewallManager.Instance.CreateApplicationRule(FirewallManager.Instance.GetProfile().Type, FirewallRuleName + INBOUND_RULE_NAME, FirewallAction.Allow, applicationPathFull);
                        rule.Direction = FirewallDirection.Inbound;
                        rule.Profiles = FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public;

                        // Add edge traversal permission to the inbound rule so that the rule will apply to packets delivered in encapsulated transmission formats such as VPNs
                        if (rule is WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRule)
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is a standard rule");
                            ((WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRule)rule).EdgeTraversal = true;
                        }
                        else
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is not a standard rule");
                        }
                        if (rule is WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin7)
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is a WIN7 rule");
                            ((WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin7)rule).EdgeTraversalOptions = WindowsFirewallHelper.FirewallAPIv2.EdgeTraversalAction.Allow;
                        }
                        else
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is not a WIN7 rule");
                        }
                        if (rule is WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin8)
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is a WIN8 rule");
                            ((WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin8)rule).EdgeTraversalOptions = WindowsFirewallHelper.FirewallAPIv2.EdgeTraversalAction.Allow;
                        }
                        else
                        {
                            TL.LogMessage("SetFireWallRules", "Firewall rule is not a WIN8 rule");
                        }

                        TL.LogMessage("SetFireWallRules", "Successfully created inbound firewall rule");

                        FirewallManager.Instance.Rules.Add(rule);
                        TL.LogMessage("SetFireWallRules", string.Format("Successfully added inbound firewall rule for {0}", applicationPathFull));
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

        private static void SetAcl(string Uri)
        {
            try
            {
                string command = string.Format(@"http add urlacl url={0} user={1}\{2}", Uri, Environment.UserDomainName, Environment.UserName);
                TL.LogMessage("SetAcl", "Enable arguments: " + command);

                // Parse out the port number and resource value
                int doubleSlashIndex = Uri.IndexOf("//");
                int colonIndex = Uri.IndexOf(":", doubleSlashIndex + 2);
                string portAndUri = Uri.Substring(colonIndex + 1);

                string strHostName = "";
                strHostName = Dns.GetHostName();
                TL.LogMessage("Network", string.Format("Colon index: {0}, Port and URI: {1}", colonIndex, portAndUri));

                IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);

                foreach (IPAddress curAdd in ipEntry.AddressList)
                {
                    if (curAdd.AddressFamily == AddressFamily.InterNetwork)
                    {
                        TL.LogMessage("Network", "IP V4 Network Address: " + curAdd.ToString());
                        // Remove the URLACL if it exists
                        string removeCommand = string.Format(@"http delete urlacl url=http://{0}:{1}", curAdd.ToString(), portAndUri);
                        TL.LogMessage("Network", string.Format("Sending UrlAcl Delete command to NetSh: {0}", removeCommand));
                        SendNetshCommand(removeCommand);
                        TL.BlankLine();
                    }
                }

                // Remove localhost entry if present
                string localHostCommand = string.Format(@"http delete urlacl url=http://127.0.0.1:{0}", portAndUri);
                TL.LogMessage("Network", string.Format("Sending UrlAcl Delete command to NetSh: {0}", localHostCommand));
                SendNetshCommand(localHostCommand);

                // Remove + and * wild card entries if present
                string plusCommand = string.Format(@"http delete urlacl url=http://+:{0}", portAndUri);
                TL.LogMessage("Network", string.Format("Sending UrlAcl Delete command to NetSh: {0}", plusCommand));
                SendNetshCommand(plusCommand);
                string starCommand = string.Format(@"http delete urlacl url=http://*:{0}", portAndUri);
                TL.LogMessage("Network", string.Format("Sending UrlAcl Delete command to NetSh: {0}", starCommand));
                SendNetshCommand(starCommand);

                // Now send the new UrlAcl
                TL.LogMessage("SetAcl", "Sending new UrlAcl command to NetSh: " + command);
                SendNetshCommand(command);

            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetAcl", "Process exception: " + ex.ToString());
            }
        }

        private static void SendNetshCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh")
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            TL.LogMessage("SendNetshCommand", "Creating netsh process");
            Process process = new Process();
            process.StartInfo = psi;
            process.OutputDataReceived += (sender, args) => TL.LogMessage("SetAcl", string.Format("NetSh output: {0}", args.Data));

            TL.LogMessage("SendNetshCommand", "Starting process");
            process.Start();
            process.BeginOutputReadLine();
            TL.LogMessage("SendNetshCommand", "Process started");

            TL.LogMessage("SendNetshCommand", string.Format("Sending netsh command: {0}", command));
            process.StandardInput.WriteLine(command);
            TL.LogMessage("SendNetshCommand", string.Format("Sent netsh command: {0}", command));

            TL.LogMessage("SendNetshCommand", string.Format("Sending netsh command: exit"));
            process.StandardInput.WriteLine("exit");
            TL.LogMessage("SendNetshCommand", string.Format("Sent netsh command: exit"));

            process.WaitForExit();
            TL.LogMessage("SendNetshCommand", "Process ended");

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            TraceLogger TL = new TraceLogger("SetNetworkPemissionsException")
            {
                Enabled = true
            };

            TL.LogMessageCrLf("Main", "Unhandled exception: " + exception.ToString());
            TL.Enabled = false;
            TL.Dispose();
            Environment.Exit(0);
        }
    }
}