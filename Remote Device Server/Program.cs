using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASCOM.Utilities;

namespace ASCOM.Web
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Add the event handler for handling UI thread exceptions to the event.
            Application.ThreadException += new
          ThreadExceptionEventHandler(Application_ThreadException);

            // Set the unhandled exception mode to force all Windows Forms errors
            // to go through our handler.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += new
            UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerForm());
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            TraceLogger TL = new TraceLogger("RemoteAccessServerException")
            {
                Enabled = true
            };
            TL.LogMessageCrLf("Main", "Thread exception: " + e.Exception.ToString());
            TL.Enabled = false;
            TL.Dispose();
            TL = null;

            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception, see RemoteAccessServerException log for details.");
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
            TL.Enabled = false;
            TL.Dispose();
            TL = null;
            MessageBox.Show(exception.Message, "Unhandled UI Exception, see RemoteAccessServerException log for details.");
            Environment.Exit(0);
        }
    }
}
