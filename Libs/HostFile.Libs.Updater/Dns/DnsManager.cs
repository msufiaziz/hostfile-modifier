using DnsClient;
using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Updater.Dns
{
    public class DnsManager : IDnsManager
    {
        private const string Path = "Win32_NetworkAdapterConfiguration";
        private const string IPEnabled = "IPEnabled";
        private const string SetDNSServerSearchOrder = "SetDNSServerSearchOrder";
        private const string DNSServerSearchOrder = "DNSServerSearchOrder";

        private readonly ILogger _logger;

        public DnsManager(ILogger logger)
        {
            _logger = logger;
        }

        public DnsObject GetDNS()
        {
            string[] dnses = null;
            using (var mc = new ManagementClass(Path))
            {
                using (var moCollection = mc.GetInstances())
                {
                    var managementObjList = moCollection.OfType<ManagementObject>()
                                                        .Where(mo => (bool)mo["IPEnabled"]);

                    foreach (var mo in managementObjList)
                    {
                        string caption = mo["Caption"].ToString();
                        //string[] ipAddress = (string[])mo["IPAddress"];
                        //string[] subnets = (string[])mo["IPSubnet"];
                        //string[] gateways = (string[])mo["DefaultIPGateway"];
                        //bool isEnabled = (bool)mo[IPEnabled];
                        dnses = (string[])mo["DNSServerSearchOrder"];

                        _logger.LogInfo($"Found device '{caption}' as IP-enabled.");
                    }
                }
            }

            if (dnses != null)
            {
                if (dnses.Length < 2)
                {
                    _logger.LogInfo($"DNS found: {dnses[0]}.");
                    return new DnsObject(dnses[0], string.Empty);
                }
                else
                {
                    _logger.LogInfo($"DNS found: {dnses[0]}, {dnses[1]}.");
                    return new DnsObject(dnses[0], dnses[1]);
                }
            }

            _logger.LogInfo("No DNS address found.");
            return null;
        }

        public IPAddress GetIPAddress(IDnsClient client, string hostname)
        {
            return client.ResolveHostName(hostname);
        }

        public async Task<IPAddress> GetIPAddressAsync(IDnsClient client, string hostname)
        {
            return await client.ResolveHostNameAsync(hostname);
        }

        public void SetDNS(DnsObject dnsObject = null)
        {
            string[] dnsArray = null;
            if (dnsObject != null)
            {
                // The FirstAddress property is required. SecondAddress property is optional.
                if (string.IsNullOrEmpty(dnsObject.FirstAddress))
                {
                    var exception = new ArgumentException("The first DNS address is required.");
                    _logger.LogException(exception);
                    throw exception;
                }

                var dnsList = new List<string>() { dnsObject.FirstAddress };
                if (!string.IsNullOrEmpty(dnsObject.SecondAddress))
                    dnsList.Add(dnsObject.SecondAddress);
                dnsArray = dnsList.ToArray();
            }
            
            using (var mc = new ManagementClass(Path))
            {
                using (var moCollection = mc.GetInstances())
                {
                    var managementObjList = moCollection.OfType<ManagementObject>()
                                                        .Where(mo => (bool)mo[IPEnabled]);

                    foreach (var mo in managementObjList)
                    {
                        using (var managementBaseObj = mo.GetMethodParameters(SetDNSServerSearchOrder))
                        {
                            managementBaseObj[DNSServerSearchOrder] = dnsArray;
                            mo.InvokeMethod(SetDNSServerSearchOrder, managementBaseObj, null);
                        }

                        string caption = mo["Caption"].ToString();
                        _logger.LogInfo($"DNS values for device '{caption}' is modified.");
                    }
                }
            }
        }
    }
}
