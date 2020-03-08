using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace ASCOM.Remote
{
    /// <summary>
    /// Class that presents all usable IPv4 and IPv6 addresses on the host
    /// </summary>
    internal class HostIpAddresses
    {
        /// <summary>
        /// List of IPv4 addresses
        /// </summary>
        public List<IPAddress> IpV4Addresses { get; }

        /// <summary>
        /// List of IPv6 addresses
        /// </summary>
        public List<IPAddress> IpV6Addresses { get; }

        /// <summary>
        /// Class initialiser
        /// </summary>
        public HostIpAddresses()
        {
            // Initialise the IPv4 and IPv6 address lists
            IpV4Addresses = new List<IPAddress>();
            IpV6Addresses = new List<IPAddress>();

            // Get an array of all network interfaces on this host
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            // Iterate over each network adapter looking for usable IP addresses
            foreach (NetworkInterface adapter in adapters)
            {
                // Only test operational adapters
                if (adapter.OperationalStatus == OperationalStatus.Up)
                {
                    // Get the adapter's properties
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();

                    // If the adapter has properties get the collection of unicast addresses
                    if (adapterProperties != null)
                    {
                        // Get the collection of unicast addresses
                        UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;

                        // If there are some unicast IP addresses get these
                        if (uniCast.Count > 0)
                        {
                            // Iterate over the unicast addresses 
                            foreach (UnicastIPAddressInformation uni in uniCast)
                            {
                                // Save IPv4 addresses to the IPv4 list
                                if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    IpV4Addresses.Add(uni.Address);
                                }

                                // Save IPv6 addresses to the IPv6 list
                                if (uni.Address.AddressFamily == AddressFamily.InterNetworkV6)
                                {
                                    IpV6Addresses.Add(uni.Address);
                                }
                            }
                        }
                    }
                }
            }

            IpV4Addresses.Sort(CompareIPaddresses);
            IpV6Addresses.Sort(CompareIPaddresses);
        }

        private int CompareIPaddresses(IPAddress x, IPAddress y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal. 
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater. 
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the 
                    // lengths of the two strings.
                    //
                    return x.ToString().CompareTo(y.ToString());

                }
            }
        }
    }
}
