using DnsClient;
using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Updater.Dns
{
    public class DnsClient : LookupClient, IDnsClient
    {
        private readonly ILogger _logger;

        public DnsClient(ILogger logger)
        {
            _logger = logger;
        }

        public DnsClient(ILogger logger, params IPAddress[] nameServers): base(nameServers)
        {
            _logger = logger;
        }

        public IPAddress ResolveHostName(string hostname)
        {
            return ResolveHostNameAsync(hostname).Result;
        }

        public async Task<IPAddress> ResolveHostNameAsync(string hostname)
        {
            var response = await QueryAsync(hostname, QueryType.A);
            var records = response.AllRecords.ARecords();

            if (records.Any())
            {
                var address = records.First().Address;
                _logger.LogInfo($"A record retrieved for {hostname}: {address}.");
                return address;
            }
            else
            {
                _logger.LogWarning($"No record retrieved for {hostname}.");
                return null;
            }
        }
    }
}
