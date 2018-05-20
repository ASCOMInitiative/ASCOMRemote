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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerForm));
            this.BtnSetup = new System.Windows.Forms.Button();
            this.BtnConnect = new System.Windows.Forms.Button();
            this.BtnDisconnect = new System.Windows.Forms.Button();
            this.BtnExit = new System.Windows.Forms.Button();
            this.pBoxConnected = new System.Windows.Forms.PictureBox();
            this.lblConnected = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.chkLogRequests = new System.Windows.Forms.CheckBox();
            this.chkLogResponses = new System.Windows.Forms.CheckBox();
            this.txtConcurrency = new System.Windows.Forms.TextBox();
            this.lblConcurrentTransactions = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pBoxConnected)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnSetup
            // 
            this.BtnSetup.Location = new System.Drawing.Point(738, 479);
            this.BtnSetup.Name = "BtnSetup";
            this.BtnSetup.Size = new System.Drawing.Size(100, 23);
            this.BtnSetup.TabIndex = 0;
            this.BtnSetup.Text = "Setup";
            this.BtnSetup.UseVisualStyleBackColor = true;
            this.BtnSetup.Click += new System.EventHandler(this.BtnSetup_Click);
            // 
            // BtnConnect
            // 
            this.BtnConnect.Location = new System.Drawing.Point(738, 353);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Size = new System.Drawing.Size(100, 23);
            this.BtnConnect.TabIndex = 2;
            this.BtnConnect.Text = "Connect";
            this.BtnConnect.UseVisualStyleBackColor = true;
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // BtnDisconnect
            // 
            this.BtnDisconnect.Location = new System.Drawing.Point(738, 382);
            this.BtnDisconnect.Name = "BtnDisconnect";
            this.BtnDisconnect.Size = new System.Drawing.Size(100, 23);
            this.BtnDisconnect.TabIndex = 3;
            this.BtnDisconnect.Text = "Disconnect";
            this.BtnDisconnect.UseVisualStyleBackColor = true;
            this.BtnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Location = new System.Drawing.Point(738, 581);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Size = new System.Drawing.Size(100, 23);
            this.BtnExit.TabIndex = 4;
            this.BtnExit.Text = "Exit";
            this.BtnExit.UseVisualStyleBackColor = true;
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // pBoxConnected
            // 
            this.pBoxConnected.BackColor = System.Drawing.Color.Red;
            this.pBoxConnected.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pBoxConnected.Location = new System.Drawing.Point(738, 225);
            this.pBoxConnected.Name = "pBoxConnected";
            this.pBoxConnected.Size = new System.Drawing.Size(100, 50);
            this.pBoxConnected.TabIndex = 5;
            this.pBoxConnected.TabStop = false;
            // 
            // lblConnected
            // 
            this.lblConnected.Font = new System.Drawing.Font("Arial Narrow", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnected.Location = new System.Drawing.Point(738, 278);
            this.lblConnected.Name = "lblConnected";
            this.lblConnected.Size = new System.Drawing.Size(100, 20);
            this.lblConnected.TabIndex = 6;
            this.lblConnected.Text = "Disconnected";
            this.lblConnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 73);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(704, 531);
            this.txtLog.TabIndex = 7;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblTitle.Location = new System.Drawing.Point(164, 25);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(357, 26);
            this.lblTitle.TabIndex = 8;
            this.lblTitle.Text = "ASCOM Restful Interface Server";
            // 
            // chkLogRequests
            // 
            this.chkLogRequests.AutoSize = true;
            this.chkLogRequests.Location = new System.Drawing.Point(752, 159);
            this.chkLogRequests.Name = "chkLogRequests";
            this.chkLogRequests.Size = new System.Drawing.Size(92, 17);
            this.chkLogRequests.TabIndex = 9;
            this.chkLogRequests.Text = "Log Requests";
            this.chkLogRequests.UseVisualStyleBackColor = true;
            // 
            // chkLogResponses
            // 
            this.chkLogResponses.AutoSize = true;
            this.chkLogResponses.Location = new System.Drawing.Point(752, 182);
            this.chkLogResponses.Name = "chkLogResponses";
            this.chkLogResponses.Size = new System.Drawing.Size(100, 17);
            this.chkLogResponses.TabIndex = 10;
            this.chkLogResponses.Text = "Log Responses";
            this.chkLogResponses.UseVisualStyleBackColor = true;
            // 
            // txtConcurrency
            // 
            this.txtConcurrency.BackColor = System.Drawing.SystemColors.Window;
            this.txtConcurrency.Location = new System.Drawing.Point(738, 103);
            this.txtConcurrency.Name = "txtConcurrency";
            this.txtConcurrency.ReadOnly = true;
            this.txtConcurrency.Size = new System.Drawing.Size(26, 20);
            this.txtConcurrency.TabIndex = 11;
            this.txtConcurrency.Text = "0";
            this.txtConcurrency.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblConcurrentTransactions
            // 
            this.lblConcurrentTransactions.AutoSize = true;
            this.lblConcurrentTransactions.Location = new System.Drawing.Point(770, 99);
            this.lblConcurrentTransactions.Name = "lblConcurrentTransactions";
            this.lblConcurrentTransactions.Size = new System.Drawing.Size(68, 26);
            this.lblConcurrentTransactions.TabIndex = 12;
            this.lblConcurrentTransactions.Text = "Concurrent\r\nTransactions";
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 616);
            this.Controls.Add(this.lblConcurrentTransactions);
            this.Controls.Add(this.txtConcurrency);
            this.Controls.Add(this.chkLogResponses);
            this.Controls.Add(this.chkLogRequests);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblConnected);
            this.Controls.Add(this.pBoxConnected);
            this.Controls.Add(this.BtnExit);
            this.Controls.Add(this.BtnDisconnect);
            this.Controls.Add(this.BtnConnect);
            this.Controls.Add(this.BtnSetup);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServerForm";
            this.Text = "HTTP Server";
            this.Load += new System.EventHandler(this.ServerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pBoxConnected)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnSetup;
        private System.Windows.Forms.Button BtnConnect;
        private System.Windows.Forms.Button BtnDisconnect;
        private System.Windows.Forms.Button BtnExit;
        private System.Windows.Forms.PictureBox pBoxConnected;
        private System.Windows.Forms.Label lblConnected;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.CheckBox chkLogRequests;
        private System.Windows.Forms.CheckBox chkLogResponses;
        private System.Windows.Forms.TextBox txtConcurrency;
        private System.Windows.Forms.Label lblConcurrentTransactions;
    }
}

