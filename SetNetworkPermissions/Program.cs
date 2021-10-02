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
using System.Runtime.ExceptionServices;
using System.Reflection;
using WindowsFirewallHelper.Collections;
using WindowsFirewallHelper.FirewallRules;

namespace ASCOM.Remote
{
    static class Program
    {
        const string LOCAL_SERVER_OUTBOUND_RULE_NAME = @"ASCOM Local Device Server - Outbound";
        const string REMOTE_SERVER_RULE_NAME_BASE = @"ASCOM Remote Device Server";
        const string HTTP_DOT_SYS_INBOUND_RULE_NAME = @"ASCOM Remote Server HTTP.SYS";
        const string GROUP_NAME = "ASCOM Remote";

        public const string NOT_PRESENT_FLAG = "***** PARAMETER NOT PRESENT *****";

        static TraceLogger TL;

        [HandleProcessCorruptedStateExceptions]
        static void Main(string[] args)
        {

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            try
            {
                TL = new TraceLogger("", "SetNetworkPermissions")
                {
                    Enabled = true
                };

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

        private static void ProcessOptions(Options opts)
        {

            TL.LogMessage("ProcessOptions", $"Local server path: {opts.LocalServerPath}");
            TL.LogMessage("ProcessOptions", $"API URI: {opts.SetApiUriAcl}");
            TL.LogMessage("ProcessOptions", $"Remote server management URI: {opts.SetRemoteServerManagementUriAcl}");
            TL.LogMessage("ProcessOptions", $"Alpaca device management URI: {opts.SetAlpacaManagementUriAcl}");
            TL.LogMessage("ProcessOptions", $"Alpaca device HTML setup URI: {opts.SetAlpacaSetupUriAcl}");
            TL.LogMessage("ProcessOptions", $"HTTP.SYS port: {opts.HttpDotSysPort}");
            TL.LogMessage("ProcessOptions", $"HTTP.SYS supplied user name: {opts.UserName}");

            // Get the supplied user name but substitute the current user name if none was supplied
            string userName = $"\"{opts.UserName}\"";
            if (userName == NOT_PRESENT_FLAG) userName = $"{Environment.UserDomainName}\\{Environment.UserName}";
            TL.LogMessage("ProcessOptions", $"HTTP.SYS user name that will be used: {userName}");
            TL.BlankLine();

            // Set firewall rules as needed
            if (opts.LocalServerPath != NOT_PRESENT_FLAG) SetLocalServerFireWallOutboundRule(opts.LocalServerPath);
            if (opts.HttpDotSysPort != NOT_PRESENT_FLAG) SetHttpSysFireWallInboundRule(opts.HttpDotSysPort);


            // Set http.sys ACLs as needed
            if (opts.SetApiUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetApiUriAcl, userName);
            if (opts.SetRemoteServerManagementUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetRemoteServerManagementUriAcl, userName);
            if (opts.SetAlpacaManagementUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetAlpacaManagementUriAcl, userName);
            if (opts.SetAlpacaSetupUriAcl != NOT_PRESENT_FLAG) SetAcl(opts.SetAlpacaSetupUriAcl, userName);
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (Error err in errs)
            {
                TL.LogMessage("HandleParseError", $"Error stopped processing: {err.StopsProcessing}, {err}");
            }
        }

        private static void SetLocalServerFireWallOutboundRule(string applicationPath)
        {
            try // Make sure that we still try and set the firewall rules even if we bomb out trying to get information on the firewall configuration
            {
                TL.LogMessage("QueryFireWall", string.Format("Firewall version: {0}", FirewallManager.Version.ToString())); // Log the firewall version in use
                foreach (IFirewallProfile profile in FirewallManager.Instance.Profiles)
                {
                    TL.LogMessage("QueryFireWall", string.Format("Found current firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                }

                COMTypeResolver cOMTypeResolver = new COMTypeResolver();
                IFirewallProductsCollection thirdPartyFirewalls = FirewallManager.GetRegisteredProducts(cOMTypeResolver);
                TL.LogMessage("QueryFireWall", string.Format("number of third party firewalls: {0}", thirdPartyFirewalls.Count));
                foreach (FirewallProduct firewall in thirdPartyFirewalls)
                {
                    TL.LogMessage("QueryFireWall", $"Found third party firewall: {firewall.Name} - {firewall.FriendlyName}");
                    //foreach (IFirewallProfile profile in firewall.)
                    //{
                    //    TL.LogMessage("QueryFireWall", string.Format("Found third party firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                    //}
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
                        TL.LogMessage("SetFireWallOutboundRule", string.Format("Supplied path: {0}, full path: {1}", applicationPath, applicationPathFull));

                        // Now clear up previous instances of this rule
                        IEnumerable<IFirewallRule> query = FirewallManager.Instance.Rules.Where(ruleName => ruleName.Name.ToUpperInvariant().StartsWith(LOCAL_SERVER_OUTBOUND_RULE_NAME.ToUpperInvariant()));
                        List<IFirewallRule> queryCopy = query.ToList();
                        foreach (IFirewallRule existingRule in queryCopy)
                        {
                            TL.LogMessage("SetFireWallOutboundRule", string.Format("Found rule: {0}", existingRule.Name));
                            FirewallManager.Instance.Rules.Remove(existingRule); // Delete the rule
                            TL.LogMessage("SetFireWallOutboundRule", string.Format("Deleted rule: {0}", existingRule.Name));
                        }

                        IFirewallRule rule = FirewallManager.Instance.CreateApplicationRule(FirewallManager.Instance.GetProfile(FirewallProfiles.Domain | FirewallProfiles.Private | FirewallProfiles.Public).Type, LOCAL_SERVER_OUTBOUND_RULE_NAME, FirewallAction.Allow, applicationPathFull);
                        rule.Direction = FirewallDirection.Outbound;

                        // Add the group name to the outbound rule
                        if (rule is FirewallWASRule) //Rules.StandardRule)
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a standard rule");
                            ((FirewallWASRule)rule).Grouping = GROUP_NAME;
                            TL.LogMessage("SetHttpSysFireWallRule", $"Group name set to: {GROUP_NAME}");
                        }
                        else
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a standard rule");
                        }
                        if (rule is FirewallWASRuleWin7)
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a WIN7 rule");
                            ((FirewallWASRuleWin7)rule).Grouping = GROUP_NAME;
                            TL.LogMessage("SetHttpSysFireWallRule", $"Group name set to: {GROUP_NAME}");
                        }
                        else
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a WIN7 rule");
                        }
                        if (rule is FirewallWASRuleWin8)
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a WIN8 rule");
                            ((FirewallWASRuleWin8)rule).Grouping = GROUP_NAME;
                            TL.LogMessage("SetHttpSysFireWallRule", $"Group name set to: {GROUP_NAME}");
                        }
                        else
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a WIN8 rule");
                        }

                        TL.LogMessage("SetFireWallOutboundRule", "Successfully created outbound rule");
                        FirewallManager.Instance.Rules.Add(rule);
                        TL.LogMessage("SetFireWallOutboundRule", string.Format("Successfully added outbound rule for {0}", applicationPathFull));

                    }
                    else
                    {
                        TL.LogMessage("SetFireWallOutboundRule", string.Format("The specified file does not exist: {0}", applicationPath));
                        Console.WriteLine("The specified file does not exist: {0}", applicationPath);
                    }
                }
                else
                {
                    TL.LogMessage("SetFireWallOutboundRule", "Not running as Administrator so unable to set firewall rules.");
                    Console.WriteLine("Not running as Administrator so unable to set firewall rules.");
                }
                TL.BlankLine();
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetFireWallOutboundRule", "Exception: " + ex.ToString());
                Console.WriteLine("SetFireWallOutboundRule threw an exception: " + ex.Message);
            }
        }

        private static void SetHttpSysFireWallInboundRule(string portNumberString)
        {
            try // Make sure that we still try and set the firewall rules even if we bomb out trying to get information on the firewall configuration
            {
                TL.LogMessage("QueryFireWall", string.Format("Firewall version: {0}", FirewallManager.Version.ToString())); // Log the firewall version in use
                foreach (IFirewallProfile profile in FirewallManager.Instance.Profiles)
                {
                    TL.LogMessage("QueryFireWall", string.Format("Found current firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                }

                COMTypeResolver cOMTypeResolver = new COMTypeResolver();
                IFirewallProductsCollection thirdPartyFirewalls = FirewallManager.GetRegisteredProducts(cOMTypeResolver);
                TL.LogMessage("QueryFireWall", string.Format("number of third party firewalls: {0}", thirdPartyFirewalls.Count));
                foreach (FirewallProduct firewall in thirdPartyFirewalls)
                {
                    TL.LogMessage("QueryFireWall", $"Found third party firewall: {firewall.Name} - {firewall.FriendlyName}");
                    //foreach (IFirewallProfile profile in firewall.Profiles)
                    //{
                    //    TL.LogMessage("QueryFireWall", string.Format("Found third party firewall profile {0}, enabled: {1}", profile.Type.ToString(), profile.IsActive));
                    //}
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
                    TL.LogMessage("SetHttpSysFireWallRule", $"Supplied HTTP.SYS port: {portNumberString}");

                    if (ushort.TryParse(portNumberString, out ushort portNumber)) // Make sure the supplied port number is a valid value before processing it
                    {
                        // Clear up redundant firewall rules left over from previous versions (ASCOM Remote Server - Inbound and Outbound)
                        IEnumerable<IFirewallRule> queryRedundant = FirewallManager.Instance.Rules.Where(ruleName => ruleName.Name.ToUpperInvariant().StartsWith(REMOTE_SERVER_RULE_NAME_BASE.ToUpperInvariant()));
                        List<IFirewallRule> queryRedundantCopy = queryRedundant.ToList();
                        foreach (IFirewallRule existingRule in queryRedundantCopy)
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", string.Format("Found redundant rule: {0}", existingRule.Name));
                            FirewallManager.Instance.Rules.Remove(existingRule); // Delete the rule
                            TL.LogMessage("SetHttpSysFireWallRule", string.Format("Deleted redundant rule: {0}", existingRule.Name));
                        }

                        // Check whether the specified file exists and if so delete it
                        IEnumerable<IFirewallRule> query = FirewallManager.Instance.Rules.Where(ruleName => ruleName.Name.ToUpperInvariant().Equals(HTTP_DOT_SYS_INBOUND_RULE_NAME.ToUpperInvariant()));
                        List<IFirewallRule> queryCopy = query.ToList();
                        foreach (IFirewallRule existingRule in queryCopy)
                        {
                            TL.LogMessage("SetHttpSysFireWallRule", string.Format("Found rule: {0}", existingRule.Name));
                            FirewallManager.Instance.Rules.Remove(existingRule); // Delete the rule
                            TL.LogMessage("SetHttpSysFireWallRule", string.Format("Deleted rule: {0}", existingRule.Name));
                        }

                        SetHttpRule(FirewallProfiles.Private, portNumber);
                        SetHttpRule(FirewallProfiles.Public, portNumber);
                        SetHttpRule(FirewallProfiles.Domain, portNumber);
                    }
                    else
                    {
                        TL.LogMessage("SetHttpSysFireWallRule", $"Supplied port number {portNumberString} is not valid so can't set permission for HTTP.SYS");
                        Console.WriteLine($"Supplied port number: \"{portNumberString}\" is not valid so can't set permission for HTTP.SYS");
                    }
                }
                else
                {
                    TL.LogMessage("SetHttpSysFireWallRule", "Not running as Administrator so unable to set firewall rules.");
                    Console.WriteLine("Not running as Administrator so unable to set firewall rules.");
                }
                TL.BlankLine();
            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetHttpSysFireWallRule", "Exception: " + ex.ToString());
                Console.WriteLine("SetHttpSysFireWallRule threw an exception: " + ex.Message);
            }

        }
        private static void SetHttpRule(FirewallProfiles firewallProfile, ushort portNumber)
        {
            IFirewallRule rule = FirewallManager.Instance.CreateApplicationRule(
                FirewallManager.Instance.GetProfile(firewallProfile).Type,
                HTTP_DOT_SYS_INBOUND_RULE_NAME,
                FirewallAction.Allow,
                "SYSTEM");
            rule.Direction = FirewallDirection.Inbound;
            rule.Protocol = FirewallProtocol.TCP;
            rule.LocalPorts = new ushort[1] { portNumber }; // Create an array containing the port number
            TL.LogMessage("SetHttpSysFireWallRule", "Successfully created inbound rule");


            // Add edge traversal permission to the inbound rule so that the rule will apply to packets delivered in encapsulated transmission formats such as VPNs
            if (rule is FirewallWASRule)
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a standard rule");
                ((FirewallWASRule)rule).EdgeTraversal = true;
                ((FirewallWASRule)rule).Grouping = GROUP_NAME;
                TL.LogMessage("SetHttpSysFireWallRule", $"Edge traversal set {true}, Group name set to: {GROUP_NAME}");
            }
            else
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a standard rule");
            }
            if (rule is FirewallWASRuleWin7)
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a WIN7 rule");
                ((FirewallWASRuleWin7)rule).EdgeTraversalOptions = EdgeTraversalAction.Allow;
                ((FirewallWASRuleWin7)rule).Grouping = GROUP_NAME;
                TL.LogMessage("SetHttpSysFireWallRule", $"Edge traversal set {true}, Group name set to: {GROUP_NAME}");
            }
            else
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a WIN7 rule");
            }
            if (rule is FirewallWASRuleWin8)
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is a WIN8 rule");
                ((FirewallWASRuleWin8)rule).EdgeTraversalOptions = EdgeTraversalAction.Allow;
                ((FirewallWASRuleWin8)rule).Grouping = GROUP_NAME;
                TL.LogMessage("SetHttpSysFireWallRule", $"Edge traversal set {true}, Group name set to: {GROUP_NAME}");
            }
            else
            {
                TL.LogMessage("SetHttpSysFireWallRule", "Firewall rule is not a WIN8 rule");
            }

            TL.LogMessage("SetHttpSysFireWallRule", "Successfully created inbound firewall rule");

            FirewallManager.Instance.Rules.Add(rule);
            TL.LogMessage("SetHttpSysFireWallRule", $"Successfully added inbound rule for HTTP.SYS permitting listening on port {portNumber} for {firewallProfile}");
            TL.BlankLine();
        }
        private static void SetAcl(string uri, string userName)
        {
            try
            {
                string command = $"http add urlacl url={uri} user={userName}";
                TL.LogMessage("SetAcl", $"Enable arguments: {command}");

                // Parse out the port number and resource value
                int doubleSlashIndex = uri.IndexOf("//");
                int colonIndex;

                if (uri.Contains("[")) // The URI contains an IPv6 address
                {
                    colonIndex = uri.IndexOf("]:", doubleSlashIndex + 2) + 1;
                }
                else // A host name or IPv4 address
                {
                    colonIndex = uri.IndexOf(":", doubleSlashIndex + 2);
                }

                string portAndUri = uri.Substring(colonIndex + 1);
                TL.LogMessage("SetAcl", $"Colon index: {colonIndex}, Port and URI: {portAndUri}");

                string netshCommand = "";
                foreach (IPAddress ipAddress in HostPc.IpV4Addresses)
                {
                    TL.LogMessage("SetAcl", $"Found IP Network Address: {ipAddress}");

                    netshCommand += $@"http delete urlacl url=http://{ipAddress}:{portAndUri}" + "\r\n";
                    TL.LogMessage("SetAcl", $"Sending UrlAcl Delete command to NetSh: {netshCommand}");

                }

                // Remove the URL ACL if it exists
                SendNetshCommand(netshCommand);
                TL.BlankLine();

                netshCommand = "";
                foreach (IPAddress ipAddress in HostPc.IpV6Addresses)
                {
                    TL.LogMessage("SetAcl", $"Found IP Network Address: {ipAddress}");

                    netshCommand += $@"http delete urlacl url=http://[{ipAddress}]:{portAndUri}" + "\r\n";
                    TL.LogMessage("SetAcl", $"Sending UrlAcl Delete command to NetSh: {netshCommand}");

                }

                // Remove the URL ACL if it exists
                SendNetshCommand(netshCommand);
                TL.BlankLine();

                netshCommand = "";
                // Remove localhost entry if present
                string localHostCommand = $@"http delete urlacl url=http://127.0.0.1:{portAndUri}" + "\r\n";
                TL.LogMessage("SetAcl", $"Sending UrlAcl Delete command to NetSh: {localHostCommand}");
                netshCommand += localHostCommand;

                // Remove + wild card entry if present
                string plusCommand = $@"http delete urlacl url=http://+:{portAndUri}" + "\r\n";
                TL.LogMessage("SetAcl", $"Sending UrlAcl Delete command to NetSh: {plusCommand}");
                netshCommand += plusCommand;

                // Remove * wild card entry if present
                string starCommand = $@"http delete urlacl url=http://*:{portAndUri}" + "\r\n";
                TL.LogMessage("SetAcl", $"Sending UrlAcl Delete command to NetSh: {starCommand}");
                netshCommand += starCommand;

                // Now send the new UrlAcl
                TL.LogMessage("SetAcl", $"Sending new UrlAcl command to NetSh: {command}");
                netshCommand += command + "\r\n";
                SendNetshCommand(netshCommand);

            }
            catch (Exception ex)
            {
                TL.LogMessageCrLf("SetAcl", $"Process exception: {ex}");
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
            Process process = new Process
            {
                StartInfo = psi
            };
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