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
using System.Drawing;

namespace ASCOM.Remote
{
    public partial class SetupForm : Form
    {

        #region Variables

        private List<string> registeredDeviceTypes = new List<string>();
        // Create a dictionary to hold the current device instance numbers of every device type
        private Dictionary<string, int> deviceNumberIndexes;

        private bool selectByMouse = false; // Variable to help select the whole contents of a numeric up-down box when tabbed into our selected by mouse

        // CORS data grid view presentation variables
        private BindingSource bindingSource = new BindingSource(); // Binding source to connect the List of permitted origins to the data grid view control

        private ToolStripMenuItem insertRow = new ToolStripMenuItem(); // Tool strip menu items for the context menu entries
        private ToolStripMenuItem insertTenRows = new ToolStripMenuItem();
        private ToolStripMenuItem deleteSelectedRows = new ToolStripMenuItem();

        private int currentRowIndex; // Variable to hold the current row index during row inserts

        private DataGridViewSelectedRowCollection selectedRows; // Collections to hold selected rows and cells for use when deleting origins
        private DataGridViewSelectedCellCollection selectedCells;

        private List<StringValue> corsPermittedOriginsCopy = new List<StringValue>(); // Variable to hold a copy of the list of permitted origins so that it can be edited without affecting the master copy.

        private bool alreadyDisposed = false;

        #endregion

        #region Setup and form load

        public SetupForm()
        {
            InitializeComponent();

            HideTabControlBorders tabControl = new HideTabControlBorders(SetupTabControl); // Apply special drawing handler to the tab control in order to suppress white boarders that appear in the default control

            addressList.Validating += AddressList_Validating; // Add event handlers for IP address validation events
            chkTrace.CheckedChanged += ChkTrace_CheckedChanged;

            // Create event handlers to select the whole contents of the device numeric up down boxes when tabbed into or selected by mouse click
            numPort.Enter += NumericUpDown_Enter;
            numPort.MouseDown += NumericUpDown_MouseDown;

            // CORS origins enable and edit event handlers
            ChkEnableCors.CheckedChanged += ChkEnableCors_CheckedChanged;
            DataGridCorsOrigins.CellContextMenuStripNeeded += DataGridView1_CellContextMenuStripNeeded;
            DataGridCorsOrigins.EnabledChanged += DataGridCorsOrigins_EnabledChanged;

            insertRow.Click += InsertRow_Click;
            insertTenRows.Click += InsertTenRows_Click;
            deleteSelectedRows.Click += DeleteSelectedOrigins_Click;

            // Create the right click context menu entries
            insertRow.Text = "Insert row here";
            insertTenRows.Text = "Insert 10 rows here";
            deleteSelectedRows.Text = "Delete selected origins";

            // Associate the data grid view control with a clone of the List<string> permitted origins data source
            corsPermittedOriginsCopy = ServerForm.CorsPermittedOrigins.ToListStringValue();
            bindingSource.DataSource = corsPermittedOriginsCopy;
            DataGridCorsOrigins.DataSource = bindingSource;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            // Declare local variables
            IPHostEntry host;
            bool foundAnIPAddress = false;
            bool foundTheIPAddress = false;
            int selectedIndex = 0;

            try
            {
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Start");

                host = Dns.GetHostEntry(Dns.GetHostName()); // Get an IPHostEntry so that we can get the list of IP addresses on this PC
                deviceNumberIndexes = new Dictionary<string, int>(); // Create a dictionary to hold the current device instance numbers of every device type

                // Create a list of valid IP addresses on this PC so that the user can select one on which to run the ASCOM device server.
                addressList.Items.Add(SharedConstants.LOCALHOST_NAME); // Make "localhost" the first entry in the list of addresses
                foreach (IPAddress ip in host.AddressList) // Add the other addresses on this PC
                {
                    //if ((ip.AddressFamily == AddressFamily.InterNetwork) & !foundAnIPAddress) // Only process IPv4 addresses and ignore the rest including IPv6
                    if (ip.AddressFamily == AddressFamily.InterNetwork) // Only process IPv4 addresses and ignore the rest including IPv6
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
                TxtRemoteServerLocation.Text = ServerForm.RemoteServerLocation;
                ChkEnableCors.Checked = ServerForm.CorsSupportIsEnabled; // Set the CORS enabled checkbox (this doesn't fire associated event handlers if support is disabled)
                NumCorsMaxAge.Value = ServerForm.CorsMaxAge;
                ChkCorsSupportCredentials.Checked = ServerForm.CorsCredentialsPermitted;
                ChkEnableDiscovery.Checked = ServerForm.AlpacaDiscoveryEnabled;
                NumDiscoveryPort.Value = ServerForm.AlpacaDiscoveryPort;

                // CORS tab event handler
                ChkEnableCors_CheckedChanged(ChkEnableCors, new EventArgs()); // Fire the event handlers to ensure that the controls reflect the CORS enabled / disabled state
                DataGridCorsOrigins_EnabledChanged(DataGridCorsOrigins, new EventArgs());

                using (Profile profile = new Profile())
                {
                    // Populate the device types list
                    foreach (string deviceType in profile.RegisteredDeviceTypes)
                    {
                        ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Adding device type: " + deviceType);
                        registeredDeviceTypes.Add(deviceType); // Remember the device types on this system
                    }
                }
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Number of configured devices: {0}.", ServerForm.ConfiguredDevices.Count));
                foreach (string deviceName in ServerForm.ConfiguredDevices.Keys)
                {
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("ConfiguredDevices contains key {0}.", deviceName));
                }

                // Initialise each of the device GUI components
                foreach (ServedDevice item in DeviceConfigurationTab.Controls.OfType<ServedDevice>())
                {
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Starting Init for {0}.", item.Name));
                    item.InitUI(this);
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Completed Init for {0}, now setting its parameters.", item.Name));
                    item.DeviceType = ServerForm.ConfiguredDevices[item.Name].DeviceType;
                    item.ProgID = ServerForm.ConfiguredDevices[item.Name].ProgID;
                    item.DeviceNumber = ServerForm.ConfiguredDevices[item.Name].DeviceNumber;
                    item.AllowConnectedSetFalse = ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetFalse;
                    item.AllowConnectedSetTrue = ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetTrue;
                    item.AllowConcurrentAccess = ServerForm.ConfiguredDevices[item.Name].AllowConcurrentAccess;
                    item.DevicesAreConnected = ServerForm.devicesAreConnected;

                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Completed Init for {0}.", item.Name));
                }

                RecalculateDeviceNumbers();
            }
            catch (Exception ex)
            {
                ServerForm.LogException(0, 0, 0, "SetupForm Load", string.Format("Exception on loading form: {0}.", ex.ToString()));
                MessageBox.Show(string.Format("Setup exception: {0}\r\nThe form may not function correctly.", ex.Message), "Setup form load error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing && (!alreadyDisposed))
            {
                alreadyDisposed = true;
                bindingSource.Dispose();
                insertRow.Dispose();
                insertTenRows.Dispose();
                deleteSelectedRows.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// Handler for colouring the CORS data grid control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridCorsOrigins_EnabledChanged(object sender, EventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (!dgv.Enabled)
            {
                dgv.DefaultCellStyle.BackColor = SystemColors.Control;
                dgv.DefaultCellStyle.ForeColor = SystemColors.GrayText;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.GrayText;
                dgv.CurrentCell = null;
                dgv.ReadOnly = true;
                dgv.EnableHeadersVisualStyles = false;
                dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Control;
                dgv.DefaultCellStyle.SelectionForeColor = SystemColors.GrayText;
            }
            else
            {
                dgv.DefaultCellStyle.BackColor = SystemColors.Window;
                dgv.DefaultCellStyle.ForeColor = SystemColors.ControlText;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Window;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.Highlight; ;
                dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
                dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
                dgv.ReadOnly = false;
                dgv.CurrentCell = dgv.Rows[0].Cells[0];
            }
        }

        /// <summary>
        /// Handler for changes to the CORS enable state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkEnableCors_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked)
            {
                DataGridCorsOrigins.Enabled = true;
                NumCorsMaxAge.Enabled = true;
                LabHelp1.ForeColor = SystemColors.Highlight;
                LabHelp2.ForeColor = SystemColors.Highlight;
                LabHelp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                LabHelp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                LabMaxAge.ForeColor = SystemColors.ControlText;
                ChkCorsSupportCredentials.Enabled = true;
            }
            else
            {
                DataGridCorsOrigins.Enabled = false;
                NumCorsMaxAge.Enabled = false;
                LabHelp1.ForeColor = SystemColors.GrayText;
                LabHelp2.ForeColor = SystemColors.GrayText;
                LabHelp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                LabHelp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                LabMaxAge.ForeColor = SystemColors.GrayText;
                ChkCorsSupportCredentials.Enabled = false;
            }
        }

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
        /// Called when debug trace is enabled to make sure that normal logging is also enabled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChkDebugTrace_CheckedChanged(object sender, EventArgs e)
        {
            // If debug logging is requested, make sure that normal logging is enabled!
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.Checked) chkTrace.Checked = true;
        }

        /// <summary>
        /// Called when the user presses the OK button to commit any new set up values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
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
                ServerForm.RemoteServerLocation = TxtRemoteServerLocation.Text;
                ServerForm.CorsSupportIsEnabled = ChkEnableCors.Checked;
                ServerForm.CorsMaxAge = NumCorsMaxAge.Value;
                ServerForm.CorsCredentialsPermitted = ChkCorsSupportCredentials.Checked;
                ServerForm.AlpacaDiscoveryEnabled = ChkEnableDiscovery.Checked;
                ServerForm.AlpacaDiscoveryPort = NumDiscoveryPort.Value;

                foreach (ServedDevice item in DeviceConfigurationTab.Controls.OfType<ServedDevice>())
                {
                    ServerForm.ConfiguredDevices[item.Name].DeviceType = item.DeviceType;
                    ServerForm.ConfiguredDevices[item.Name].ProgID = item.ProgID;
                    ServerForm.ConfiguredDevices[item.Name].Description = item.Description;
                    ServerForm.ConfiguredDevices[item.Name].DeviceNumber = item.DeviceNumber;
                    ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetFalse = item.AllowConnectedSetFalse;
                    ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetTrue = item.AllowConnectedSetTrue;
                    ServerForm.ConfiguredDevices[item.Name].AllowConcurrentAccess = item.AllowConcurrentAccess;
                }

                ServerForm.CorsPermittedOrigins = corsPermittedOriginsCopy.ToListString(); // Copy the edited list back to the master copy

                ServerForm.WriteProfile();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ServerForm.LogException(0, 0, 0, "OK Button", string.Format("Exception on closing form: {0}.", ex.ToString()));
            }
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
                foreach (ServedDevice c in DeviceConfigurationTab.Controls.OfType<ServedDevice>().Where(device => device.DeviceType == deviceType))
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

        public bool ValidIPAddress(string ipAddress, out string errorMessage)
        {
            if (string.IsNullOrEmpty(ipAddress.Trim()))
            {
                errorMessage = "The IP field has no content";
                return false;
            }

            if (ipAddress.ToLower() == SharedConstants.LOCALHOST_NAME)
            {
                errorMessage = "";
                return true;
            }

            if (ipAddress.ToLower() == "*")
            {
                errorMessage = "";
                return true;
            }

            if (ipAddress.ToLower() == "+")
            {
                errorMessage = "";
                return true;
            }

            if (Regex.Matches(ipAddress, @"\.").Count != 3)
            {
                errorMessage = "The IP address must have the form W.X.Y.Z";
                return false;
            }

            bool isValidIpAddress = IPAddress.TryParse(ipAddress, out _); // Try and parse the Ip address discarding the output (out _)
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

        #region CORS SUpport
        /// <summary>
        /// Insert a single blank row into the CORS approved origins list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertRow_Click(object sender, EventArgs e)
        {
            bindingSource.Insert(currentRowIndex, new StringValue("")); // Insert a single row containing an empty string
        }

        /// <summary>
        /// Insert 10 blank rows into the CORS approved origins list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InsertTenRows_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= 9; i++) // Insert 10 rows containing empty strings
            {
                bindingSource.Insert(currentRowIndex, new StringValue(""));
            }
        }

        /// <summary>
        /// Delete the selected rows from the list of approved origins
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSelectedOrigins_Click(object sender, EventArgs e)
        {
            // Delete any selected rows
            foreach (DataGridViewRow row in selectedRows)
            {
                try
                {
                    bindingSource.RemoveAt(row.Index);
                }
                catch { }
            }

            // Delete any remaining cells that were selected but were not parts of whole selected rows.
            // Some of these may have been removed when deleting whole rows above, so ignore any exceptions here
            foreach (DataGridViewCell cell in selectedCells)
            {
                try
                {
                    bindingSource.RemoveAt(cell.RowIndex);
                }
                catch { }
            }

            // Add a single default row if all rows have been deleted
            if (bindingSource.Count == 0) bindingSource.Add(new StringValue(SharedConstants.CORS_DEFAULT_PERMISSION.ToString()));
        }

        /// <summary>
        /// Create the CORS origins list right click menu 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridView1_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            DataGridView dataGridViewControl = (DataGridView)sender;
            if (e.RowIndex >= 0) // Only show the menu in the content part of the table and not the header or row selector parts
            {
                selectedRows = dataGridViewControl.SelectedRows; // Save the currently selected rows and cells for use by the delete selected origins method
                selectedCells = dataGridViewControl.SelectedCells;

                // Create the right click context menu contents
                ContextMenuStrip strip = new ContextMenuStrip();
                strip.Items.Add(insertRow);
                strip.Items.Add(insertTenRows);

                // Only include the "delete selected rows" option if there is more than 1 row and 1 or more selected cells
                if ((dataGridViewControl.Rows.Count > 1) && ((selectedRows.Count > 0) || (selectedCells.Count > 0)))
                {
                    strip.Items.Add(deleteSelectedRows);
                }
                e.ContextMenuStrip = strip;
                currentRowIndex = e.RowIndex;
            }
        }
        #endregion

    }
}