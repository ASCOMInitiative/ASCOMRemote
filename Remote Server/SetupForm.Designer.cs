namespace ASCOM.Remote
{
    partial class SetupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkAutoConnect = new System.Windows.Forms.CheckBox();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.chkDebugTrace = new System.Windows.Forms.CheckBox();
            this.chkAccessLog = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.chkManagementInterfaceEnabled = new System.Windows.Forms.CheckBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.ChkStartWithApiEnabled = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.LblDevicesNotDisconnoected = new System.Windows.Forms.Label();
            this.ChkRunDriversInSeparateThreadss = new System.Windows.Forms.CheckBox();
            this.ChkLogClientIPAddress = new System.Windows.Forms.CheckBox();
            this.ChkIncludeDriverExceptionsInJsonResponses = new System.Windows.Forms.CheckBox();
            this.SetupTabControl = new System.Windows.Forms.TabControl();
            this.DeviceConfigurationTab = new System.Windows.Forms.TabPage();
            this.label12 = new System.Windows.Forms.Label();
            this.ServedDevice0 = new ASCOM.Remote.ServedDevice();
            this.label11 = new System.Windows.Forms.Label();
            this.ServedDevice1 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice2 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice3 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice4 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice5 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice6 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice7 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice8 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice9 = new ASCOM.Remote.ServedDevice();
            this.ServerConfigurationTab = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.addressList = new System.Windows.Forms.ComboBox();
            this.TxtRemoteServerLocation = new System.Windows.Forms.TextBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.CorsConfigurationTab = new System.Windows.Forms.TabPage();
            this.ChkCorsSupportCredentials = new System.Windows.Forms.CheckBox();
            this.LabMaxAge = new System.Windows.Forms.Label();
            this.NumCorsMaxAge = new System.Windows.Forms.NumericUpDown();
            this.ChkEnableCors = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.LabHelp2 = new System.Windows.Forms.Label();
            this.LabHelp1 = new System.Windows.Forms.Label();
            this.DataGridCorsOrigins = new System.Windows.Forms.DataGridView();
            this.ChkEnableDiscovery = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SetupTabControl.SuspendLayout();
            this.DeviceConfigurationTab.SuspendLayout();
            this.ServerConfigurationTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.CorsConfigurationTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumCorsMaxAge)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridCorsOrigins)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(972, 414);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 19;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(891, 414);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 18;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(71, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Device Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(441, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Device";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(202, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Device Number";
            // 
            // chkAutoConnect
            // 
            this.chkAutoConnect.AutoSize = true;
            this.chkAutoConnect.Location = new System.Drawing.Point(293, 167);
            this.chkAutoConnect.Name = "chkAutoConnect";
            this.chkAutoConnect.Size = new System.Drawing.Size(133, 17);
            this.chkAutoConnect.TabIndex = 2;
            this.chkAutoConnect.Text = "Auto Connect Devices";
            this.chkAutoConnect.UseVisualStyleBackColor = true;
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(293, 264);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(96, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Write Log Files";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // chkDebugTrace
            // 
            this.chkDebugTrace.AutoSize = true;
            this.chkDebugTrace.Location = new System.Drawing.Point(293, 287);
            this.chkDebugTrace.Name = "chkDebugTrace";
            this.chkDebugTrace.Size = new System.Drawing.Size(135, 17);
            this.chkDebugTrace.TabIndex = 7;
            this.chkDebugTrace.Text = "Enable Debug Logging";
            this.chkDebugTrace.UseVisualStyleBackColor = true;
            this.chkDebugTrace.CheckedChanged += new System.EventHandler(this.ChkDebugTrace_CheckedChanged);
            // 
            // chkAccessLog
            // 
            this.chkAccessLog.AutoSize = true;
            this.chkAccessLog.Location = new System.Drawing.Point(293, 190);
            this.chkAccessLog.Name = "chkAccessLog";
            this.chkAccessLog.Size = new System.Drawing.Size(124, 17);
            this.chkAccessLog.TabIndex = 4;
            this.chkAccessLog.Text = "Access Log Enabled";
            this.chkAccessLog.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(779, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Allow Connected to be set";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(799, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "False";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(859, 32);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "True";
            // 
            // chkManagementInterfaceEnabled
            // 
            this.chkManagementInterfaceEnabled.AutoSize = true;
            this.chkManagementInterfaceEnabled.Location = new System.Drawing.Point(506, 167);
            this.chkManagementInterfaceEnabled.Name = "chkManagementInterfaceEnabled";
            this.chkManagementInterfaceEnabled.Size = new System.Drawing.Size(169, 17);
            this.chkManagementInterfaceEnabled.TabIndex = 3;
            this.chkManagementInterfaceEnabled.Text = "Enable Management Interface";
            this.chkManagementInterfaceEnabled.UseVisualStyleBackColor = true;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // ChkStartWithApiEnabled
            // 
            this.ChkStartWithApiEnabled.AutoSize = true;
            this.ChkStartWithApiEnabled.Location = new System.Drawing.Point(506, 190);
            this.ChkStartWithApiEnabled.Name = "ChkStartWithApiEnabled";
            this.ChkStartWithApiEnabled.Size = new System.Drawing.Size(131, 17);
            this.ChkStartWithApiEnabled.TabIndex = 5;
            this.ChkStartWithApiEnabled.Text = "Start with API enabled";
            this.ChkStartWithApiEnabled.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(663, 32);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(89, 13);
            this.label9.TabIndex = 32;
            this.label9.Text = "Configure Device";
            // 
            // LblDevicesNotDisconnoected
            // 
            this.LblDevicesNotDisconnoected.AutoSize = true;
            this.LblDevicesNotDisconnoected.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblDevicesNotDisconnoected.ForeColor = System.Drawing.Color.Red;
            this.LblDevicesNotDisconnoected.Location = new System.Drawing.Point(574, 329);
            this.LblDevicesNotDisconnoected.Name = "LblDevicesNotDisconnoected";
            this.LblDevicesNotDisconnoected.Size = new System.Drawing.Size(257, 26);
            this.LblDevicesNotDisconnoected.TabIndex = 33;
            this.LblDevicesNotDisconnoected.Text = "Devices are Connected\r\nConfiguration requires that devices are Disconnected";
            this.LblDevicesNotDisconnoected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ChkRunDriversInSeparateThreadss
            // 
            this.ChkRunDriversInSeparateThreadss.AutoSize = true;
            this.ChkRunDriversInSeparateThreadss.Location = new System.Drawing.Point(506, 264);
            this.ChkRunDriversInSeparateThreadss.Name = "ChkRunDriversInSeparateThreadss";
            this.ChkRunDriversInSeparateThreadss.Size = new System.Drawing.Size(181, 17);
            this.ChkRunDriversInSeparateThreadss.TabIndex = 34;
            this.ChkRunDriversInSeparateThreadss.Text = "Run Drivers in Separate Threads";
            this.ChkRunDriversInSeparateThreadss.UseVisualStyleBackColor = true;
            // 
            // ChkLogClientIPAddress
            // 
            this.ChkLogClientIPAddress.AutoSize = true;
            this.ChkLogClientIPAddress.Location = new System.Drawing.Point(506, 287);
            this.ChkLogClientIPAddress.Name = "ChkLogClientIPAddress";
            this.ChkLogClientIPAddress.Size = new System.Drawing.Size(134, 17);
            this.ChkLogClientIPAddress.TabIndex = 35;
            this.ChkLogClientIPAddress.Text = "Log Client\'s IP Address";
            this.ChkLogClientIPAddress.UseVisualStyleBackColor = true;
            // 
            // ChkIncludeDriverExceptionsInJsonResponses
            // 
            this.ChkIncludeDriverExceptionsInJsonResponses.AutoSize = true;
            this.ChkIncludeDriverExceptionsInJsonResponses.Location = new System.Drawing.Point(293, 310);
            this.ChkIncludeDriverExceptionsInJsonResponses.Name = "ChkIncludeDriverExceptionsInJsonResponses";
            this.ChkIncludeDriverExceptionsInJsonResponses.Size = new System.Drawing.Size(364, 17);
            this.ChkIncludeDriverExceptionsInJsonResponses.TabIndex = 36;
            this.ChkIncludeDriverExceptionsInJsonResponses.Text = "Include driver exceptions in JSON responses (only useful for debugging)";
            this.ChkIncludeDriverExceptionsInJsonResponses.UseVisualStyleBackColor = true;
            // 
            // SetupTabControl
            // 
            this.SetupTabControl.Controls.Add(this.DeviceConfigurationTab);
            this.SetupTabControl.Controls.Add(this.ServerConfigurationTab);
            this.SetupTabControl.Controls.Add(this.CorsConfigurationTab);
            this.SetupTabControl.Location = new System.Drawing.Point(12, 12);
            this.SetupTabControl.Name = "SetupTabControl";
            this.SetupTabControl.SelectedIndex = 0;
            this.SetupTabControl.Size = new System.Drawing.Size(1035, 396);
            this.SetupTabControl.TabIndex = 39;
            // 
            // DeviceConfigurationTab
            // 
            this.DeviceConfigurationTab.BackColor = System.Drawing.SystemColors.Control;
            this.DeviceConfigurationTab.Controls.Add(this.label12);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice0);
            this.DeviceConfigurationTab.Controls.Add(this.label11);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice1);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice2);
            this.DeviceConfigurationTab.Controls.Add(this.label1);
            this.DeviceConfigurationTab.Controls.Add(this.LblDevicesNotDisconnoected);
            this.DeviceConfigurationTab.Controls.Add(this.label2);
            this.DeviceConfigurationTab.Controls.Add(this.label9);
            this.DeviceConfigurationTab.Controls.Add(this.label3);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice3);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice4);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice5);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice6);
            this.DeviceConfigurationTab.Controls.Add(this.label6);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice7);
            this.DeviceConfigurationTab.Controls.Add(this.label5);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice8);
            this.DeviceConfigurationTab.Controls.Add(this.label4);
            this.DeviceConfigurationTab.Controls.Add(this.ServedDevice9);
            this.DeviceConfigurationTab.Location = new System.Drawing.Point(4, 22);
            this.DeviceConfigurationTab.Name = "DeviceConfigurationTab";
            this.DeviceConfigurationTab.Padding = new System.Windows.Forms.Padding(3);
            this.DeviceConfigurationTab.Size = new System.Drawing.Size(1027, 370);
            this.DeviceConfigurationTab.TabIndex = 0;
            this.DeviceConfigurationTab.Text = "Device Configuration";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(943, 16);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(40, 13);
            this.label12.TabIndex = 40;
            this.label12.Text = "Enable";
            // 
            // ServedDevice0
            // 
            this.ServedDevice0.AllowConcurrentAccess = false;
            this.ServedDevice0.AllowConnectedSetFalse = false;
            this.ServedDevice0.AllowConnectedSetTrue = false;
            this.ServedDevice0.Description = "";
            this.ServedDevice0.DeviceNumber = 0;
            this.ServedDevice0.DevicesAreConnected = false;
            this.ServedDevice0.DeviceType = "None";
            this.ServedDevice0.Location = new System.Drawing.Point(23, 48);
            this.ServedDevice0.Name = "ServedDevice0";
            this.ServedDevice0.ProgID = "";
            this.ServedDevice0.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice0.TabIndex = 8;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(914, 32);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(97, 13);
            this.label11.TabIndex = 34;
            this.label11.Text = "Concurrent Access";
            // 
            // ServedDevice1
            // 
            this.ServedDevice1.AllowConcurrentAccess = false;
            this.ServedDevice1.AllowConnectedSetFalse = false;
            this.ServedDevice1.AllowConnectedSetTrue = false;
            this.ServedDevice1.Description = "";
            this.ServedDevice1.DeviceNumber = 0;
            this.ServedDevice1.DevicesAreConnected = false;
            this.ServedDevice1.DeviceType = "None";
            this.ServedDevice1.Location = new System.Drawing.Point(23, 76);
            this.ServedDevice1.Name = "ServedDevice1";
            this.ServedDevice1.ProgID = "";
            this.ServedDevice1.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice1.TabIndex = 9;
            // 
            // ServedDevice2
            // 
            this.ServedDevice2.AllowConcurrentAccess = false;
            this.ServedDevice2.AllowConnectedSetFalse = false;
            this.ServedDevice2.AllowConnectedSetTrue = false;
            this.ServedDevice2.Description = "";
            this.ServedDevice2.DeviceNumber = 0;
            this.ServedDevice2.DevicesAreConnected = false;
            this.ServedDevice2.DeviceType = "None";
            this.ServedDevice2.Location = new System.Drawing.Point(23, 104);
            this.ServedDevice2.Name = "ServedDevice2";
            this.ServedDevice2.ProgID = "";
            this.ServedDevice2.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice2.TabIndex = 10;
            // 
            // ServedDevice3
            // 
            this.ServedDevice3.AllowConcurrentAccess = false;
            this.ServedDevice3.AllowConnectedSetFalse = false;
            this.ServedDevice3.AllowConnectedSetTrue = false;
            this.ServedDevice3.Description = "";
            this.ServedDevice3.DeviceNumber = 0;
            this.ServedDevice3.DevicesAreConnected = false;
            this.ServedDevice3.DeviceType = "None";
            this.ServedDevice3.Location = new System.Drawing.Point(23, 132);
            this.ServedDevice3.Name = "ServedDevice3";
            this.ServedDevice3.ProgID = "";
            this.ServedDevice3.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice3.TabIndex = 11;
            // 
            // ServedDevice4
            // 
            this.ServedDevice4.AllowConcurrentAccess = false;
            this.ServedDevice4.AllowConnectedSetFalse = false;
            this.ServedDevice4.AllowConnectedSetTrue = false;
            this.ServedDevice4.Description = "";
            this.ServedDevice4.DeviceNumber = 0;
            this.ServedDevice4.DevicesAreConnected = false;
            this.ServedDevice4.DeviceType = "None";
            this.ServedDevice4.Location = new System.Drawing.Point(23, 160);
            this.ServedDevice4.Name = "ServedDevice4";
            this.ServedDevice4.ProgID = "";
            this.ServedDevice4.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice4.TabIndex = 12;
            // 
            // ServedDevice5
            // 
            this.ServedDevice5.AllowConcurrentAccess = false;
            this.ServedDevice5.AllowConnectedSetFalse = false;
            this.ServedDevice5.AllowConnectedSetTrue = false;
            this.ServedDevice5.Description = "";
            this.ServedDevice5.DeviceNumber = 0;
            this.ServedDevice5.DevicesAreConnected = false;
            this.ServedDevice5.DeviceType = "None";
            this.ServedDevice5.Location = new System.Drawing.Point(23, 188);
            this.ServedDevice5.Name = "ServedDevice5";
            this.ServedDevice5.ProgID = "";
            this.ServedDevice5.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice5.TabIndex = 13;
            // 
            // ServedDevice6
            // 
            this.ServedDevice6.AllowConcurrentAccess = false;
            this.ServedDevice6.AllowConnectedSetFalse = false;
            this.ServedDevice6.AllowConnectedSetTrue = false;
            this.ServedDevice6.Description = "";
            this.ServedDevice6.DeviceNumber = 0;
            this.ServedDevice6.DevicesAreConnected = false;
            this.ServedDevice6.DeviceType = "None";
            this.ServedDevice6.Location = new System.Drawing.Point(23, 216);
            this.ServedDevice6.Name = "ServedDevice6";
            this.ServedDevice6.ProgID = "";
            this.ServedDevice6.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice6.TabIndex = 14;
            // 
            // ServedDevice7
            // 
            this.ServedDevice7.AllowConcurrentAccess = false;
            this.ServedDevice7.AllowConnectedSetFalse = false;
            this.ServedDevice7.AllowConnectedSetTrue = false;
            this.ServedDevice7.Description = "";
            this.ServedDevice7.DeviceNumber = 0;
            this.ServedDevice7.DevicesAreConnected = false;
            this.ServedDevice7.DeviceType = "None";
            this.ServedDevice7.Location = new System.Drawing.Point(23, 244);
            this.ServedDevice7.Name = "ServedDevice7";
            this.ServedDevice7.ProgID = "";
            this.ServedDevice7.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice7.TabIndex = 15;
            // 
            // ServedDevice8
            // 
            this.ServedDevice8.AllowConcurrentAccess = false;
            this.ServedDevice8.AllowConnectedSetFalse = false;
            this.ServedDevice8.AllowConnectedSetTrue = false;
            this.ServedDevice8.Description = "";
            this.ServedDevice8.DeviceNumber = 0;
            this.ServedDevice8.DevicesAreConnected = false;
            this.ServedDevice8.DeviceType = "None";
            this.ServedDevice8.Location = new System.Drawing.Point(23, 272);
            this.ServedDevice8.Name = "ServedDevice8";
            this.ServedDevice8.ProgID = "";
            this.ServedDevice8.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice8.TabIndex = 16;
            // 
            // ServedDevice9
            // 
            this.ServedDevice9.AllowConcurrentAccess = false;
            this.ServedDevice9.AllowConnectedSetFalse = false;
            this.ServedDevice9.AllowConnectedSetTrue = false;
            this.ServedDevice9.Description = "";
            this.ServedDevice9.DeviceNumber = 0;
            this.ServedDevice9.DevicesAreConnected = false;
            this.ServedDevice9.DeviceType = "None";
            this.ServedDevice9.Location = new System.Drawing.Point(23, 300);
            this.ServedDevice9.Name = "ServedDevice9";
            this.ServedDevice9.ProgID = "";
            this.ServedDevice9.Size = new System.Drawing.Size(960, 22);
            this.ServedDevice9.TabIndex = 17;
            // 
            // ServerConfigurationTab
            // 
            this.ServerConfigurationTab.BackColor = System.Drawing.SystemColors.Control;
            this.ServerConfigurationTab.Controls.Add(this.ChkEnableDiscovery);
            this.ServerConfigurationTab.Controls.Add(this.label10);
            this.ServerConfigurationTab.Controls.Add(this.addressList);
            this.ServerConfigurationTab.Controls.Add(this.TxtRemoteServerLocation);
            this.ServerConfigurationTab.Controls.Add(this.numPort);
            this.ServerConfigurationTab.Controls.Add(this.label8);
            this.ServerConfigurationTab.Controls.Add(this.label7);
            this.ServerConfigurationTab.Controls.Add(this.chkManagementInterfaceEnabled);
            this.ServerConfigurationTab.Controls.Add(this.ChkIncludeDriverExceptionsInJsonResponses);
            this.ServerConfigurationTab.Controls.Add(this.chkAutoConnect);
            this.ServerConfigurationTab.Controls.Add(this.ChkLogClientIPAddress);
            this.ServerConfigurationTab.Controls.Add(this.chkTrace);
            this.ServerConfigurationTab.Controls.Add(this.ChkRunDriversInSeparateThreadss);
            this.ServerConfigurationTab.Controls.Add(this.chkDebugTrace);
            this.ServerConfigurationTab.Controls.Add(this.ChkStartWithApiEnabled);
            this.ServerConfigurationTab.Controls.Add(this.chkAccessLog);
            this.ServerConfigurationTab.Location = new System.Drawing.Point(4, 22);
            this.ServerConfigurationTab.Margin = new System.Windows.Forms.Padding(0);
            this.ServerConfigurationTab.Name = "ServerConfigurationTab";
            this.ServerConfigurationTab.Padding = new System.Windows.Forms.Padding(3);
            this.ServerConfigurationTab.Size = new System.Drawing.Size(1027, 370);
            this.ServerConfigurationTab.TabIndex = 1;
            this.ServerConfigurationTab.Text = "Server Configuration";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(408, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 13);
            this.label10.TabIndex = 44;
            this.label10.Text = "Server Location";
            // 
            // addressList
            // 
            this.addressList.FormattingEnabled = true;
            this.addressList.Location = new System.Drawing.Point(293, 96);
            this.addressList.Name = "addressList";
            this.addressList.Size = new System.Drawing.Size(328, 21);
            this.addressList.TabIndex = 39;
            // 
            // TxtRemoteServerLocation
            // 
            this.TxtRemoteServerLocation.Location = new System.Drawing.Point(293, 36);
            this.TxtRemoteServerLocation.Name = "TxtRemoteServerLocation";
            this.TxtRemoteServerLocation.Size = new System.Drawing.Size(328, 20);
            this.TxtRemoteServerLocation.TabIndex = 43;
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(627, 97);
            this.numPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(87, 20);
            this.numPort.TabIndex = 40;
            this.numPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(625, 80);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(111, 13);
            this.label8.TabIndex = 42;
            this.label8.Text = "Remote server IP Port";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(407, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(129, 13);
            this.label7.TabIndex = 41;
            this.label7.Text = "Remote server IP address";
            // 
            // CorsConfigurationTab
            // 
            this.CorsConfigurationTab.BackColor = System.Drawing.SystemColors.Control;
            this.CorsConfigurationTab.Controls.Add(this.ChkCorsSupportCredentials);
            this.CorsConfigurationTab.Controls.Add(this.LabMaxAge);
            this.CorsConfigurationTab.Controls.Add(this.NumCorsMaxAge);
            this.CorsConfigurationTab.Controls.Add(this.ChkEnableCors);
            this.CorsConfigurationTab.Controls.Add(this.label14);
            this.CorsConfigurationTab.Controls.Add(this.LabHelp2);
            this.CorsConfigurationTab.Controls.Add(this.LabHelp1);
            this.CorsConfigurationTab.Controls.Add(this.DataGridCorsOrigins);
            this.CorsConfigurationTab.Location = new System.Drawing.Point(4, 22);
            this.CorsConfigurationTab.Name = "CorsConfigurationTab";
            this.CorsConfigurationTab.Padding = new System.Windows.Forms.Padding(3);
            this.CorsConfigurationTab.Size = new System.Drawing.Size(1027, 370);
            this.CorsConfigurationTab.TabIndex = 2;
            this.CorsConfigurationTab.Text = "CORS Configuration";
            // 
            // ChkCorsSupportCredentials
            // 
            this.ChkCorsSupportCredentials.AutoSize = true;
            this.ChkCorsSupportCredentials.Location = new System.Drawing.Point(827, 166);
            this.ChkCorsSupportCredentials.Name = "ChkCorsSupportCredentials";
            this.ChkCorsSupportCredentials.Size = new System.Drawing.Size(118, 17);
            this.ChkCorsSupportCredentials.TabIndex = 10;
            this.ChkCorsSupportCredentials.Text = "Support Credentials";
            this.ChkCorsSupportCredentials.UseVisualStyleBackColor = true;
            // 
            // LabMaxAge
            // 
            this.LabMaxAge.AutoSize = true;
            this.LabMaxAge.Location = new System.Drawing.Point(495, 286);
            this.LabMaxAge.Name = "LabMaxAge";
            this.LabMaxAge.Size = new System.Drawing.Size(169, 13);
            this.LabMaxAge.TabIndex = 9;
            this.LabMaxAge.Text = "CORS Max Age Header (seconds)";
            // 
            // NumCorsMaxAge
            // 
            this.NumCorsMaxAge.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.NumCorsMaxAge.Location = new System.Drawing.Point(369, 284);
            this.NumCorsMaxAge.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.NumCorsMaxAge.Name = "NumCorsMaxAge";
            this.NumCorsMaxAge.Size = new System.Drawing.Size(120, 20);
            this.NumCorsMaxAge.TabIndex = 8;
            this.NumCorsMaxAge.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.NumCorsMaxAge.Value = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            // 
            // ChkEnableCors
            // 
            this.ChkEnableCors.AutoSize = true;
            this.ChkEnableCors.Location = new System.Drawing.Point(827, 142);
            this.ChkEnableCors.Name = "ChkEnableCors";
            this.ChkEnableCors.Size = new System.Drawing.Size(132, 17);
            this.ChkEnableCors.TabIndex = 7;
            this.ChkEnableCors.Text = "Enable CORS Support";
            this.ChkEnableCors.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(52, 18);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(921, 23);
            this.label14.TabIndex = 6;
            this.label14.Text = "CORS support is not required by most users. It only needs to be enabled when ASCO" +
    "M Remote is used within a cross site scripting environment.";
            this.label14.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LabHelp2
            // 
            this.LabHelp2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabHelp2.ForeColor = System.Drawing.SystemColors.Highlight;
            this.LabHelp2.Location = new System.Drawing.Point(55, 340);
            this.LabHelp2.Name = "LabHelp2";
            this.LabHelp2.Size = new System.Drawing.Size(918, 20);
            this.LabHelp2.TabIndex = 5;
            this.LabHelp2.Text = " If a CORS request comes from an origin that is not in this list, the returned Ac" +
    "cess-Control-Allow-Origin header will contain the first entry in this table.";
            this.LabHelp2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // LabHelp1
            // 
            this.LabHelp1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabHelp1.ForeColor = System.Drawing.SystemColors.Highlight;
            this.LabHelp1.Location = new System.Drawing.Point(49, 320);
            this.LabHelp1.Name = "LabHelp1";
            this.LabHelp1.Size = new System.Drawing.Size(924, 20);
            this.LabHelp1.TabIndex = 2;
            this.LabHelp1.Text = "The default origin * indicates that all hosts are permitted to access the Remote " +
    "Server";
            this.LabHelp1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // DataGridCorsOrigins
            // 
            this.DataGridCorsOrigins.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.DataGridCorsOrigins.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.DataGridCorsOrigins.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataGridCorsOrigins.EnableHeadersVisualStyles = false;
            this.DataGridCorsOrigins.Location = new System.Drawing.Point(219, 50);
            this.DataGridCorsOrigins.Name = "DataGridCorsOrigins";
            this.DataGridCorsOrigins.Size = new System.Drawing.Size(586, 217);
            this.DataGridCorsOrigins.TabIndex = 0;
            // 
            // ChkEnableDiscovery
            // 
            this.ChkEnableDiscovery.AutoSize = true;
            this.ChkEnableDiscovery.Location = new System.Drawing.Point(293, 213);
            this.ChkEnableDiscovery.Name = "ChkEnableDiscovery";
            this.ChkEnableDiscovery.Size = new System.Drawing.Size(109, 17);
            this.ChkEnableDiscovery.TabIndex = 45;
            this.ChkEnableDiscovery.Text = "Enable Discovery";
            this.ChkEnableDiscovery.UseVisualStyleBackColor = true;
            // 
            // SetupForm
            // 
            this.AcceptButton = this.BtnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(1061, 447);
            this.Controls.Add(this.SetupTabControl);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.BtnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetupForm";
            this.Text = "ASCOM Remote Server Configuration";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.SetupTabControl.ResumeLayout(false);
            this.DeviceConfigurationTab.ResumeLayout(false);
            this.DeviceConfigurationTab.PerformLayout();
            this.ServerConfigurationTab.ResumeLayout(false);
            this.ServerConfigurationTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.CorsConfigurationTab.ResumeLayout(false);
            this.CorsConfigurationTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumCorsMaxAge)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataGridCorsOrigins)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private ServedDevice ServedDevice1;
        private ServedDevice ServedDevice2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private ServedDevice ServedDevice3;
        private ServedDevice ServedDevice4;
        private ServedDevice ServedDevice5;
        private ServedDevice ServedDevice6;
        private ServedDevice ServedDevice7;
        private ServedDevice ServedDevice8;
        private ServedDevice ServedDevice9;
        private System.Windows.Forms.CheckBox chkAutoConnect;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.CheckBox chkDebugTrace;
        private System.Windows.Forms.CheckBox chkAccessLog;
        private ServedDevice ServedDevice0;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox chkManagementInterfaceEnabled;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.CheckBox ChkStartWithApiEnabled;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label LblDevicesNotDisconnoected;
        private System.Windows.Forms.CheckBox ChkRunDriversInSeparateThreadss;
        private System.Windows.Forms.CheckBox ChkLogClientIPAddress;
        private System.Windows.Forms.CheckBox ChkIncludeDriverExceptionsInJsonResponses;
        private System.Windows.Forms.TabControl SetupTabControl;
        private System.Windows.Forms.TabPage DeviceConfigurationTab;
        private System.Windows.Forms.TabPage ServerConfigurationTab;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox addressList;
        private System.Windows.Forms.TextBox TxtRemoteServerLocation;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabPage CorsConfigurationTab;
        private System.Windows.Forms.DataGridView DataGridCorsOrigins;
        private System.Windows.Forms.Label LabHelp2;
        private System.Windows.Forms.Label LabHelp1;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label LabMaxAge;
        private System.Windows.Forms.NumericUpDown NumCorsMaxAge;
        private System.Windows.Forms.CheckBox ChkEnableCors;
        private System.Windows.Forms.CheckBox ChkCorsSupportCredentials;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox ChkEnableDiscovery;
    }
}