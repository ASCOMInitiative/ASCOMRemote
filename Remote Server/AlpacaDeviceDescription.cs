using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public class AlpacaDeviceDescription
    {
        public AlpacaDeviceDescription() { }

        public AlpacaDeviceDescription(string serverName, string manufacturer, string manufacturerVersion, string location)
        {
            ServerName = serverName;
            Manufacturer = manufacturer;
            ManufacturerVersion = manufacturerVersion;
            Location = location;
        }

        public string ServerName { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string ManufacturerVersion { get; set; } = "";
        public string Location { get; set; } = "";
    }
}
