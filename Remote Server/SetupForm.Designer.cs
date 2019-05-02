namespace ASCOM.Remote
{
    partial class SetupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
            this.addressList = new System.Windows.Forms.ComboBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnOK = new System.Windows.Forms.Button();
            this.numPort = new System.Windows.Forms.NumericUpDown();
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
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkManagementInterfaceEnabled = new System.Windows.Forms.CheckBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.ChkStartWithApiEnabled = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.LblDevicesNotDisconnoected = new System.Windows.Forms.Label();
            this.ServedDevice0 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice9 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice8 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice7 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice6 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice5 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice4 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice3 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice2 = new ASCOM.Remote.ServedDevice();
            this.ServedDevice1 = new ASCOM.Remote.ServedDevice();
            this.ChkRunDriversInSeparateThreadss = new System.Windows.Forms.CheckBox();
            this.ChkLogClientIPAddress = new System.Windows.Forms.CheckBox();
            this.ChkIncludeDriverExceptionsInJsonResponses = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // addressList
            // 
            this.addressList.FormattingEnabled = true;
            this.addressList.Location = new System.Drawing.Point(76, 388);
            this.addressList.Name = "addressList";
            this.addressList.Size = new System.Drawing.Size(328, 21);
            this.addressList.TabIndex = 0;
            // 
            // BtnCancel
            // 
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Location = new System.Drawing.Point(735, 543);
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.Size = new System.Drawing.Size(75, 23);
            this.BtnCancel.TabIndex = 19;
            this.BtnCancel.Text = "Cancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // BtnOK
            // 
            this.BtnOK.Location = new System.Drawing.Point(735, 514);
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.Size = new System.Drawing.Size(75, 23);
            this.BtnOK.TabIndex = 18;
            this.BtnOK.Text = "OK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(410, 389);
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
            this.numPort.TabIndex = 1;
            this.numPort.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(60, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Device Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(430, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Device";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(191, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(81, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Device Number";
            // 
            // chkAutoConnect
            // 
            this.chkAutoConnect.AutoSize = true;
            this.chkAutoConnect.Location = new System.Drawing.Point(96, 442);
            this.chkAutoConnect.Name = "chkAutoConnect";
            this.chkAutoConnect.Size = new System.Drawing.Size(133, 17);
            this.chkAutoConnect.TabIndex = 2;
            this.chkAutoConnect.Text = "Auto Connect Devices";
            this.chkAutoConnect.UseVisualStyleBackColor = true;
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(96, 501);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(96, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Write Log Files";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // chkDebugTrace
            // 
            this.chkDebugTrace.AutoSize = true;
            this.chkDebugTrace.Location = new System.Drawing.Point(96, 524);
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
            this.chkAccessLog.Location = new System.Drawing.Point(96, 465);
            this.chkAccessLog.Name = "chkAccessLog";
            this.chkAccessLog.Size = new System.Drawing.Size(124, 17);
            this.chkAccessLog.TabIndex = 4;
            this.chkAccessLog.Text = "Access Log Enabled";
            this.chkAccessLog.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(768, 28);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Allow Connected to be set";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(788, 44);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "False";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(848, 44);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 29;
            this.label6.Text = "True";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(190, 372);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 13);
            this.label7.TabIndex = 30;
            this.label7.Text = "Rest server IP address";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(408, 372);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 13);
            this.label8.TabIndex = 31;
            this.label8.Text = "Rest server IP Port";
            // 
            // chkManagementInterfaceEnabled
            // 
            this.chkManagementInterfaceEnabled.AutoSize = true;
            this.chkManagementInterfaceEnabled.Enabled = false;
            this.chkManagementInterfaceEnabled.Location = new System.Drawing.Point(309, 442);
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
            this.ChkStartWithApiEnabled.Location = new System.Drawing.Point(309, 465);
            this.ChkStartWithApiEnabled.Name = "ChkStartWithApiEnabled";
            this.ChkStartWithApiEnabled.Size = new System.Drawing.Size(131, 17);
            this.ChkStartWithApiEnabled.TabIndex = 5;
            this.ChkStartWithApiEnabled.Text = "Start with API enabled";
            this.ChkStartWithApiEnabled.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(652, 44);
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
            this.LblDevicesNotDisconnoected.Location = new System.Drawing.Point(563, 341);
            this.LblDevicesNotDisconnoected.Name = "LblDevicesNotDisconnoected";
            this.LblDevicesNotDisconnoected.Size = new System.Drawing.Size(257, 26);
            this.LblDevicesNotDisconnoected.TabIndex = 33;
            this.LblDevicesNotDisconnoected.Text = "Devices are Connected\r\nConfiguration requires that devices are Disconnected";
            this.LblDevicesNotDisconnoected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ServedDevice0
            // 
            this.ServedDevice0.AllowConnectedSetFalse = false;
            this.ServedDevice0.AllowConnectedSetTrue = false;
            this.ServedDevice0.Description = "";
            this.ServedDevice0.DeviceNumber = 0;
            this.ServedDevice0.DevicesAreConnected = false;
            this.ServedDevice0.DeviceType = "None";
            this.ServedDevice0.Location = new System.Drawing.Point(12, 60);
            this.ServedDevice0.Name = "ServedDevice0";
            this.ServedDevice0.ProgID = "";
            this.ServedDevice0.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice0.TabIndex = 8;
            // 
            // ServedDevice9
            // 
            this.ServedDevice9.AllowConnectedSetFalse = false;
            this.ServedDevice9.AllowConnectedSetTrue = false;
            this.ServedDevice9.Description = "";
            this.ServedDevice9.DeviceNumber = 0;
            this.ServedDevice9.DevicesAreConnected = false;
            this.ServedDevice9.DeviceType = "None";
            this.ServedDevice9.Location = new System.Drawing.Point(12, 312);
            this.ServedDevice9.Name = "ServedDevice9";
            this.ServedDevice9.ProgID = "";
            this.ServedDevice9.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice9.TabIndex = 17;
            // 
            // ServedDevice8
            // 
            this.ServedDevice8.AllowConnectedSetFalse = false;
            this.ServedDevice8.AllowConnectedSetTrue = false;
            this.ServedDevice8.Description = "";
            this.ServedDevice8.DeviceNumber = 0;
            this.ServedDevice8.DevicesAreConnected = false;
            this.ServedDevice8.DeviceType = "None";
            this.ServedDevice8.Location = new System.Drawing.Point(12, 284);
            this.ServedDevice8.Name = "ServedDevice8";
            this.ServedDevice8.ProgID = "";
            this.ServedDevice8.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice8.TabIndex = 16;
            // 
            // ServedDevice7
            // 
            this.ServedDevice7.AllowConnectedSetFalse = false;
            this.ServedDevice7.AllowConnectedSetTrue = false;
            this.ServedDevice7.Description = "";
            this.ServedDevice7.DeviceNumber = 0;
            this.ServedDevice7.DevicesAreConnected = false;
            this.ServedDevice7.DeviceType = "None";
            this.ServedDevice7.Location = new System.Drawing.Point(12, 256);
            this.ServedDevice7.Name = "ServedDevice7";
            this.ServedDevice7.ProgID = "";
            this.ServedDevice7.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice7.TabIndex = 15;
            // 
            // ServedDevice6
            // 
            this.ServedDevice6.AllowConnectedSetFalse = false;
            this.ServedDevice6.AllowConnectedSetTrue = false;
            this.ServedDevice6.Description = "";
            this.ServedDevice6.DeviceNumber = 0;
            this.ServedDevice6.DevicesAreConnected = false;
            this.ServedDevice6.DeviceType = "None";
            this.ServedDevice6.Location = new System.Drawing.Point(12, 228);
            this.ServedDevice6.Name = "ServedDevice6";
            this.ServedDevice6.ProgID = "";
            this.ServedDevice6.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice6.TabIndex = 14;
            // 
            // ServedDevice5
            // 
            this.ServedDevice5.AllowConnectedSetFalse = false;
            this.ServedDevice5.AllowConnectedSetTrue = false;
            this.ServedDevice5.Description = "";
            this.ServedDevice5.DeviceNumber = 0;
            this.ServedDevice5.DevicesAreConnected = false;
            this.ServedDevice5.DeviceType = "None";
            this.ServedDevice5.Location = new System.Drawing.Point(12, 200);
            this.ServedDevice5.Name = "ServedDevice5";
            this.ServedDevice5.ProgID = "";
            this.ServedDevice5.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice5.TabIndex = 13;
            // 
            // ServedDevice4
            // 
            this.ServedDevice4.AllowConnectedSetFalse = false;
            this.ServedDevice4.AllowConnectedSetTrue = false;
            this.ServedDevice4.Description = "";
            this.ServedDevice4.DeviceNumber = 0;
            this.ServedDevice4.DevicesAreConnected = false;
            this.ServedDevice4.DeviceType = "None";
            this.ServedDevice4.Location = new System.Drawing.Point(12, 172);
            this.ServedDevice4.Name = "ServedDevice4";
            this.ServedDevice4.ProgID = "";
            this.ServedDevice4.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice4.TabIndex = 12;
            // 
            // ServedDevice3
            // 
            this.ServedDevice3.AllowConnectedSetFalse = false;
            this.ServedDevice3.AllowConnectedSetTrue = false;
            this.ServedDevice3.Description = "";
            this.ServedDevice3.DeviceNumber = 0;
            this.ServedDevice3.DevicesAreConnected = false;
            this.ServedDevice3.DeviceType = "None";
            this.ServedDevice3.Location = new System.Drawing.Point(12, 144);
            this.ServedDevice3.Name = "ServedDevice3";
            this.ServedDevice3.ProgID = "";
            this.ServedDevice3.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice3.TabIndex = 11;
            // 
            // ServedDevice2
            // 
            this.ServedDevice2.AllowConnectedSetFalse = false;
            this.ServedDevice2.AllowConnectedSetTrue = false;
            this.ServedDevice2.Description = "";
            this.ServedDevice2.DeviceNumber = 0;
            this.ServedDevice2.DevicesAreConnected = false;
            this.ServedDevice2.DeviceType = "None";
            this.ServedDevice2.Location = new System.Drawing.Point(12, 116);
            this.ServedDevice2.Name = "ServedDevice2";
            this.ServedDevice2.ProgID = "";
            this.ServedDevice2.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice2.TabIndex = 10;
            // 
            // ServedDevice1
            // 
            this.ServedDevice1.AllowConnectedSetFalse = false;
            this.ServedDevice1.AllowConnectedSetTrue = false;
            this.ServedDevice1.Description = "";
            this.ServedDevice1.DeviceNumber = 0;
            this.ServedDevice1.DevicesAreConnected = false;
            this.ServedDevice1.DeviceType = "None";
            this.ServedDevice1.Location = new System.Drawing.Point(12, 88);
            this.ServedDevice1.Name = "ServedDevice1";
            this.ServedDevice1.ProgID = "";
            this.ServedDevice1.Size = new System.Drawing.Size(872, 22);
            this.ServedDevice1.TabIndex = 9;
            // 
            // ChkRunDriversInSeparateThreadss
            // 
            this.ChkRunDriversInSeparateThreadss.AutoSize = true;
            this.ChkRunDriversInSeparateThreadss.Location = new System.Drawing.Point(309, 501);
            this.ChkRunDriversInSeparateThreadss.Name = "ChkRunDriversInSeparateThreadss";
            this.ChkRunDriversInSeparateThreadss.Size = new System.Drawing.Size(181, 17);
            this.ChkRunDriversInSeparateThreadss.TabIndex = 34;
            this.ChkRunDriversInSeparateThreadss.Text = "Run Drivers in Separate Threads";
            this.ChkRunDriversInSeparateThreadss.UseVisualStyleBackColor = true;
            // 
            // ChkLogClientIPAddress
            // 
            this.ChkLogClientIPAddress.AutoSize = true;
            this.ChkLogClientIPAddress.Location = new System.Drawing.Point(309, 524);
            this.ChkLogClientIPAddress.Name = "ChkLogClientIPAddress";
            this.ChkLogClientIPAddress.Size = new System.Drawing.Size(134, 17);
            this.ChkLogClientIPAddress.TabIndex = 35;
            this.ChkLogClientIPAddress.Text = "Log Client\'s IP Address";
            this.ChkLogClientIPAddress.UseVisualStyleBackColor = true;
            // 
            // ChkIncludeDriverExceptionsInJsonResponses
            // 
            this.ChkIncludeDriverExceptionsInJsonResponses.AutoSize = true;
            this.ChkIncludeDriverExceptionsInJsonResponses.Location = new System.Drawing.Point(96, 547);
            this.ChkIncludeDriverExceptionsInJsonResponses.Name = "ChkIncludeDriverExceptionsInJsonResponses";
            this.ChkIncludeDriverExceptionsInJsonResponses.Size = new System.Drawing.Size(364, 17);
            this.ChkIncludeDriverExceptionsInJsonResponses.TabIndex = 36;
            this.ChkIncludeDriverExceptionsInJsonResponses.Text = "Include driver exceptions in JSON responses (only useful for debugging)";
            this.ChkIncludeDriverExceptionsInJsonResponses.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.ForeColor = System.Drawing.Color.Red;
            this.label10.Location = new System.Drawing.Point(484, 443);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(138, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "(Disabled pending redesign)";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SetupForm
            // 
            this.AcceptButton = this.BtnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.ClientSize = new System.Drawing.Size(920, 577);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.ChkIncludeDriverExceptionsInJsonResponses);
            this.Controls.Add(this.ChkLogClientIPAddress);
            this.Controls.Add(this.ChkRunDriversInSeparateThreadss);
            this.Controls.Add(this.LblDevicesNotDisconnoected);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.ChkStartWithApiEnabled);
            this.Controls.Add(this.chkManagementInterfaceEnabled);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ServedDevice0);
            this.Controls.Add(this.chkAccessLog);
            this.Controls.Add(this.chkDebugTrace);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.chkAutoConnect);
            this.Controls.Add(this.ServedDevice9);
            this.Controls.Add(this.ServedDevice8);
            this.Controls.Add(this.ServedDevice7);
            this.Controls.Add(this.ServedDevice6);
            this.Controls.Add(this.ServedDevice5);
            this.Controls.Add(this.ServedDevice4);
            this.Controls.Add(this.ServedDevice3);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ServedDevice2);
            this.Controls.Add(this.ServedDevice1);
            this.Controls.Add(this.numPort);
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.addressList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SetupForm";
            this.Text = "ASCOM REST Server Configuration";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox addressList;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.NumericUpDown numPort;
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
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkManagementInterfaceEnabled;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.CheckBox ChkStartWithApiEnabled;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label LblDevicesNotDisconnoected;
        private System.Windows.Forms.CheckBox ChkRunDriversInSeparateThreadss;
        private System.Windows.Forms.CheckBox ChkLogClientIPAddress;
        private System.Windows.Forms.CheckBox ChkIncludeDriverExceptionsInJsonResponses;
        private System.Windows.Forms.Label label10;
    }
}