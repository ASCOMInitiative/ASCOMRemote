using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ASCOM.Utilities;
using System.Collections;

namespace ASCOM.Remote
{
    public partial class ServedDevice : UserControl
    {

        #region Variables

        int deviceNumber = 0;
        string description = "";
        string progID = "";

        Profile profile;
        List<string> deviceTypes;
        Dictionary<string, string> deviceDictionary;
        SetupForm setupForm;
        bool recalculate = false;
        TraceLoggerPlus TL;

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

        public void InitUI(SetupForm parent, TraceLoggerPlus Logger)
        {
            setupForm = parent;
            TL = Logger;
            TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Start");
            profile = new Profile();
            TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Created Profile");

            cmbDeviceType.Items.Add(SharedConstants.DEVICE_NOT_CONFIGURED);
            TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Added Device not configured");


            foreach (string deviceType in profile.RegisteredDeviceTypes)
            {
                TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Adding item: " + deviceType);
                cmbDeviceType.Items.Add(deviceType);
                deviceTypes.Add(deviceType); // Remember the device types on this system
            }
            TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Setting selected index to 0");

            cmbDeviceType.SelectedIndex = 0;
            TL.LogMessage(0, 0, 0, "ServedDevice.InitUI", "Finished");

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
                        if (TL != null) TL.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Set ProgID to: " + progID);
                        switch (progID)
                        {
                            case "":
                                cmbDevice.SelectedIndex = -1;
                                break;
                            case SharedConstants.DEVICE_NOT_CONFIGURED:
                                TL.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Description: " + SharedConstants.DEVICE_NOT_CONFIGURED);
                                cmbDevice.SelectedItem = 0;
                                break;
                            default:
                                TL.LogMessage(0, 0, 0, "ServedDevice.ProgID", "Description: " + deviceDictionary[progID]);
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

        #endregion

        #region Event handlers
        private void CmbDeviceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {

                TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Clearing items");
                cmbDevice.Items.Clear();

                TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Setting selected index to -1");
                cmbDevice.SelectedIndex = -1;

                TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Resetting instance number");
                DeviceNumber = 0;

                if (cmbDeviceType.SelectedItem.ToString() == SharedConstants.DEVICE_NOT_CONFIGURED)
                {
                    TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - \"None\" device type selected");
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
                    TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Real device type has been selected");
                }


                if (recalculate)
                {
                    setupForm.RecalculateDeviceNumbers();
                    recalculate = false;
                }

                // Set up device list so we can translate ProgID to description

                ArrayList installedDevices = profile.RegisteredDevices(cmbDeviceType.SelectedItem.ToString());
                TL.LogMessage(0, 0, 0, this.Name, "cmbDeviceType_Changed - Created registered device arraylist");

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

        #endregion
    }
}
