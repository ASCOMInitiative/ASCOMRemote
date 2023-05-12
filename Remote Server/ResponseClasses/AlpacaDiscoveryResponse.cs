using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASCOM.Remote
{
    public class AlpacaDiscoveryResponse
    {
        public AlpacaDiscoveryResponse() { }

        public AlpacaDiscoveryResponse(int alpacaPort)
        {
            AlpacaPort = alpacaPort;
        }

        public int AlpacaPort { get; set; }
    }
}
