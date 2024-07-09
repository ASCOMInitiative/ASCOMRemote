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
