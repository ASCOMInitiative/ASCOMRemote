namespace ASCOM.Remote
{
    public class ProfileDevice(string deviceType, string progID, string description)
    {
        public string DeviceType { get; set; } = deviceType;
        public string ProgID { get; set; } = progID;
        public string Description { get; set; } = description;
    }
}
