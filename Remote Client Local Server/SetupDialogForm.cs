using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ASCOM.Remote
{
    public partial class SetupDialogForm : Form
    {

        #region Variables

        private TraceLoggerPlus TL;

        public string DriverDisplayName { get; set; }
        public string ServiceType { get; set; }
        public string IPAddressString { get; set; }
        public decimal PortNumber { get; set; }
        public decimal RemoteDeviceNumber { get; set; }
        public decimal EstablishConnectionTimeout { get; set; }
        public decimal StandardTimeout { get; set; }
        public decimal LongTimeout { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool TraceState { get; set; }
        public bool DebugTraceState { get; set; }
        public bool ManageConnectLocally { get; set; }

        private bool selectByMouse = false; // Variable to help select the whole contents of a numeric updown box when tabbed into our selected by mouse

        #endregion

        #region Initialisation and Form Load

        public SetupDialogForm()
        {
            InitializeComponent();

            // Event handlers to paint the service type drop down white rather than the default grey.
            cmbServiceType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbServiceType.DrawItem += new DrawItemEventHandler(ComboBox_DrawItem);
            cmbServiceType.DrawMode = DrawMode.OwnerDrawFixed;

            // Create event handlers to select the whole contents of the numeric updown boxes when tabbed into or selected by mouse click
            numPort.Enter += NumericUpDown_Enter;
            numPort.MouseDown += NumericUpDown_MouseDown;
            numRemoteDeviceNumber.Enter += NumericUpDown_Enter;
            numRemoteDeviceNumber.MouseDown += NumericUpDown_MouseDown;
            numEstablishCommunicationsTimeout.Enter += NumericUpDown_Enter;
            numEstablishCommunicationsTimeout.MouseDown += NumericUpDown_MouseDown;
            numStandardTimeout.Enter += NumericUpDown_Enter;
            numStandardTimeout.MouseDown += NumericUpDown_MouseDown;
            numLongTimeout.Enter += NumericUpDown_Enter;
            numLongTimeout.MouseDown += NumericUpDown_MouseDown;
        }

        public SetupDialogForm(TraceLoggerPlus TraceLogger) : this()
        {
            TL = TraceLogger;
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            TL.LogMessage("SetupForm Load", "Start");

            this.Text = DriverDisplayName + " Configuration";
            addressList.Items.Add(SharedConstants.LOCALHOST_NAME);

            cmbServiceType.Text = ServiceType;

            int selectedIndex = 0;

            if (IPAddressString != SharedConstants.LOCALHOST_NAME)
            {
                addressList.Items.Add(IPAddressString);
                selectedIndex = 1;
            }

            IPHostEntry host;
            IPAddress localIP = null;
            host = Dns.GetHostEntry(Dns.GetHostName());
            bool found = false;
            foreach (IPAddress ip in host.AddressList)
            {
                if ((ip.AddressFamily == AddressFamily.InterNetwork) & !found)
                {
                    localIP = ip;
                    TL.LogMessage("GetIPAddress", "Found IP Address: " + ip.ToString());
                    found = true;
                    if (ip.ToString() != IPAddressString) // Only add addresses that are not the currently selected IP address
                    {
                        addressList.Items.Add(ip.ToString());
                    }
                }
                else
                {
                    TL.LogMessage("GetIPAddress", "Ignored IP Address: " + ip.ToString());
                }
            }
            if (localIP == null) throw new Exception("Cannot find IP address of this device");

            TL.LogMessage("GetIPAddress", localIP.ToString());
            addressList.SelectedIndex = selectedIndex;
            numPort.Value = PortNumber;
            numRemoteDeviceNumber.Value = RemoteDeviceNumber;
            numEstablishCommunicationsTimeout.Value = Convert.ToDecimal(EstablishConnectionTimeout);
            numStandardTimeout.Value = Convert.ToDecimal(StandardTimeout);
            numLongTimeout.Value = Convert.ToDecimal(LongTimeout);
            txtUserName.Text = UserName.Unencrypt(TL);
            txtPassword.Text = Password.Unencrypt(TL);
            chkTrace.Checked = TraceState;
            chkDebugTrace.Checked = DebugTraceState;
            if (ManageConnectLocally)
            {
                radManageConnectLocally.Checked = true;
            }
            else
            {
                radManageConnectRemotely.Checked = true;
            }

            this.BringToFront();
        }

        #endregion

        #region Event handlers

        private void BtnOK_Click(object sender, EventArgs e)
        {
            TraceState = chkTrace.Checked;
            DebugTraceState = chkDebugTrace.Checked;
            IPAddressString = addressList.Text;
            PortNumber = numPort.Value;
            RemoteDeviceNumber = numRemoteDeviceNumber.Value;
            ServiceType = cmbServiceType.Text;
            EstablishConnectionTimeout = Convert.ToInt32(numEstablishCommunicationsTimeout.Value);
            StandardTimeout = Convert.ToInt32(numStandardTimeout.Value);
            LongTimeout = Convert.ToInt32(numLongTimeout.Value);
            UserName = txtUserName.Text.Encrypt(TL); // Encrypt the provided username and password
            Password = txtPassword.Text.Encrypt(TL);
            ManageConnectLocally = radManageConnectLocally.Checked;

            this.DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Event handler to paint the device list combo box in the "DropDown" rather than "DropDownList" style
        /// </summary>
        /// <param name="sender">Device to be painted</param>
        /// <param name="e">Draw event arguments object</param>
        void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ComboBox combo = sender as ComboBox;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) // Draw the selected item in menu highlight colour
            {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.MenuHighlight), e.Bounds);
                e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, new SolidBrush(SystemColors.HighlightText), new Point(e.Bounds.X, e.Bounds.Y));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
                e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font, new SolidBrush(combo.ForeColor), new Point(e.Bounds.X, e.Bounds.Y));
            }

            e.DrawFocusRectangle();
        }

        private void BtnServerConfiguration_Click(object sender, EventArgs e)
        {
            ServerConfigurationForm configurationForm = new ServerConfigurationForm(TL, cmbServiceType.Text, addressList.Text, numPort.Value, txtUserName.Text, txtPassword.Text);
            configurationForm.ShowDialog();
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

        #endregion

    }
}
