using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Windows.Forms;
using ASCOM.Utilities;

namespace ASCOM.Remote
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        static void Main()
        {
#if !DEBUG // Exclude the unhandled exception handlers from the Debug version so that the application can be debugged in Visual Studio
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServerForm serverForm = new ServerForm();
            Application.Run(serverForm);

            if (serverForm.RestartApplication) Application.Restart(); // Restart the application if the network permissions have been changed

#if DEBUG // When debugging, include a log to show the time of close down
            TraceLogger TL = new TraceLogger("ASCOMRemoteEnded")
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
            TraceLogger TL = new TraceLogger("RemoteAccessServerException")
            {
                Enabled = true
            };
            TL.LogMessageCrLf("Main", "Thread exception: " + e.Exception.ToString());
            Process.Start(TL.LogFileName);

            TL.Enabled = false;
            TL.Dispose();

            //MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception, see RemoteAccessServerException log for details.");
            Environment.Exit(0);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            TraceLogger TL = new TraceLogger("RemoteAccessServerException")
            {
                Enabled = true
            };
            TL.LogMessageCrLf("Main", "Unhandled exception: " + exception.ToString());
            Process.Start(TL.LogFileName);
            TL.Enabled = false;
            TL.Dispose();
            Environment.Exit(0);
        }
    }
}
