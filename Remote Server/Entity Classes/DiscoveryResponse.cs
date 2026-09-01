using System.Net;
using System.Net.Sockets;

namespace ASCOM.Remote
{
    internal class DiscoveryResponse(uint discoveryNumber, UdpClient udpClient, byte[] response, IPEndPoint remoteEndpoint)
    {
        internal uint DiscoveryNumber { get; } = discoveryNumber;
        internal UdpClient UdpClient { get; } = udpClient;
        internal byte[] Response { get; } = response;
        internal IPEndPoint RemoteEndpoint { get; } = remoteEndpoint;
    }
}
