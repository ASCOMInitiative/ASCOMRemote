using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASCOM.Utilities;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using System.Text.RegularExpressions;

namespace ASCOM.DynamicRemoteClients
{
    static class Program
    {
        static TraceLogger TL;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {

            TL = new TraceLogger("", "DynamicClients");
            TL.Enabled = true;

            try
            {
                string parameter = ""; // Iniitialise the supplied parameter to empty string
                if (args.Length > 0) parameter = args[0]; // Copy any supplied parameter to the paramemter variable

                TL.LogMessage("Main", string.Format(@"Supplied parameter: ""{0}""", parameter));
                parameter = parameter.TrimStart(' ', '-', '/', '\\'); // Remove any parameter prefixes and leading spaces
                parameter = parameter.TrimEnd(' '); // Remove any trailing spaces

                TL.LogMessage("Main", string.Format(@"Trimmed parameter: ""{0}""", parameter));

                switch (parameter.ToUpperInvariant()) // Act on the supplied parameter, if any
                {
                    case "": // Run the application in user interactive mode
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        TL.LogMessage("Main", "Starting application form");
                        Application.Run(new Form1(TL));
                        break;

                    case "INSTALLERSETUP": // Called by the installer to create drivers on first time use
                        TL.LogMessage("Main", "Running installer setup");

                        // Find if there are any driver files already installed, indicating that this is not a first time install
                        string localServerPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86) + Form1.REMOTE_SERVER_PATH;
                        string deviceType = "*";
                        string searchPattern = string.Format(Form1.REMOTE_CLIENT_DRIVER_NAME_TEMPLATE, deviceType);

                        IEnumerable<string> files = Directory.EnumerateFiles(localServerPath).Where(name => Regex.IsMatch(name, @"ascom\.remote\d\.\w+\.dll", RegexOptions.IgnoreCase)); // Regex format - ASCOM.REMOTEn.xxxxxx.DLL
                        TL.LogMessage("Apply", string.Format("Found {0} driver files", files.Count()));

                        if (files.Count() == 0) // There are no driver files present so this is a first time installation
                        {
                            TL.LogMessage("Main", string.Format("Found {0} driver files so this is a first time installation - creating one remote client driver of each device type", files.Count()));

                            string localServerExe = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86) + Form1.REMOTE_SERVER_PATH + Form1.REMOTE_SERVER; // Get the local server path
                            if (File.Exists(localServerExe)) // Local server does exist
                            {
                                // Check whether any pdb files are present, if so delete them
                                files = Directory.EnumerateFiles(localServerPath).Where(name => Regex.IsMatch(name, @"ascom\.remote\d\.\w+\.pdb", RegexOptions.IgnoreCase)); // Regex format - ASCOM.REMOTEn.xxxxxx.PDB
                                TL.LogMessage("Main", string.Format("Found {0} debug files", files.Count()));
                                if (files.Count() != 0) // Delete extraneous pdb files
                                {
                                    foreach (string file in files)
                                    {
                                        TL.LogMessage("Main", string.Format("Deleting driver file {0}", file));
                                        try
                                        {
                                            File.Delete(file);
                                            TL.LogMessage("Main", string.Format("Successfully deleted driver file {0}", file));
                                        }
                                        catch (Exception ex)
                                        {
                                            string errorMessage = string.Format("Unable to delete driver file {0} - {1}", file, ex.Message);
                                            TL.LogMessage("Main", errorMessage);
                                            MessageBox.Show(errorMessage);
                                        }
                                    }
                                }

                                // Create the required drivers
                                TL.LogMessage("Main", "Creating drivers");
                                Form1.CreateDriver("Camera", 1, localServerPath, TL);
                                Form1.CreateDriver("Dome", 1, localServerPath, TL);
                                Form1.CreateDriver("FilterWheel", 1, localServerPath, TL);
                                Form1.CreateDriver("Focuser", 1, localServerPath, TL);
                                Form1.CreateDriver("ObservingConditions", 1, localServerPath, TL);
                                Form1.CreateDriver("Rotator", 1, localServerPath, TL);
                                Form1.CreateDriver("SafetyMonitor", 1, localServerPath, TL);
                                Form1.CreateDriver("Switch", 1, localServerPath, TL);
                                Form1.CreateDriver("Telescope", 1, localServerPath, TL);

                                // Register the drivers
                                TL.LogMessage("Main", "Registering drivers");
                                Form1.RunLocalServer(localServerExe, "-regserver", TL);
                            }
                            else // Local server can not be found so report the issue
                            {
                                string errorMessage = string.Format("Could not find local server {0}, unable to register drivers", localServerExe);
                                TL.LogMessage("Main", errorMessage);
                                MessageBox.Show(errorMessage);
                            }
                        }
                        else // Drivers are already installed so no action required
                        {
                            TL.LogMessage("Main", string.Format("Found {0} driver files so this is not a first time installation - no action taken", files.Count()));
                            foreach (string file in files)
                            {
                                TL.LogMessage("Main", string.Format("Found file: {0}", file));
                            }
                        }
                        break;

                    default: // Unrecognised parameter so flag this to the user
                        string errMsg = string.Format("Unrecognised parameter: {0}, the only valid value is /InstallerSetup", parameter);

                        MessageBox.Show(errMsg);
                        break;
                }

            }
            catch (Exception ex)
            {
                string errMsg = ("DynamicRemoteClients exception: " + ex.ToString());
                TL.LogMessageCrLf("Main", errMsg);
                MessageBox.Show(errMsg);
            }

            TL.Enabled = false;
            TL.Dispose();
            TL = null;
        }
    }
}
