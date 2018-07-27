using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.Interfaces
{
    public interface IDnsClient
    {
        IPAddress ResolveHostName(string hostname);

        Task<IPAddress> ResolveHostNameAsync(string hostname);
    }
}
