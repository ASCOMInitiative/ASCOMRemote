namespace ASCOM.Remote
{
    partial class ServedDevice
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cmbDeviceType = new System.Windows.Forms.ComboBox();
            txtDeviceNumber = new System.Windows.Forms.TextBox();
            cmbDevice = new System.Windows.Forms.ComboBox();
            chkAllowSetConnectedTrue = new System.Windows.Forms.CheckBox();
            chkAllowSetConnectedFalse = new System.Windows.Forms.CheckBox();
            btnSetup = new System.Windows.Forms.Button();
            ChkAllowConcurrentAccess = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // cmbDeviceType
            // 
            cmbDeviceType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDeviceType.FormattingEnabled = true;
            cmbDeviceType.Location = new System.Drawing.Point(0, 1);
            cmbDeviceType.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmbDeviceType.Name = "cmbDeviceType";
            cmbDeviceType.Size = new System.Drawing.Size(192, 23);
            cmbDeviceType.TabIndex = 0;
            cmbDeviceType.SelectedIndexChanged += CmbDeviceType_SelectedIndexChanged;
            // 
            // txtDeviceNumber
            // 
            txtDeviceNumber.BackColor = System.Drawing.SystemColors.Window;
            txtDeviceNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txtDeviceNumber.Location = new System.Drawing.Point(233, 1);
            txtDeviceNumber.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtDeviceNumber.Name = "txtDeviceNumber";
            txtDeviceNumber.ReadOnly = true;
            txtDeviceNumber.Size = new System.Drawing.Size(28, 23);
            txtDeviceNumber.TabIndex = 1;
            txtDeviceNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmbDevice
            // 
            cmbDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new System.Drawing.Point(338, 1);
            cmbDevice.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new System.Drawing.Size(345, 23);
            cmbDevice.TabIndex = 3;
            cmbDevice.SelectedIndexChanged += CmbDevice_SelectedIndexChanged;
            // 
            // chkAllowSetConnectedTrue
            // 
            chkAllowSetConnectedTrue.AutoSize = true;
            chkAllowSetConnectedTrue.Location = new System.Drawing.Point(985, 5);
            chkAllowSetConnectedTrue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkAllowSetConnectedTrue.Name = "chkAllowSetConnectedTrue";
            chkAllowSetConnectedTrue.Size = new System.Drawing.Size(15, 14);
            chkAllowSetConnectedTrue.TabIndex = 5;
            chkAllowSetConnectedTrue.UseVisualStyleBackColor = true;
            // 
            // chkAllowSetConnectedFalse
            // 
            chkAllowSetConnectedFalse.AutoSize = true;
            chkAllowSetConnectedFalse.Location = new System.Drawing.Point(915, 5);
            chkAllowSetConnectedFalse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkAllowSetConnectedFalse.Name = "chkAllowSetConnectedFalse";
            chkAllowSetConnectedFalse.Size = new System.Drawing.Size(15, 14);
            chkAllowSetConnectedFalse.TabIndex = 4;
            chkAllowSetConnectedFalse.UseVisualStyleBackColor = true;
            // 
            // btnSetup
            // 
            btnSetup.Location = new System.Drawing.Point(751, 1);
            btnSetup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnSetup.Name = "btnSetup";
            btnSetup.Size = new System.Drawing.Size(88, 22);
            btnSetup.TabIndex = 6;
            btnSetup.Text = "Setup";
            btnSetup.UseVisualStyleBackColor = true;
            btnSetup.Click += BtnSetup_Click;
            // 
            // ChkAllowConcurrentAccess
            // 
            ChkAllowConcurrentAccess.AutoSize = true;
            ChkAllowConcurrentAccess.Location = new System.Drawing.Point(1083, 5);
            ChkAllowConcurrentAccess.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ChkAllowConcurrentAccess.Name = "ChkAllowConcurrentAccess";
            ChkAllowConcurrentAccess.Size = new System.Drawing.Size(15, 14);
            ChkAllowConcurrentAccess.TabIndex = 7;
            ChkAllowConcurrentAccess.UseVisualStyleBackColor = true;
            // 
            // ServedDevice
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(ChkAllowConcurrentAccess);
            Controls.Add(btnSetup);
            Controls.Add(chkAllowSetConnectedTrue);
            Controls.Add(chkAllowSetConnectedFalse);
            Controls.Add(cmbDevice);
            Controls.Add(txtDeviceNumber);
            Controls.Add(cmbDeviceType);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ServedDevice";
            Size = new System.Drawing.Size(1142, 25);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox cmbDeviceType;
        private System.Windows.Forms.TextBox txtDeviceNumber;
        private System.Windows.Forms.ComboBox cmbDevice;
        private System.Windows.Forms.CheckBox chkAllowSetConnectedTrue;
        private System.Windows.Forms.CheckBox chkAllowSetConnectedFalse;
        private System.Windows.Forms.Button btnSetup;
        private System.Windows.Forms.CheckBox ChkAllowConcurrentAccess;
    }
}
