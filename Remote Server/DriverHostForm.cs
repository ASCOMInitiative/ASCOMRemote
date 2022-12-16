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
        string deviceKey;
        ServerForm restServer;
        KeyValuePair<string, ConfiguredDevice> configuredDevice;

        /// <summary>
        /// Main constructor for the form. Save state variables to use when creating, using and destroying the driver
        /// </summary>
        /// <param name="traceLoggerPlus">TraceLogger object</param>
        /// <param name="device">ConfiguredDevice object with details of the driver to be created</param>
        /// <param name="server">Handle to the main server form</param>
        public DriverHostForm(KeyValuePair<string, ConfiguredDevice> device, ServerForm server)
        {
            InitializeComponent();
            configuredDevice = device;
            restServer = server;

            this.FormClosed += DriverHostForm_FormClosed;
            this.Load += DriverHostForm_Load;
            ServerForm.LogMessage(0, 0, 0, "DriverHostForm", "Form has been instantiated on thread: " + Thread.CurrentThread.ManagedThreadId);
        }

        /// <summary>
        /// Form load event - Create the driver
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DriverHostForm_Load(object sender, EventArgs e)
        {
            deviceKey = string.Format("{0}/{1}", configuredDevice.Value.DeviceType.ToLowerInvariant(), configuredDevice.Value.DeviceNumber);

            ServerForm.LogMessage(0, 0, 0, "DriverHostForm", $"Creating driver {deviceKey} ({configuredDevice.Key}) on thread {Thread.CurrentThread.ManagedThreadId} with apartment state {Thread.CurrentThread.GetApartmentState()}");
            restServer.CreateInstance(configuredDevice); // Create the driver on this thread
            ServerForm.LogMessage(0, 0, 0, "DriverHostForm", string.Format("Created driver {0} ({1}) on thread {2}", deviceKey, configuredDevice.Key, Thread.CurrentThread.ManagedThreadId));

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
        /// <returns>Background thread on which the command is executing</returns>
        public Thread DriverCommand(RequestData requestData)
        {
            try // Process the command
            {
                Application.DoEvents();

                // Process the command on a separate thread allowing other requests to be handled concurrently through this thread, which is running the Windows message loop
                Thread driverThread = new Thread(new ParameterizedThreadStart(ProcessCommand)); // Create a new thread on which to make the call to the COM driver
                if (ServerForm.DebugTraceState) ServerForm.LogMessage1(requestData, requestData.Elements[SharedConstants.URL_ELEMENT_METHOD], $"DriverCommand has received a command for {deviceKey} on FORM thread {Thread.CurrentThread.ManagedThreadId} Apartment state: {Thread.CurrentThread.GetApartmentState()} Is background: {Thread.CurrentThread.IsBackground} Is thread pool thread: {Thread.CurrentThread.IsThreadPoolThread}");

                driverThread.Start(requestData); // Start the thread supplying the request data to the method

                if (ServerForm.DebugTraceState) ServerForm.LogMessage1(requestData, requestData.Elements[SharedConstants.URL_ELEMENT_METHOD], $"DriverCommand has started the command for {deviceKey} on FORM thread { Thread.CurrentThread.ManagedThreadId}");
                return driverThread; // Return the thread so that the calling method can wait for it to complete and so that this thread can start waiting for the next command
            }
            catch (Exception ex) // Something serious has gone wrong with the ASCOM Remote server itself so report this to the user
            {
                ServerForm.LogException1(requestData, "DriverCommand", ex.ToString());
                restServer.Return500Error(requestData, "Internal server error (DriverOnSeparateThread): " + ex.ToString());
            }
            return null;
        }

        void ProcessCommand(object requestData)
        {
            restServer.ProcessDriverCommand((RequestData)requestData);
        }

        /// <summary>
        /// Destroy the driver
        /// </summary>
        public void DestroyDriver()
        {
            ServerForm.LogMessage(0, 0, 0, "DriverHostForm", "Destroy driver method has been called on thread: " + Thread.CurrentThread.ManagedThreadId);
            ServerForm.DestroyDriver(deviceKey);
            ServerForm.LogMessage(0, 0, 0, "DriverHostForm", "Destroy driver method completed on thread: " + Thread.CurrentThread.ManagedThreadId);
            Application.ExitThread(); // Close all forms on this thread, which will also terminate the thread itself
        }
    }
}
