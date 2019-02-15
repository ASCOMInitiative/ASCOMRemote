using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using ASCOM.Utilities;
using System.Text.RegularExpressions;

namespace ASCOM.Remote
{
    public partial class SetupForm : Form
    {

        #region Variables

        private List<string> registeredDeviceTypes = new List<string>();
        // Create a dictionary to hold the current device instance numbers of every device type
        private Dictionary<string, int> deviceNumberIndexes;
        private Profile profile;

        private bool selectByMouse = false; // Variable to help select the whole contents of a numeric up-down box when tabbed into our selected by mouse

        #endregion

        #region Setup and form load

        public SetupForm()
        {
            InitializeComponent();
            addressList.Validating += AddressList_Validating; // Add event handlers for IP address validation events
            chkTrace.CheckedChanged += ChkTrace_CheckedChanged;

            // Create event handlers to select the whole contents of the numeric up down boxes when tabbed into or selected by mouse click
            numPort.Enter += NumericUpDown_Enter;
            numPort.MouseDown += NumericUpDown_MouseDown;

        }

        private void Form_Load(object sender, EventArgs e)
        {
            // Declare local variables
            IPHostEntry host;
            bool foundAnIPAddress = false;
            bool foundTheIPAddress = false;
            int selectedIndex = 0;

            ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Start");

            profile = new Profile();
            host = Dns.GetHostEntry(Dns.GetHostName()); // Get an IPHostEntry so that we can get the list of IP addresses on this PC
            deviceNumberIndexes = new Dictionary<string, int>(); // Create a dictionary to hold the current device instance numbers of every device type

            // Create a list of valid IP addresses on this PC so that the user can select one on which to run the ASCOM device server.
            addressList.Items.Add(SharedConstants.LOCALHOST_NAME); // Make "localhost" the first entry in the list of addresses
            foreach (IPAddress ip in host.AddressList) // Add the other addresses on this PC
            {
                if ((ip.AddressFamily == AddressFamily.InterNetwork) & !foundAnIPAddress) // Only process IPv4 addresses and ignore the rest including IPv6
                {
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Found {0} Address: {1}", ip.AddressFamily.ToString(), ip.ToString()));
                    foundAnIPAddress = true;
                    addressList.Items.Add(ip.ToString());
                    if (ip.ToString() == ServerForm.ServerIPAddressString)
                    {
                        selectedIndex = addressList.Items.Count - 1;
                        foundTheIPAddress = true;
                    }
                }
                else
                {
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Ignored {0} Address: {1}", ip.AddressFamily.ToString(), ip.ToString()));
                }
            }
            ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Found an IP address: {0}, Found the IP address: {1}, Stored IP Address: {2}", foundAnIPAddress, foundTheIPAddress, ServerForm.ServerIPAddressString));

            if ((!foundTheIPAddress) & (ServerForm.ServerIPAddressString != "")) // Add the last stored IP address if it isn't found in the search above
            {
                addressList.Items.Add(ServerForm.ServerIPAddressString); // Add the stored address to the list
                selectedIndex = addressList.Items.Count - 1; // Select this item in the list
            }

            // Add the wild card addresses at the end of the list
            if (ServerForm.ServerIPAddressString != SharedConstants.STRONG_WILDCARD_NAME) addressList.Items.Add(SharedConstants.STRONG_WILDCARD_NAME); // Include the strong wild card character in the list of addresses if not already in use
            if (ServerForm.ServerIPAddressString != SharedConstants.WEAK_WILDCARD_NAME) addressList.Items.Add(SharedConstants.WEAK_WILDCARD_NAME); // Include the weak wild card character in the list of addresses if not already in use

            // Set up the GUI components
            addressList.SelectedIndex = selectedIndex;
            numPort.Value = ServerForm.ServerPortNumber;
            chkAutoConnect.Checked = ServerForm.StartWithDevicesConnected;
            chkAccessLog.Checked = ServerForm.AccessLogEnabled;
            chkTrace.Checked = ServerForm.TraceState;
            chkDebugTrace.Checked = ServerForm.DebugTraceState;
            chkDebugTrace.Enabled = ServerForm.TraceState; // Enable or disable the debug trace check box depending on whether normal trace is enabled
            chkManagementInterfaceEnabled.Checked = ServerForm.ManagementInterfaceEnabled;
            ChkStartWithApiEnabled.Checked = ServerForm.StartWithApiEnabled;
            LblDevicesNotDisconnoected.Visible = ServerForm.devicesAreConnected;
            ChkRunDriversInSeparateThreadss.Checked = ServerForm.RunDriversOnSeparateThreads;
            ChkLogClientIPAddress.Checked = ServerForm.LogClientIPAddress;
            ChkIncludeDriverExceptionsInJsonResponses.Checked = ServerForm.IncludeDriverExceptionInJsonResponse;

            // Populate the device types list
            foreach (string deviceType in profile.RegisteredDeviceTypes)
            {
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Adding device type: " + deviceType);
                registeredDeviceTypes.Add(deviceType); // Remember the device types on this system
            }

            ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Number of configured devices: {0}.", ServerForm.ConfiguredDevices.Count));
            foreach (string deviceName in ServerForm.ConfiguredDevices.Keys)
            {
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("ConfiguredDevices contains key {0}.", deviceName));
            }

            // Initialise each of the device GUI components
            foreach (ServedDevice item in this.Controls.OfType<ServedDevice>())
            {
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Starting Init for {0}.", item.Name));
                item.InitUI(this);
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Completed Init for {0}, now setting its parameters.", item.Name));
                item.DeviceType = ServerForm.ConfiguredDevices[item.Name].DeviceType;
                item.ProgID = ServerForm.ConfiguredDevices[item.Name].ProgID;
                item.DeviceNumber = ServerForm.ConfiguredDevices[item.Name].DeviceNumber;
                item.AllowConnectedSetFalse = ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetFalse;
                item.AllowConnectedSetTrue = ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetTrue;
                item.DevicesAreConnected = ServerForm.devicesAreConnected;

                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Completed Init for {0}.", item.Name));
            }

            RecalculateDeviceNumbers();

        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Event handler fired when entering a numeric up/down control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_Enter(object sender, EventArgs e)
        {
            NumericUpDown curBox = sender as NumericUpDown;
            curBox.Select();
            curBox.Select(0, curBox.Text.Length);
            if (MouseButtons == MouseButtons.Left)
            {
                selectByMouse = true;
            }
        }

        /// <summary>
        /// Event handler fired when a mouse down event happens in a numeric up/down control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumericUpDown_MouseDown(object sender, MouseEventArgs e)
        {
            NumericUpDown curBox = sender as NumericUpDown;
            if (selectByMouse)
            {
                curBox.Select(0, curBox.Text.Length);
                selectByMouse = false;
            }
        }

        /// <summary>
        /// Called whenever the Trace Enabled check box is changed so that the Debug Trace check box can be en/disabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkTrace_CheckedChanged(object sender, EventArgs e)
        {
            chkDebugTrace.Enabled = chkTrace.Checked; // Enable or disable the debug trace check box depending on whether normal trace is enabled
        }

        /// <summary>
        /// Called when the IP address value changes so that we can validate whether the new address is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddressList_Validating(object sender, CancelEventArgs e)
        {
            // Test whether or not we have a valid IP address, if not valid then indicate this and set the error message to display.
            if (!ValidIPAddress(addressList.Text, out string errorMsg))
            {
                // Not valid so cancel the event and select the text to be corrected by the user.
                e.Cancel = true;
                addressList.Select(0, addressList.Text.Length);
                BtnOK.Enabled = false;

                // Set the ErrorProvider error with the text to display. 
                this.errorProvider1.SetError(addressList, errorMsg);
            }
            else // We do have a valid IP address so permit the Validated event to fire and clear any previous error message
            {
                e.Cancel = false;
                BtnOK.Enabled = true;

                // Clear the ErrorProvider error message.
                errorProvider1.SetError(addressList, "");
            }
        }

        /// <summary>
        /// Called when the user presses the OK button to commit any new set up values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (addressList.Text == SharedConstants.LOCALHOST_NAME) ServerForm.ServerIPAddressString = SharedConstants.LOCALHOST_ADDRESS;
            else ServerForm.ServerIPAddressString = addressList.Text;
            ServerForm.ServerPortNumber = numPort.Value;
            ServerForm.StartWithDevicesConnected = chkAutoConnect.Checked;
            ServerForm.AccessLogEnabled = chkAccessLog.Checked;
            ServerForm.TraceState = chkTrace.Checked;
            ServerForm.DebugTraceState = chkDebugTrace.Checked;
            ServerForm.ManagementInterfaceEnabled = chkManagementInterfaceEnabled.Checked;
            ServerForm.StartWithApiEnabled = ChkStartWithApiEnabled.Checked;
            ServerForm.RunDriversOnSeparateThreads = ChkRunDriversInSeparateThreadss.Checked;
            ServerForm.LogClientIPAddress = ChkLogClientIPAddress.Checked;
            ServerForm.IncludeDriverExceptionInJsonResponse = ChkIncludeDriverExceptionsInJsonResponses.Checked;

            foreach (ServedDevice item in this.Controls.OfType<ServedDevice>())
            {
                ServerForm.ConfiguredDevices[item.Name].DeviceType = item.DeviceType;
                ServerForm.ConfiguredDevices[item.Name].ProgID = item.ProgID;
                ServerForm.ConfiguredDevices[item.Name].Description = item.Description;
                ServerForm.ConfiguredDevices[item.Name].DeviceNumber = item.DeviceNumber;
                ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetFalse = item.AllowConnectedSetFalse;
                ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetTrue = item.AllowConnectedSetTrue;
            }

            ServerForm.WriteProfile();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        #endregion

        #region Utility methods

        public void RecalculateDeviceNumbers()
        {
            // Add a zero entry for each device type on this system
            deviceNumberIndexes.Clear();
            foreach (string device in registeredDeviceTypes)
            {
                deviceNumberIndexes.Add(device, 0);
            }
            ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Initialised device numbers");

            foreach (string deviceType in registeredDeviceTypes)
            {
                ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Processing device type: " + deviceType);
                SortedDictionary<string, ServedDevice> servedDevices = new SortedDictionary<string, ServedDevice>();
                foreach (ServedDevice c in this.Controls.OfType<ServedDevice>().Where(asd => asd.DeviceType == deviceType))
                {
                    servedDevices.Add(c.Name, c);
                }
                ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Added served devices");
                Dictionary<string, string> x = new Dictionary<string, string>();

                foreach (KeyValuePair<string, ServedDevice> item in servedDevices)
                {
                    ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Processing item number: " + item.Value.Name + " ");
                    if (item.Value.DeviceType == deviceType)
                    {
                        ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Setting " + deviceType + " item number: " + deviceNumberIndexes[deviceType].ToString());
                        item.Value.DeviceNumber = deviceNumberIndexes[deviceType];
                        deviceNumberIndexes[deviceType] += 1;
                    }
                }
            }
        }

        public bool ValidIPAddress(string emailAddress, out string errorMessage)
        {
            if (addressList.Text.ToLower() == SharedConstants.LOCALHOST_NAME)
            {
                errorMessage = "";
                return true;
            }

            if (addressList.Text.ToLower() == "*")
            {
                errorMessage = "";
                return true;
            }

            if (addressList.Text.ToLower() == "+")
            {
                errorMessage = "";
                return true;
            }

            if (Regex.Matches(addressList.Text, @"\.").Count != 3)
            {
                errorMessage = "The IP address must have the form W.X.Y.Z";
                return false;
            }

            bool isValidIpAddress = IPAddress.TryParse(addressList.Text, out IPAddress testAddress);
            if (isValidIpAddress)
            {
                errorMessage = "";
                return true;
            }
            else
            {
                errorMessage = "Address given is not a valid IP address.";
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Called when debug trace is enabled to make sure that normal logging is also enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkDebugTrace_CheckedChanged(object sender, EventArgs e)
        {
            // If debug logging is requested, make sure that normal logging is enabled!
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked) chkTrace.Checked = true;
        }
    }
}
