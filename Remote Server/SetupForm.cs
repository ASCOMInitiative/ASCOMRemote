﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;
using ASCOM.Com;
using ASCOM.Common;

namespace ASCOM.Remote
{
    public partial class SetupForm : Form
    {

        #region Variables

        private readonly List<string> registeredDeviceTypes = new();
        // Create a dictionary to hold the current device instance numbers of every device type
        private Dictionary<string, int> deviceNumberIndexes;

        private List<ServedDevice> deviceList; //List of served device controls

        private bool selectByMouse = false; // Variable to help select the whole contents of a numeric up-down box when tabbed into our selected by mouse

        // CORS data grid view presentation variables
        private readonly BindingSource bindingSource = new(); // Binding source to connect the List of permitted origins to the data grid view control

        private readonly ToolStripMenuItem insertRow = new(); // Tool strip menu items for the context menu entries
        private readonly ToolStripMenuItem insertTenRows = new();
        private readonly ToolStripMenuItem deleteSelectedRows = new();

        private int currentRowIndex; // Variable to hold the current row index during row inserts

        private DataGridViewSelectedRowCollection selectedRows; // Collections to hold selected rows and cells for use when deleting origins
        private DataGridViewSelectedCellCollection selectedCells;

        private readonly List<StringValue> corsPermittedOriginsCopy = new(); // Variable to hold a copy of the list of permitted origins so that it can be edited without affecting the master copy.

        private bool alreadyDisposed = false;
        private bool maxDevicesHasChanged;

        private ConfigurationManager configurationManager;

        #endregion

        #region Setup and form load

        public SetupForm()
        {
            InitializeComponent();

            HideTabControlBorders tabControl = new(SetupTabControl); // Apply special drawing handler to the tab control in order to suppress white boarders that appear in the default control

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

            // Add a handler for changes in the minimisation behaviour combo box
            cmbMinimiseOptions.SelectedIndexChanged += CmbMinimiseOptions_SelectedIndexChanged;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Start");

                // Set up the GUI components
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
                NumMaxDevices.Value = ServerForm.MaximumNumberOfDevices;
                DlgSetLogFolderPath.SelectedPath = ServerForm.TraceFolder;
                DlgSetLogFolderPath.Description = "Select the Remote Server Log File Folder (Default: Documents\\ASCOM)";
                ChkRollOverLogs.Checked = ServerForm.RolloverLogsEnabled;
                DateTimeLogRolloverTime.Value = ServerForm.RolloverTime;
                SetRolloverTimeControlState();
                ChkUseUtcTime.Checked = ServerForm.UseUtcTimeInLogs;
                chkConfirmExit.Checked = ServerForm.ConfirmExit;
                chkStartMinimised.Checked = ServerForm.StartMinimised;
                ChkCheckForUpdates.Checked = ServerForm.CheckForUpdates;
                ChkCheckForPreReleaseUpdates.Checked = ServerForm.CheckForPreReleaseUpdates;
                chkSuppressConformationOnWindowsClose.Checked = ServerForm.SuppressConfirmationOnWindowsClose;
                chkSuppressConformationOnWindowsClose.Enabled = chkConfirmExit.Checked;
                ChkEnableReboot.Checked = ServerForm.EnableReboot;

                // Initialise the application minimise options combo box
                cmbMinimiseOptions.Items.AddRange(new object[] { ServerForm.MINIMISE_TO_SYSTEM_TRAY_KEY, ServerForm.MINIMISE_TO_TASK_BAR_KEY });
                if (ServerForm.MinimiseToSystemTray) // Minimise to system tray
                {
                    cmbMinimiseOptions.SelectedItem = ServerForm.MINIMISE_TO_SYSTEM_TRAY_KEY;
                    lblMinimisationBehaviour.Text = ServerForm.MINIMISE_TO_SYSTEM_TRAY_DESCRIPTION;
                }
                else // Minimise to task bar
                {
                    cmbMinimiseOptions.SelectedItem = ServerForm.MINIMISE_TO_TASK_BAR_KEY;
                    lblMinimisationBehaviour.Text = ServerForm.MINIMISE_TO_TASK_BAR_DESCRIPTION;
                }

                // Set the IP v4 / v6 radio boxes
                if (ServerForm.IpV4Enabled & ServerForm.IpV6Enabled) // Both IPv4 and v6 are enabled so set the "both" button
                {
                    RadIpV4AndV6.Checked = true;
                }
                else // Only one of v4 or v6 is enabled so set accordingly 
                {
                    RadIpV4.Checked = ServerForm.IpV4Enabled;
                    RadIpV6.Checked = ServerForm.IpV6Enabled;
                }

                // Populate the address list combo box  
                PopulateAddressList();

                // CORS tab event handler
                ChkEnableCors_CheckedChanged(ChkEnableCors, new EventArgs()); // Fire the event handlers to ensure that the controls reflect the CORS enabled / disabled state
                DataGridCorsOrigins_EnabledChanged(DataGridCorsOrigins, new EventArgs());

                using (Profile profile = new())
                {
                    // Populate the device types list
                    foreach (string deviceType in Devices.DeviceTypeNames())
                    {
                        //ServerForm.LogMessage(0, 0, 0, "SetupForm Load", "Adding device type: " + deviceType);
                        registeredDeviceTypes.Add(deviceType); // Remember the device types on this system
                    }
                }
                ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("Number of configured devices: {0}.", ServerForm.ConfiguredDevices.Count));

                //foreach (string deviceName in ServerForm.ConfiguredDevices.Keys)
                //{
                //    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", string.Format("ConfiguredDevices contains key {0}.", deviceName));
                //}

                // Populate the device list with all configured controls
                deviceList = WalkControls(this, new List<ServedDevice>());

                // Initialise each of the device GUI components
                foreach (ServedDevice item in deviceList)
                {
                    string devicenumberString = item.Name.Substring("ServedDevice".Length);
                    ServerForm.LogMessage(0, 0, 0, "SetupForm Load", $"Init - Found device {item.Name} - {devicenumberString}");
                    if (int.Parse(item.Name.Substring("ServedDevice".Length)) < ServerForm.MaximumNumberOfDevices)   //string.Compare(item.Name, $"ServedDevice{ServerForm.MaximumNumberOfDevices - 1}") <= 0)         //item.Name <= $"servedDevice{ServerForm.MaximumNumberOfDevices-1}")
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
                }

                switch (ServerForm.MaximumNumberOfDevices)
                {
                    case 10:
                        DeviceTabs.TabPages.Remove(DeviceTab1);
                        DeviceTabs.TabPages.Remove(DeviceTab2);
                        DeviceTabs.TabPages.Remove(DeviceTab3);
                        DeviceTabs.TabPages.Remove(DeviceTab4);
                        DeviceTabs.TabPages.Remove(DeviceTab5);
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 20:
                        DeviceTabs.TabPages.Remove(DeviceTab2);
                        DeviceTabs.TabPages.Remove(DeviceTab3);
                        DeviceTabs.TabPages.Remove(DeviceTab4);
                        DeviceTabs.TabPages.Remove(DeviceTab5);
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 30:
                        DeviceTabs.TabPages.Remove(DeviceTab3);
                        DeviceTabs.TabPages.Remove(DeviceTab4);
                        DeviceTabs.TabPages.Remove(DeviceTab5);
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 40:
                        DeviceTabs.TabPages.Remove(DeviceTab4);
                        DeviceTabs.TabPages.Remove(DeviceTab5);
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 50:
                        DeviceTabs.TabPages.Remove(DeviceTab5);
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 60:
                        DeviceTabs.TabPages.Remove(DeviceTab6);
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 70:
                        DeviceTabs.TabPages.Remove(DeviceTab7);
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 80:
                        DeviceTabs.TabPages.Remove(DeviceTab8);
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 90:
                        DeviceTabs.TabPages.Remove(DeviceTab9);
                        break;
                    case 100:
                        break;
                }

                RecalculateDeviceNumbers();

                // Add this event handler after the initial value has been set so that this doesn't trigger an event
                this.NumMaxDevices.ValueChanged += new System.EventHandler(this.NumMaxDevices_ValueChanged);

                // Set configuration handled by the configuration manager 
                configurationManager = new(null);
                ChkRunAs64BitApplication.Checked = configurationManager.Settings.RunAs64Bit;

                // Enable or disable this option depending on whether or not we are running on a 64bit OS
                if (Environment.Is64BitOperatingSystem)
                    ChkRunAs64BitApplication.Enabled = true;
                else
                    ChkRunAs64BitApplication.Enabled = false;

            }
            catch (Exception ex)
            {
                ServerForm.LogException(0, 0, 0, "SetupForm Load", string.Format("Exception on loading form: {0}.", ex.ToString()));
                MessageBox.Show(string.Format("Setup exception: {0}\r\nThe form may not function correctly.", ex.Message), "Setup form load error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Create a list of valid IP addresses on this PC so that the user can select one on which to run the ASCOM device server.
        /// </summary>
        private void PopulateAddressList()
        {
            bool foundAnIPAddress = false;
            bool foundTheIPAddress = false;
            int selectedIndex = 0;

            ServerForm.LogMessage(0, 0, 0, "PopulateAddressList", "Start");

            addressList.Items.Clear();
            deviceNumberIndexes = new Dictionary<string, int>(); // Create a dictionary to hold the current device instance numbers of every device type

            // Add IPv4 addresses
            if (RadIpV4.Checked | RadIpV4AndV6.Checked) // IPv4 addresses are required
            {
                // Add a local host entry
                addressList.Items.Add(SharedConstants.LOCALHOST_NAME_IPV4); // Make "localhost" the first entry in the list of IPv4 addresses
                foreach (IPAddress ipAddress in HostPc.IpV4Addresses)
                {
                    addressList.Items.Add(ipAddress.ToString());
                    ServerForm.LogMessage(0, 0, 0, "PopulateAddressList", string.Format("  Added {0} Address: {1}", ipAddress.AddressFamily.ToString(), ipAddress.ToString()));

                    foundAnIPAddress = true;

                    if (ipAddress.ToString() == ServerForm.ServerIPAddressString)
                    {
                        selectedIndex = addressList.Items.Count - 1;
                        foundTheIPAddress = true;
                    }
                }
            }

            // Add IPv6 addresses
            if (RadIpV6.Checked | RadIpV4AndV6.Checked) // IPv6 addresses are required
            {
                foreach (IPAddress ipAddress in HostPc.IpV6Addresses)
                {
                    addressList.Items.Add($"[{ipAddress}]");
                    ServerForm.LogMessage(0, 0, 0, "PopulateAddressList", string.Format("  Added {0} Address: {1}", ipAddress.AddressFamily.ToString(), ipAddress.ToString()));

                    foundAnIPAddress = true;

                    if ($"[{ipAddress}]" == ServerForm.ServerIPAddressString)
                    {
                        selectedIndex = addressList.Items.Count - 1;
                        foundTheIPAddress = true;
                    }
                }
            }

            ServerForm.LogMessage(0, 0, 0, "PopulateAddressList", string.Format($"Found an IP address: {foundAnIPAddress}, Found the IP address: {foundTheIPAddress}, Stored IP Address: {ServerForm.ServerIPAddressString}"));

            if ((!foundTheIPAddress) & (ServerForm.ServerIPAddressString != "")) // Add the last stored IP address if it isn't found in the search above
            {
                if (ServerForm.ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG) // Handle the "Strong bind all addresses" special case
                {
                    addressList.Items.Add(SharedConstants.BIND_TO_ALL_INTERFACES_DESCRIPTION); // Add the "All interfaces" description to the list
                    selectedIndex = addressList.Items.Count - 1; // Select this item in the list
                }
                else if (ServerForm.ServerIPAddressString == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_WEAK) // Handle the "Weak bind all addresses" special case
                {
                    addressList.Items.Add(SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_WEAK); // Add the "Weak bind" * character to the list
                    selectedIndex = addressList.Items.Count - 1; // Select this item in the list
                }
                else  // One specific address so add it if it parses OK
                {
                    IPAddress serverIpAddress = IPAddress.Parse(ServerForm.ServerIPAddressString);
                    if (((serverIpAddress.AddressFamily == AddressFamily.InterNetwork) & ((RadIpV4.Checked | RadIpV4AndV6.Checked))) |
                        ((serverIpAddress.AddressFamily == AddressFamily.InterNetworkV6) & ((RadIpV6.Checked | RadIpV4AndV6.Checked)))) // Address parses OK so add it
                    {
                        addressList.Items.Add(ServerForm.ServerIPAddressString); // Add the stored address to the list
                        selectedIndex = addressList.Items.Count - 1; // Select this item in the list
                    }
                    else // Address does not parse so ignore it, should not occur because IP addresses are validated on entry through the Setup GUI
                    {
                        selectedIndex = 0;
                    }
                }
            }

            // Include the "All interfaces" name at the end of the list of addresses if not already in use
            if (ServerForm.ServerIPAddressString != SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG) addressList.Items.Add(SharedConstants.BIND_TO_ALL_INTERFACES_DESCRIPTION);

            addressList.SelectedIndex = selectedIndex;
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
                // Save the selected IP address
                if (addressList.Text == SharedConstants.LOCALHOST_NAME_IPV4) // Handle IPV4 localhost address special case
                {
                    ServerForm.ServerIPAddressString = SharedConstants.LOCALHOST_ADDRESS_IPV4;
                }
                else if (addressList.Text == SharedConstants.BIND_TO_ALL_INTERFACES_DESCRIPTION) // Handle "All IP addresses" special case
                {
                    ServerForm.ServerIPAddressString = SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG;
                }
                else // Handle all other IP addresses
                {
                    ServerForm.ServerIPAddressString = addressList.Text;
                }
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
                ServerForm.MaximumNumberOfDevices = (int)NumMaxDevices.Value;
                ServerForm.TraceFolder = DlgSetLogFolderPath.SelectedPath;
                ServerForm.RolloverLogsEnabled = ChkRollOverLogs.Checked;
                ServerForm.RolloverTime = DateTimeLogRolloverTime.Value;
                ServerForm.UseUtcTimeInLogs = ChkUseUtcTime.Checked;
                ServerForm.ConfirmExit = chkConfirmExit.Checked;
                ServerForm.StartMinimised = chkStartMinimised.Checked;
                ServerForm.CheckForUpdates = ChkCheckForUpdates.Checked;
                ServerForm.CheckForPreReleaseUpdates = ChkCheckForPreReleaseUpdates.Checked;
                ServerForm.SuppressConfirmationOnWindowsClose = chkSuppressConformationOnWindowsClose.Checked;
                ServerForm.EnableReboot = ChkEnableReboot.Checked;

                // Update the minimise to system tray value
                ServerForm.MinimiseToSystemTray = (string)cmbMinimiseOptions.SelectedItem == ServerForm.MINIMISE_TO_SYSTEM_TRAY_KEY; // Expression evaluates to True if minimise to tray is selected, otherwise false

                // Set the IP v4 and v6 variables as necessary
                if (RadIpV4.Checked) // The IPv4 radio button is checked so set the IP v4 and IP v6 variables accordingly
                {
                    ServerForm.IpV4Enabled = true;
                    ServerForm.IpV6Enabled = false;
                }
                if (RadIpV6.Checked) // The IPv6 radio button is checked so set the IP v4 and IP v6 variables accordingly
                {
                    ServerForm.IpV4Enabled = false;
                    ServerForm.IpV6Enabled = true;
                }
                if (RadIpV4AndV6.Checked) // The IPv4 and IPV6 radio button is checked so set the IP v4 and IP v6 variables accordingly
                {
                    ServerForm.IpV4Enabled = true;
                    ServerForm.IpV6Enabled = true;
                }

                foreach (ServedDevice item in deviceList)
                {
                    ServerForm.ConfiguredDevices[item.Name].DeviceType = item.DeviceType;

                    // Update the unique ID if the ProgID has changed
                    if (ServerForm.ConfiguredDevices[item.Name].ProgID != item.ProgID)
                    {
                        if (item.ProgID == SharedConstants.DEVICE_NOT_CONFIGURED) // Device has been de-configured
                        {
                            ServerForm.ConfiguredDevices[item.Name].UniqueID = SharedConstants.DEVICE_NOT_CONFIGURED;
                        }
                        else // Device has been changed so create a new unique ID
                        {
                            ServerForm.ConfiguredDevices[item.Name].UniqueID = Guid.NewGuid().ToString().ToUpperInvariant();
                        }
                    }

                    ServerForm.ConfiguredDevices[item.Name].ProgID = item.ProgID;
                    ServerForm.ConfiguredDevices[item.Name].Description = item.Description;
                    ServerForm.ConfiguredDevices[item.Name].DeviceNumber = item.DeviceNumber;
                    ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetFalse = item.AllowConnectedSetFalse;
                    ServerForm.ConfiguredDevices[item.Name].AllowConnectedSetTrue = item.AllowConnectedSetTrue;
                    ServerForm.ConfiguredDevices[item.Name].AllowConcurrentAccess = item.AllowConcurrentAccess;
                }

                ServerForm.CorsPermittedOrigins = corsPermittedOriginsCopy.ToListString(); // Copy the edited list back to the master copy

                ServerForm.WriteProfile();

                if (maxDevicesHasChanged) MessageBox.Show("The maximum number of devices has changed, please close and restart the Remote Server before adding further devices.", "Maximum Number of Devices", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Save the configuration manager settings
                configurationManager.Save();
                configurationManager.Dispose();

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ServerForm.LogException(0, 0, 0, "OK Button", string.Format("Exception on closing form: {0}.", ex.ToString()));
            }
        }

        private void NumMaxDevices_ValueChanged(object sender, EventArgs e)
        {
            maxDevicesHasChanged = true;

            ServerForm.MaximumNumberOfDevices = (int)NumMaxDevices.Value;
            ServerForm.ReadProfile();
        }

        private void RadIpV4_CheckedChanged(object sender, EventArgs e)
        {
            PopulateAddressList();
        }

        private void RadIpV6_CheckedChanged(object sender, EventArgs e)
        {
            PopulateAddressList();
        }

        private void RadIpV4AndV6_CheckedChanged(object sender, EventArgs e)
        {
            PopulateAddressList();
        }

        private void ChkEnableDiscovery_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkEnableDiscovery.Checked)
            {
                chkManagementInterfaceEnabled.Checked = true;
                chkManagementInterfaceEnabled.Enabled = false;
            }
            else
            {
                chkManagementInterfaceEnabled.Enabled = true;
            }
        }

        private void BtnSelectLogFileFolder_Click(object sender, EventArgs e)
        {
            DlgSetLogFolderPath.ShowDialog();
        }

        private void ChkRollOverLogs_CheckedChanged(object sender, EventArgs e)
        {
            SetRolloverTimeControlState();
        }

        /// <summary>
        /// Handler for changes in the minimisation combo box selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CmbMinimiseOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Determine what to do based on the new selected item
            if (cmbMinimiseOptions.SelectedItem.ToString() == ServerForm.MINIMISE_TO_SYSTEM_TRAY_KEY) // Minimise to system tray has been selected
            {
                lblMinimisationBehaviour.Text = ServerForm.MINIMISE_TO_SYSTEM_TRAY_DESCRIPTION; // Update the option description with the minimise to system tray description
            }
            else // Minimise to task bar has been selected
            {
                lblMinimisationBehaviour.Text = ServerForm.MINIMISE_TO_TASK_BAR_DESCRIPTION; // Update the option description with the minimise to task bar description 
            }
        }

        #endregion

        #region Utility methods

        public static List<ServedDevice> WalkControls(Control TopControl, List<ServedDevice> deviceList)
        {
            //ServerForm.LogMessage(0, 0, 0, "WalkControls", $"Top control: {TopControl.Name}");

            foreach (Control control in TopControl.Controls)
            {
                if (control.HasChildren)
                {
                    if (control is ServedDevice)
                    {
                        if (int.Parse(control.Name.Substring("ServedDevice".Length)) < ServerForm.MaximumNumberOfDevices)
                        {
                            deviceList.Add(control as ServedDevice);
                            //ServerForm.LogMessage(0, 0, 0, "WalkControls", $"Found served device: {control.Name}");
                        }
                        else
                        {
                            //ServerForm.LogMessage(0, 0, 0, "WalkControls", $"Ignoring served device: {control.Name}");
                        }
                    }
                    else
                    {
                        //ServerForm.LogMessage(0, 0, 0, "WalkControls", $"Found control with children: {x.Name}. Recursing down...");
                        WalkControls(control, deviceList);
                    }
                }
            }

            return deviceList;
        }

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
                SortedDictionary<string, ServedDevice> servedDevices = new();
                foreach (ServedDevice c in deviceList) //.Where(device => device.DeviceType == deviceType))
                {
                    if (c.DeviceType == deviceType)
                    {
                        servedDevices.Add(c.Name, c);
                        ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", $"Added {c.Name}");
                    }
                }
                ServerForm.LogMessage(0, 0, 0, "RecalculateDeviceNumbers", "Added served devices");
                Dictionary<string, string> x = new();

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

        public static bool ValidIPAddress(string ipAddress, out string errorMessage)
        {
            if (string.IsNullOrEmpty(ipAddress.Trim()))
            {
                errorMessage = "The IP field has no content";
                return false;
            }

            if (ipAddress.ToLower() == SharedConstants.LOCALHOST_NAME_IPV4)
            {
                errorMessage = "";
                return true;
            }

            if (ipAddress == SharedConstants.BIND_TO_ALL_INTERFACES_DESCRIPTION)
            {
                errorMessage = "";
                return true;
            }

            if (ipAddress == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_WEAK)
            {
                errorMessage = "";
                return true;
            }

            if (ipAddress == SharedConstants.BIND_TO_ALL_INTERFACES_IP_ADDRESS_STRONG)
            {
                errorMessage = "";
                return true;
            }

            bool isValidIpAddress = IPAddress.TryParse(ipAddress, out _); // Try and parse the IP address discarding the output (out _)
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

        private void SetRolloverTimeControlState()
        {
            if (ChkRollOverLogs.Checked)
            {
                DateTimeLogRolloverTime.Enabled = true;
                LblLogRolloverTime.Enabled = true;
            }
            else
            {
                DateTimeLogRolloverTime.Enabled = false;
                LblLogRolloverTime.Enabled = false;
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
                ContextMenuStrip strip = new();
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

        private void ChkRunAs64BitApplication_CheckedChanged(object sender, EventArgs e)
        {
            configurationManager.Settings.RunAs64Bit = ((CheckBox)sender).Checked;
        }

        private void chkConfirmExit_CheckedChanged(object sender, EventArgs e)
        {
            chkSuppressConformationOnWindowsClose.Enabled = chkConfirmExit.Checked;
        }
    }
}