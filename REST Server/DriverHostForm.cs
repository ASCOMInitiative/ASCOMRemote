using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ASCOM.Remote
{
    /// <summary>
    /// Form class to host a single driver and provide the required message loop to make sure all works properly
    /// The driver is created in the form loaded event and signalled by adding an entry in ActiveObjects in the form load event handler.
    /// </summary>
    public partial class DriverHostForm : Form
    {
        TraceLoggerPlus TL;
        string deviceKey;
        ServerForm restServer;
        KeyValuePair<string, ConfiguredDevice> configuredDevice;

        /// <summary>
        /// Main constructor for the form. Save state variables to use when creating, using and destroying the driver
        /// </summary>
        /// <param name="traceLoggerPlus">TraceLogger object</param>
        /// <param name="device">ConfiguredDevice object with details of the driver to be created</param>
        /// <param name="server">Handle to the main server form</param>
        public DriverHostForm(TraceLoggerPlus traceLoggerPlus, KeyValuePair<string, ConfiguredDevice> device, ServerForm server)
        {
            InitializeComponent();
            TL = traceLoggerPlus;
            configuredDevice = device;
            restServer = server;

            this.FormClosed += DriverHostForm_FormClosed;
            this.Load += DriverHostForm_Load;
            TL.LogMessage(0, 0, 0, "DriverHostForm", "Form has been instantiated on thread: " + Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// Form load event - Create the driver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriverHostForm_Load(object sender, EventArgs e)
        {
            deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber);

            TL.LogMessage(0, 0, 0, "DriverHostForm", string.Format("Creating driver {0} ({1}) on thread {2}", deviceKey, configuredDevice.Key, Thread.CurrentThread.ManagedThreadId));
            restServer.CreateInstance(configuredDevice); // Create the driver on this thread
            TL.LogMessage(0, 0, 0, "DriverHostForm", string.Format("Created driver {0} ({1}) on thread {2}", deviceKey, configuredDevice.Key, Thread.CurrentThread.ManagedThreadId));

            ServerForm.ActiveObjects[deviceKey].DriverHostForm = this; // Save the driver host form reference so that calls can be made to the driver
        }

        /// <summary>
        /// When the form is closing stop the windows message loop on this thread so that the thread will end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriverHostForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread();
        }

        /// <summary>
        /// Send a command to the driver
        /// </summary>
        /// <param name="requestData">Details of the command to send</param>
        public void DriverCommand(RequestData requestData)
        {
            try // Process the command
            {
                if (ServerForm.DebugTraceState) TL.LogMessage(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, "DriverCommand", string.Format("Received command for {0} on thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));
                Application.DoEvents();
                restServer.ProcessDriverCommand(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, requestData.SuppliedParameters, requestData.Request, requestData.Response, requestData.Elements, requestData.DeviceKey);
                if (ServerForm.DebugTraceState) TL.LogMessage(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, "DriverCommand", string.Format("Completed command for {0} on thread {1}", deviceKey, Thread.CurrentThread.ManagedThreadId));
            }
            catch (Exception ex) // Something serious has gone wrong with the ASCOM Rest server itself so report this to the user
            {
                ServerForm.LogException(requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID, "DriverCommand", ex.ToString());
                restServer.Return500Error(requestData.Request, requestData.Response, "Internal server error (DriverOnSeparateThread): " + ex.ToString(), requestData.ClientID, requestData.ClientTransactionID, requestData.ServerTransactionID);
            }
        }

        /// <summary>
        /// Destroy the driver
        /// </summary>
        public void DestroyDriver()
        {
            TL.LogMessage(0, 0, 0, "DriverHostForm", "Destroy driver method has been called on thread: " + Thread.CurrentThread.ManagedThreadId);
            ServerForm.DestroyDriver(deviceKey);
            TL.LogMessage(0, 0, 0, "DriverHostForm", "Destroy driver method completed on thread: " + Thread.CurrentThread.ManagedThreadId);
            Application.ExitThread(); // Close all forms on this thread, which will also terminate the thread itself
        }
    }
}
