using HostFile.Libs.Contracts.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.Interfaces
{
    public interface IDnsManager
    {
        DnsObject GetDNS();

        void SetDNS(DnsObject dnsObject = null);

        IPAddress GetIPAddress(IDnsClient client, string hostname);

        Task<IPAddress> GetIPAddressAsync(IDnsClient client, string hostname);
    }
}
