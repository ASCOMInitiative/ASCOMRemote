using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ASCOM.Tools;

namespace ASCOM.Remote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
            // Check whether any command line arguments were supplied
            if (arguments.Length > 0) // One or more arguments were supplied
            {
                switch (arguments[0].ToLowerInvariant())
                {
                    case "-reset":
                    case "--reset":
                    case "/reset":
                        // Reset the configuration
                        TraceLoggerPlus TLReset = new("RemoteReset", true);
                        using (ConfigurationManager configurationManager = new ConfigurationManager(TLReset))
                        {
                            configurationManager.Reset();
                            configurationManager.Save();
                        }

                        Configuration.Reset();

                        MessageBox.Show($"Configuration reset to default values.", "Reset Configuration", MessageBoxButtons.OK);
                        break;

                    default:
                        MessageBox.Show($"The parameter: {arguments[0]} is not valid.", "Unrecognised command line parameter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                }
            }

            TraceLoggerPlus TLInit = new("RemoteInit", true);
            using (ConfigurationManager configurationManager = new ConfigurationManager(TLInit))
            {
                // Restart as the other bitness version if required

                // Check whether we are supposed to be running as a 32bit or a 64bit application
                if (configurationManager.Settings.RunAs64Bit)  // We are supposed to be running as a 64bit application
                {
                    // Check whether the OS is 32bit or 64bit
                    if (Environment.Is64BitOperatingSystem) // OS is 64bit
                    {
                        // Check whether the application is running as 32bit or 64bit
                        if (Environment.Is64BitProcess) // Application is 64bit
                        {
                            // No action required because we are already running as 64bit
                            Debug.WriteLine($"Already running as 64bit");
                        }
                        else // Application is 32bit
                        {
                            // Start the 64bit version of the application
                            string folderName32 = Path.GetDirectoryName(Application.ExecutablePath); // Folder name of the 32bit version
                            Debug.WriteLine($"Folder name = {folderName32}");
                            string folderName64 = Directory.GetParent(folderName32).FullName;

                            Debug.WriteLine($"New folder name = {folderName64}");
                            Process.Start($"{folderName64}\\remoteserver.exe");

                            // End this program
                            return;
                        }
                    }
                    else // OS is 32bit
                    {
                        // Can't run as a 64bit application on a 32bit OS so no action because this must already be the 32bit version of the application
                    }
                }
                else // We are supposed to be running as a 32bit application
                {
                    // Check whether the OS is 32bit or 64bit
                    if (Environment.Is64BitOperatingSystem) // OS is 64bit
                    {
                        // Check whether the application is running as 32bit or 64bit
                        if (Environment.Is64BitProcess) // Application is 64bit
                        {
                            // Start the 32bit version of the application
                            Debug.WriteLine($"Starting the 32bit version");

                            string folderName = Path.GetDirectoryName(Application.ExecutablePath);
                            Debug.WriteLine($"Folder name = {folderName}");
                            folderName += @"\32bit";
                            Debug.WriteLine($"New folder name = {folderName}");
                            Process.Start($"{folderName}\\remoteserver.exe");

                            // End this program
                            return;
                        }
                        else // Application is 32bit
                        {
                            // No action required because we are already running as 32bit on the 64bit OS
                            Debug.WriteLine($"Already running as 32bit");
                        }
                    }
                    else // OS is 32bit
                    {
                        // No action required because the OS is 32bit so this application must also be 32bit.
                    }
                }

                Debug.WriteLine($"Starting application");
            }

#if !DEBUG // Exclude the un-handled exception handlers from the Debug version so that the application can be debugged in Visual Studio
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // Set the un-handled exception mode to force all Windows Forms errors to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServerForm serverForm = new();
            Application.Run(serverForm);

            if (serverForm.RestartApplication) Application.Restart(); // Restart the application if the network permissions have been changed

#if DEBUG // When debugging, include a log to show the time of close down
            TraceLogger TL = new("ASCOMRemoteEnded", true)
            {
                Enabled = true
            };
            TL.LogMessage("ASCOMRemoteEnded", "Application has exited");
            TL.Enabled = false;
            TL.Dispose();
            TL = null;
#endif
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            TraceLogger TL = new("RemoteAccessServerException", true)
            {
                Enabled = true
            };
            TL.LogMessage("Main", "Thread exception: " + e.Exception.ToString());
            Process.Start(TL.LogFileName);

            TL.Enabled = false;
            TL.Dispose();

            //MessageBox.Show(e.Exception.Message, "Un-handled Thread Exception, see RemoteAccessServerException log for details.");
            Environment.Exit(0);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            TraceLogger TL = new("RemoteAccessServerException", true)
            {
                Enabled = true
            };
            TL.LogMessage("Main", "Un-handled exception: " + exception.ToString());
            Process.Start(TL.LogFileName);
            TL.Enabled = false;
            TL.Dispose();
            Environment.Exit(0);
        }
    }
}
