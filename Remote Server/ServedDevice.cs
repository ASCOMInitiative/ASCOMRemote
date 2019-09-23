using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ASCOM.Utilities;
using System.Collections;
using System.Runtime.InteropServices;

namespace ASCOM.Remote
{
    public partial class ServedDevice : UserControl
    {

        #region Variables

        int deviceNumber = 0;
        string description = "";
        string progID = "";
        bool devicesAreConnected = false;

        Profile profile;
        List<string> deviceTypes;
        Dictionary<string, string> deviceDictionary;
        SetupForm setupForm;
        bool recalculate = false;

        #endregion

        #region Initialisers
        public ServedDevice()
        {
            InitializeComponent();

            // Create generic lists
            deviceTypes = new List<string>();
            deviceDictionary = new Dictionary<string, string>();

            cmbDeviceType.MouseUp += CmbDeviceType_MouseUp; // To force a device number recalculation if the device type is changed

            // The combo boxes have to be self painted because the DropDownStyle is DropDownList, to make the list read only, and this changes the background colour to grey!
            cmbDevice.DrawMode = DrawMode.OwnerDrawFixed;
            cmbDevice.DrawItem += new DrawItemEventHandler(ComboBox_DrawItem); // Attach an event handler to draw the combo box on demand
            cmbDeviceType.DrawMode = DrawMode.OwnerDrawFixed;
            cmbDeviceType.DrawItem += new DrawItemEventHandler(ComboBox_DrawItem); // Attach an event handler to draw the combo box on demand
        }

        public void InitUI(SetupForm parent)
        {
            setupForm = parent;
            ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Start");
            profile = new Profile();
            ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Created Profile");

            cmbDeviceType.Items.Add(SharedConstants.DEVICE_NOT_CONFIGURED);
            ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Added Device not configured");


            foreach (string deviceType in profile.RegisteredDeviceTypes)
            {
                ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Adding item: " + deviceType);
                cmbDeviceType.Items.Add(deviceType);
                deviceTypes.Add(deviceType); // Remember the device types on this system
            }
            ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Setting selected index to 0");

            cmbDeviceType.SelectedIndex = 0;

            ServerForm.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Finished");

        }
        #endregion

        #region Data accessor properties

        public int DeviceNumber
        {
            get
            {
                return deviceNumber;
            }
            set
            {
                deviceNumber = value;
                txtDeviceNumber.Text = value.ToString();
            }
        }

        public string DeviceType
        {
            get
            {
                try
                {
                    return cmbDeviceType.SelectedItem.ToString();
                }
                catch
                {
                    return "None";
                }
            }
            set
            {
                try
                {
                    cmbDeviceType.SelectedItem = value;
                }
                catch
                {
                    cmbDeviceType.SelectedIndex = -1; ;
                }
            }
        }

        public string Description
        {
            get
            {
                try
                {
                    return description;
                }
                catch
                {
                    return "";
                }
            }
            set
            {
                try
                {
                    description = value;
                    cmbDevice.SelectedItem = value.ToString();
                }
                catch { }
            }
        }

        public string ProgID
        {
            get
            {
                return progID;
            }
            set
            {
                progID = value;
                if (!DesignMode)
                {
                    try
                    {
                        ServerForm.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Set ProgID to: " + progID);
                        switch (progID)
                        {
                            case "":
                                cmbDevice.SelectedIndex = -1;
                                break;
                            case SharedConstants.DEVICE_NOT_CONFIGURED:
                                ServerForm.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Description: " + SharedConstants.DEVICE_NOT_CONFIGURED);
                                cmbDevice.SelectedItem = 0;
                                break;
                            default:
                                ServerForm.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Description: " + deviceDictionary[progID]);
                                cmbDevice.SelectedItem = deviceDictionary[progID];
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ServerForm.LogException(0, 0, 0, "ServedDevice.ProgID", ex.ToString());
                        cmbDevice.SelectedIndex = -1;
                    }
                }
            }
        }

        public bool AllowConnectedSetFalse
        {
            get
            {
                return chkAllowSetConnectedFalse.Checked;
            }
            set
            {
                chkAllowSetConnectedFalse.Checked = value;
            }
        }

        public bool AllowConnectedSetTrue
        {
            get
            {
                return chkAllowSetConnectedTrue.Checked;
            }
            set
            {
                chkAllowSetConnectedTrue.Checked = value;
            }
        }

        public bool DevicesAreConnected
        {
            get
            {
                return devicesAreConnected;
            }
            set
            {
                devicesAreConnected = value;
                btnSetup.Enabled = !devicesAreConnected;
            }
        }

        public bool AllowConcurrentAccess
        {
            get
            {
                return ChkAllowConcurrentAccess.Checked;
            }
            set
            {
                ChkAllowConcurrentAccess.Checked = value;
            }
        }
        #endregion

        #region Event handlers
        private void CmbDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Clearing items");
                cmbDevice.Items.Clear();

                ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Setting selected index to -1");
                cmbDevice.SelectedIndex = -1;

                ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Resetting instance number");
                DeviceNumber = 0;

                if (cmbDeviceType.SelectedItem.ToString() == SharedConstants.DEVICE_NOT_CONFIGURED)
                {
                    ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - \"None\" device type selected");
                    cmbDevice.Items.Clear();
                    cmbDevice.SelectedIndex = -1;
                    cmbDevice.ResetText();
                    cmbDevice.Enabled = false;
                    description = "";
                    progID = SharedConstants.DEVICE_NOT_CONFIGURED;

                }
                else
                {
                    cmbDevice.Enabled = true;
                    ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Real device type has been selected");
                }


                if (recalculate)
                {
                    setupForm.RecalculateDeviceNumbers();
                    recalculate = false;
                }

                // Set up device list so we can translate ProgID to description

                ArrayList installedDevices = profile.RegisteredDevices(cmbDeviceType.SelectedItem.ToString());
                ServerForm.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Created registered device array list");

                deviceDictionary.Clear();
                foreach (KeyValuePair kvp in installedDevices)
                {
                    if (!deviceDictionary.ContainsKey(kvp.Value)) deviceDictionary.Add(kvp.Key, kvp.Value);
                    cmbDevice.Items.Add(kvp.Value);
                }
                if (cmbDevice.Items.Count > 0) cmbDevice.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void CmbDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            description = cmbDevice.SelectedItem.ToString();
            foreach (KeyValuePair<string, string> kvp in deviceDictionary)
            {
                if (kvp.Value == description)
                {
                    progID = kvp.Key;
                }
            }
        }

        private void CmbDeviceType_MouseUp(object sender, MouseEventArgs e)
        {
            recalculate = true;
        }

        /// <summary>
        /// Event handler to paint the device list combo box in the "DropDown" rather than "DropDownList" style
        /// </summary>
        /// <param name="sender">Device to be painted</param>
        /// <param name="e">Draw event arguments object</param>
        public void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
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

        private void BtnSetup_Click(object sender, EventArgs e)
        {
            // This device's ProgID is held in the variable progID so try and run its SetupDialog method
            ServerForm.LogMessage(0, 0, 0, "Setup", string.Format("Setup button pressed for device: {0}, ProgID: {1}", cmbDevice.Text, progID));

            try
            {
                // Get an instance of the driver from its ProgID and store this in a dynamic variable so that we can call its method directly
                Type ProgIdType = Type.GetTypeFromProgID(progID);
                ServerForm.LogMessage(0, 0, 0, "Setup", string.Format("Found type: {0}", ProgIdType.Name));

                dynamic oDrv = Activator.CreateInstance(ProgIdType);
                ServerForm.LogMessage(0, 0, 0, "Setup", "Created driver instance OK");

                try
                {
                    if (oDrv.Connected) // Driver is connected and the Setup dialogue must be run with the device disconnected so ask whether we can disconnect it
                    {
                        DialogResult dialogResult = MessageBox.Show("Device is connected, OK to disconnect and run Setup?", "Disconnect Device?", MessageBoxButtons.OKCancel);
                        if (dialogResult == DialogResult.OK) // OK to disconnect and run setup dialogue
                        {
                            ServerForm.LogMessage(0, 0, 0, "Setup", "User gave permission to disconnect device - setting Connected to false");
                            oDrv.Connected = false;

                            int RemainingObjectCount = Marshal.FinalReleaseComObject(oDrv);
                            oDrv = null;
                            oDrv = Activator.CreateInstance(ProgIdType);

                            ServerForm.LogMessage(0, 0, 0, "Setup", string.Format("Connected has bee set false and destroyed. New Connected value: {0}", oDrv.Connected));

                            ServerForm.LogMessage(0, 0, 0, "Setup", "Device is now disconnected, calling SetupDialog method");
                            oDrv.SetupDialog();
                            ServerForm.LogMessage(0, 0, 0, "Setup", "Completed SetupDialog method, setting Connected to true");
                            oDrv.Connected = true;
                            ServerForm.LogMessage(0, 0, 0, "Setup", "Driver is now Connected");
                        }
                        else // Not OK to disconnect so just do nothing and exit
                        {
                            ServerForm.LogMessage(0, 0, 0, "Setup", "User did not give permission to disconnect device - no action taken");
                        }
                    }
                    else // Driver is not connected 
                    {
                        ServerForm.LogMessage(0, 0, 0, "Setup", "Device is disconnected so just calling SetupDialog method");
                        oDrv.SetupDialog();
                        ServerForm.LogMessage(0, 0, 0, "Setup", "Completed SetupDialog method");

                        try { oDrv.Dispose(); } catch { }; // Dispose the driver if possible

                        // Release the COM object properly
                        try
                        {
                            ServerForm.LogMessage(0, 0, 0, "Setup", "  Releasing COM object");
                            int LoopCount = 0;
                            int RemainingObjectCount = 0;

                            do
                            {
                                LoopCount += 1; // Increment the loop counter so that we don't go on for ever!
                                RemainingObjectCount = Marshal.ReleaseComObject(oDrv);
                                ServerForm.LogMessage(0, 0, 0, "Setup", "  Remaining object count: " + RemainingObjectCount.ToString() + ", LoopCount: " + LoopCount);
                            } while ((RemainingObjectCount > 0) & (LoopCount < 20));
                        }
                        catch (Exception ex2)
                        {
                            ServerForm.LogMessage(0, 0, 0, "Setup", "  ReleaseComObject Exception: " + ex2.Message);
                        }

                        oDrv = null;
                    }
                }
                catch (Exception ex1)
                {
                    string errMsg = string.Format("Exception calling SetupDialog method: {0}", ex1.Message);
                    MessageBox.Show(errMsg);
                    ServerForm.LogMessage(0, 0, 0, "Setup", errMsg);
                    ServerForm.LogException(0, 0, 0, "Setup", ex1.ToString());
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Exception creating driver {0} - {1}", progID, ex.Message);
                MessageBox.Show(errMsg);
                ServerForm.LogMessage(0, 0, 0, "Setup", errMsg);
                ServerForm.LogException(0, 0, 0, "Setup", ex.ToString());
            }
        }

        #endregion
    }
}
