namespace ASCOM.Remote
{
    /// <summary>
    /// Data response class to return the server device configuration. i.e. which devices are accessible through the http/JSON interface
    /// </summary>
    public class ConfiguredDevice
    {
        public ConfiguredDevice(string deviceType, string progID, string description, int deviceNumber, bool allowConnectedSetFalse, bool allowConnectedSetTrue)
        {
            DeviceType = deviceType;
            ProgID = progID;
            DeviceNumber = deviceNumber;
            Description = description;
            AllowConnectedSetFalse = allowConnectedSetFalse;
            AllowConnectedSetTrue = allowConnectedSetTrue;
        }

        public string DeviceType { get; set; }
        public string ProgID { get; set; }
        public string Description { get; set; }
        public int DeviceNumber { get; set; }
        public bool AllowConnectedSetFalse { get; set; }
        public bool AllowConnectedSetTrue { get; set; }
    }
}
