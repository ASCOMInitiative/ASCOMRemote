namespace ASCOM.Remote
{
    /// <summary>
    /// Class to hold COM activation and access information for a served device
    /// </summary>
    public class ConfiguredDevice(string deviceType, string progID, string description, int deviceNumber, bool allowConnectedSetFalse, bool allowConnectedSetTrue, bool allowConcurrentAccess, string uniqueId)
    {
        public string DeviceType { get; set; } = deviceType;
        public string ProgID { get; set; } = progID;
        public string Description { get; set; } = description;
        public int DeviceNumber { get; set; } = deviceNumber;
        public bool AllowConnectedSetFalse { get; set; } = allowConnectedSetFalse;
        public bool AllowConnectedSetTrue { get; set; } = allowConnectedSetTrue;
        public bool AllowConcurrentAccess { get; set; } = allowConcurrentAccess;
        public string UniqueID { get; set; } = uniqueId;

        /// <summary>
        /// Return a unique key for this device based on its device type and device number
        /// </summary>
        public string DeviceKey
        {
            get
            {
                return $"{DeviceType.ToLowerInvariant()}/{DeviceNumber}"; // Create the device key
            }
        }
    }
}
