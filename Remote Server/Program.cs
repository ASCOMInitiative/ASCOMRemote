using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
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
        static void Main()
        {
            // Restart as the other bitness version if required
            bool is64BitOS = Environment.Is64BitOperatingSystem;
            bool is64BitApplication = Environment.Is64BitProcess;

            // Check whether we are supposed to be running as a 32bit or a 64bit application
            if (Settings.RunAs64Bit)  // We are supposed to be running as a 64bit application
            {
                // Check whether the OS is 32bit or 64bit
                if (is64BitOS) // OS is 64bit
                {
                    // Check whether the application is running as 32bit or 64bit
                    if (is64BitApplication) // Application is 64bit
                    {
                        // No action required because we are already running as 64bit
                    }
                    else // Applicati9n is 32bit
                    {
                        // Start the 64bit version of the application

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
                if (is64BitOS) // OS is 64bit
                {
                    // Check whether the application is running as 32bit or 64bit
                    if (is64BitApplication) // Application is 64bit
                    {
                        // Start the 32bit version of the application

                        // End this program
                        return;
                    }
                    else // Application is 32bit
                    {
                        // No action required because we are already running as 32bit on the 64bit OS
                    }
                }
                else // OS is 32bit
                {
                    // No action required because the OS is 32bit so this application must also be 32bit.
                }
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
            TraceLogger TL = new("RemoteAccessServerException",true)
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
            TraceLogger TL = new("RemoteAccessServerException",true)
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
