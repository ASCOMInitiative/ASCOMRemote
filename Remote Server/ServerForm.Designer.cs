namespace ASCOM.Remote
{
    partial class ServerForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerForm));
            BtnSetup = new System.Windows.Forms.Button();
            BtnConnectDevices = new System.Windows.Forms.Button();
            BtnDisconnectDevices = new System.Windows.Forms.Button();
            BtnExit = new System.Windows.Forms.Button();
            LblDriverStatus = new System.Windows.Forms.Label();
            txtLog = new System.Windows.Forms.TextBox();
            lblTitle = new System.Windows.Forms.Label();
            chkLogRequests = new System.Windows.Forms.CheckBox();
            chkLogResponses = new System.Windows.Forms.CheckBox();
            txtConcurrency = new System.Windows.Forms.TextBox();
            lblConcurrentTransactions = new System.Windows.Forms.Label();
            LblRESTStatus = new System.Windows.Forms.Label();
            BtnStartRESTServer = new System.Windows.Forms.Button();
            BtnStopRESTServer = new System.Windows.Forms.Button();
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            systemTrayMenuItems = new System.Windows.Forms.ContextMenuStrip(components);
            Title = new System.Windows.Forms.ToolStripMenuItem();
            TitleSeparator = new System.Windows.Forms.ToolStripSeparator();
            NumberOfDevices = new System.Windows.Forms.ToolStripMenuItem();
            IpV4 = new System.Windows.Forms.ToolStripMenuItem();
            IpV6 = new System.Windows.Forms.ToolStripMenuItem();
            IpAddress = new System.Windows.Forms.ToolStripMenuItem();
            Port = new System.Windows.Forms.ToolStripMenuItem();
            ActionSeparator = new System.Windows.Forms.ToolStripSeparator();
            Exit = new System.Windows.Forms.ToolStripMenuItem();
            BtnUpdateAvailable = new System.Windows.Forms.Button();
            BtnPreviewAvailable = new System.Windows.Forms.Button();
            systemTrayMenuItems.SuspendLayout();
            SuspendLayout();
            // 
            // BtnSetup
            // 
            BtnSetup.Location = new System.Drawing.Point(770, 414);
            BtnSetup.Name = "BtnSetup";
            BtnSetup.Size = new System.Drawing.Size(100, 24);
            BtnSetup.TabIndex = 0;
            BtnSetup.Text = "Setup";
            BtnSetup.UseVisualStyleBackColor = true;
            BtnSetup.Click += BtnSetup_Click;
            // 
            // BtnConnectDevices
            // 
            BtnConnectDevices.Location = new System.Drawing.Point(824, 339);
            BtnConnectDevices.Name = "BtnConnectDevices";
            BtnConnectDevices.Size = new System.Drawing.Size(79, 24);
            BtnConnectDevices.TabIndex = 2;
            BtnConnectDevices.Text = "Connect";
            BtnConnectDevices.UseVisualStyleBackColor = true;
            BtnConnectDevices.Click += BtnConnectDevices_Click;
            // 
            // BtnDisconnectDevices
            // 
            BtnDisconnectDevices.Location = new System.Drawing.Point(733, 339);
            BtnDisconnectDevices.Name = "BtnDisconnectDevices";
            BtnDisconnectDevices.Size = new System.Drawing.Size(79, 24);
            BtnDisconnectDevices.TabIndex = 3;
            BtnDisconnectDevices.Text = "Disconnect";
            BtnDisconnectDevices.UseVisualStyleBackColor = true;
            BtnDisconnectDevices.Click += BtnDisconnectDevices_Click;
            // 
            // BtnExit
            // 
            BtnExit.Location = new System.Drawing.Point(770, 474);
            BtnExit.Name = "BtnExit";
            BtnExit.Size = new System.Drawing.Size(100, 24);
            BtnExit.TabIndex = 4;
            BtnExit.Text = "Exit";
            BtnExit.UseVisualStyleBackColor = true;
            BtnExit.Click += BtnExit_Click;
            // 
            // LblDriverStatus
            // 
            LblDriverStatus.BackColor = System.Drawing.Color.Red;
            LblDriverStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            LblDriverStatus.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblDriverStatus.Location = new System.Drawing.Point(734, 309);
            LblDriverStatus.Name = "LblDriverStatus";
            LblDriverStatus.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            LblDriverStatus.Size = new System.Drawing.Size(168, 24);
            LblDriverStatus.TabIndex = 6;
            LblDriverStatus.Text = "Drivers Unloaded";
            LblDriverStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtLog
            // 
            txtLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtLog.Location = new System.Drawing.Point(12, 73);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            txtLog.Size = new System.Drawing.Size(704, 531);
            txtLog.TabIndex = 7;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTitle.ForeColor = System.Drawing.SystemColors.Highlight;
            lblTitle.Location = new System.Drawing.Point(185, 25);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(359, 26);
            lblTitle.TabIndex = 8;
            lblTitle.Text = "ASCOM Remote Server Console";
            // 
            // chkLogRequests
            // 
            chkLogRequests.AutoSize = true;
            chkLogRequests.Location = new System.Drawing.Point(770, 159);
            chkLogRequests.Name = "chkLogRequests";
            chkLogRequests.Size = new System.Drawing.Size(92, 17);
            chkLogRequests.TabIndex = 9;
            chkLogRequests.Text = "Log Requests";
            chkLogRequests.UseVisualStyleBackColor = true;
            // 
            // chkLogResponses
            // 
            chkLogResponses.AutoSize = true;
            chkLogResponses.Location = new System.Drawing.Point(770, 182);
            chkLogResponses.Name = "chkLogResponses";
            chkLogResponses.Size = new System.Drawing.Size(100, 17);
            chkLogResponses.TabIndex = 10;
            chkLogResponses.Text = "Log Responses";
            chkLogResponses.UseVisualStyleBackColor = true;
            // 
            // txtConcurrency
            // 
            txtConcurrency.BackColor = System.Drawing.SystemColors.Window;
            txtConcurrency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtConcurrency.Location = new System.Drawing.Point(764, 103);
            txtConcurrency.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            txtConcurrency.Name = "txtConcurrency";
            txtConcurrency.ReadOnly = true;
            txtConcurrency.Size = new System.Drawing.Size(26, 20);
            txtConcurrency.TabIndex = 11;
            txtConcurrency.Text = "0";
            txtConcurrency.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblConcurrentTransactions
            // 
            lblConcurrentTransactions.AutoSize = true;
            lblConcurrentTransactions.Location = new System.Drawing.Point(809, 99);
            lblConcurrentTransactions.Name = "lblConcurrentTransactions";
            lblConcurrentTransactions.Size = new System.Drawing.Size(68, 26);
            lblConcurrentTransactions.TabIndex = 12;
            lblConcurrentTransactions.Text = "Concurrent\r\nTransactions";
            // 
            // LblRESTStatus
            // 
            LblRESTStatus.BackColor = System.Drawing.Color.Red;
            LblRESTStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            LblRESTStatus.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            LblRESTStatus.Location = new System.Drawing.Point(734, 220);
            LblRESTStatus.Name = "LblRESTStatus";
            LblRESTStatus.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            LblRESTStatus.Size = new System.Drawing.Size(168, 24);
            LblRESTStatus.TabIndex = 14;
            LblRESTStatus.Text = "Remote Server Down";
            LblRESTStatus.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // BtnStartRESTServer
            // 
            BtnStartRESTServer.Location = new System.Drawing.Point(824, 250);
            BtnStartRESTServer.Name = "BtnStartRESTServer";
            BtnStartRESTServer.Size = new System.Drawing.Size(79, 24);
            BtnStartRESTServer.TabIndex = 16;
            BtnStartRESTServer.Text = "Start";
            BtnStartRESTServer.UseVisualStyleBackColor = true;
            BtnStartRESTServer.Click += BtnStartRESTServer_Click;
            // 
            // BtnStopRESTServer
            // 
            BtnStopRESTServer.Location = new System.Drawing.Point(733, 250);
            BtnStopRESTServer.Name = "BtnStopRESTServer";
            BtnStopRESTServer.Size = new System.Drawing.Size(79, 24);
            BtnStopRESTServer.TabIndex = 15;
            BtnStopRESTServer.Text = "Stop";
            BtnStopRESTServer.UseVisualStyleBackColor = true;
            BtnStopRESTServer.Click += BtnStopRESTServer_Click;
            // 
            // notifyIcon
            // 
            notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "Double click to restore ASCOM Remote";
            notifyIcon.BalloonTipTitle = "ASCOM Remote";
            notifyIcon.ContextMenuStrip = systemTrayMenuItems;
            notifyIcon.Icon = (System.Drawing.Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "ASCOM Remote";
            // 
            // systemTrayMenuItems
            // 
            systemTrayMenuItems.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { Title, TitleSeparator, NumberOfDevices, IpV4, IpV6, IpAddress, Port, ActionSeparator, Exit });
            systemTrayMenuItems.Name = "systemTrayMenuItems";
            systemTrayMenuItems.Size = new System.Drawing.Size(206, 170);
            // 
            // Title
            // 
            Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            Title.Name = "Title";
            Title.Size = new System.Drawing.Size(205, 22);
            Title.Text = "ASCOM Remote Server";
            // 
            // TitleSeparator
            // 
            TitleSeparator.Name = "TitleSeparator";
            TitleSeparator.Size = new System.Drawing.Size(202, 6);
            // 
            // NumberOfDevices
            // 
            NumberOfDevices.Enabled = false;
            NumberOfDevices.Name = "NumberOfDevices";
            NumberOfDevices.Size = new System.Drawing.Size(205, 22);
            NumberOfDevices.Text = "Number of devices: 0";
            // 
            // IpV4
            // 
            IpV4.Enabled = false;
            IpV4.Name = "IpV4";
            IpV4.Size = new System.Drawing.Size(205, 22);
            IpV4.Text = "IP v4 enabled: False";
            // 
            // IpV6
            // 
            IpV6.Enabled = false;
            IpV6.Name = "IpV6";
            IpV6.Size = new System.Drawing.Size(205, 22);
            IpV6.Text = "IP v6 enabled: False";
            // 
            // IpAddress
            // 
            IpAddress.Enabled = false;
            IpAddress.Name = "IpAddress";
            IpAddress.Size = new System.Drawing.Size(205, 22);
            // 
            // Port
            // 
            Port.Enabled = false;
            Port.Name = "Port";
            Port.Size = new System.Drawing.Size(205, 22);
            Port.Text = "toolStripMenuItem2";
            // 
            // ActionSeparator
            // 
            ActionSeparator.Name = "ActionSeparator";
            ActionSeparator.Size = new System.Drawing.Size(202, 6);
            // 
            // Exit
            // 
            Exit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            Exit.Name = "Exit";
            Exit.Size = new System.Drawing.Size(205, 22);
            Exit.Text = "Exit";
            // 
            // BtnUpdateAvailable
            // 
            BtnUpdateAvailable.BackColor = System.Drawing.SystemColors.Control;
            BtnUpdateAvailable.ForeColor = System.Drawing.SystemColors.HotTrack;
            BtnUpdateAvailable.Location = new System.Drawing.Point(770, 553);
            BtnUpdateAvailable.Name = "BtnUpdateAvailable";
            BtnUpdateAvailable.Size = new System.Drawing.Size(100, 24);
            BtnUpdateAvailable.TabIndex = 17;
            BtnUpdateAvailable.Text = "Update Available";
            BtnUpdateAvailable.UseVisualStyleBackColor = false;
            BtnUpdateAvailable.Visible = false;
            BtnUpdateAvailable.Click += BtnUpdateAvailable_Click;
            // 
            // BtnPreviewAvailable
            // 
            BtnPreviewAvailable.ForeColor = System.Drawing.SystemColors.HotTrack;
            BtnPreviewAvailable.Location = new System.Drawing.Point(770, 583);
            BtnPreviewAvailable.Name = "BtnPreviewAvailable";
            BtnPreviewAvailable.Size = new System.Drawing.Size(100, 24);
            BtnPreviewAvailable.TabIndex = 18;
            BtnPreviewAvailable.Text = "Preview Available";
            BtnPreviewAvailable.UseVisualStyleBackColor = true;
            BtnPreviewAvailable.Visible = false;
            BtnPreviewAvailable.Click += BtnPreviewAvailable_Click;
            // 
            // ServerForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(920, 616);
            Controls.Add(BtnPreviewAvailable);
            Controls.Add(BtnUpdateAvailable);
            Controls.Add(BtnStartRESTServer);
            Controls.Add(BtnStopRESTServer);
            Controls.Add(LblRESTStatus);
            Controls.Add(lblConcurrentTransactions);
            Controls.Add(txtConcurrency);
            Controls.Add(chkLogResponses);
            Controls.Add(chkLogRequests);
            Controls.Add(lblTitle);
            Controls.Add(txtLog);
            Controls.Add(LblDriverStatus);
            Controls.Add(BtnExit);
            Controls.Add(BtnDisconnectDevices);
            Controls.Add(BtnConnectDevices);
            Controls.Add(BtnSetup);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Name = "ServerForm";
            Text = "ASCOM Remote Server";
            Load += ServerForm_Load;
            systemTrayMenuItems.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button BtnSetup;
        private System.Windows.Forms.Button BtnConnectDevices;
        private System.Windows.Forms.Button BtnDisconnectDevices;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.Label LblDriverStatus;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkLogRequests;
        private System.Windows.Forms.CheckBox chkLogResponses;
        private System.Windows.Forms.TextBox txtConcurrency;
        private System.Windows.Forms.Label lblConcurrentTransactions;
        private System.Windows.Forms.Label LblRESTStatus;
        private System.Windows.Forms.Button BtnStartRESTServer;
        private System.Windows.Forms.Button BtnStopRESTServer;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip systemTrayMenuItems;
        private System.Windows.Forms.ToolStripMenuItem NumberOfDevices;
        private System.Windows.Forms.ToolStripMenuItem IpV4;
        private System.Windows.Forms.ToolStripMenuItem IpV6;
        private System.Windows.Forms.ToolStripSeparator ActionSeparator;
        private System.Windows.Forms.ToolStripMenuItem Exit;
        private System.Windows.Forms.ToolStripMenuItem Title;
        private System.Windows.Forms.ToolStripSeparator TitleSeparator;
        private System.Windows.Forms.ToolStripMenuItem IpAddress;
        private System.Windows.Forms.ToolStripMenuItem Port;
        private System.Windows.Forms.Button BtnUpdateAvailable;
        private System.Windows.Forms.Button BtnPreviewAvailable;
    }
}

