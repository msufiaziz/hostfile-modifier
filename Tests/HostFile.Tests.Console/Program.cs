using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostFile.Tests.Console1
{
    static class Program
    {
        class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler()
            {
                InnerHandler = new HttpClientHandler();
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.Write($"{request.Method} {request.RequestUri.AbsolutePath}... ");
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine($"...{response.StatusCode}.");
                return response;
            }
        }

        private static ILogger _logger;

        static void Main(string[] args)
        {
            Console.Write("\nPress any key to exit...");
            Console.ReadKey();
        }

        static IEnumerable<HostItem> GetUpdatedList(Source sourceData, IEnumerable<string> contents)
        {
            IDnsClient dnsClient = new Libs.Updater.Dns.DnsClient(_logger, IPAddress.Parse(sourceData.FirstDNS), IPAddress.Parse(sourceData.SecondDNS));
            var concurrentBag = new ConcurrentBag<HostItem>();
            var tasks = new ConcurrentBag<Task>();

            var details = contents.Select(line =>
            {
                var arr = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                return new HostItem { HostName = arr[1], IPAddress = IPAddress.Parse(arr[0]) };
            });

            // Create and add a task for each of the lines.
            // Update the IP address for each hostnames. If failed, its default value will be used as output.
            Parallel.ForEach(details, item => 
            {
                tasks.Add(Task.Run(async () => 
                {
                    var newItem = new HostItem { HostName = item.HostName };
                    var ipAddress = await dnsClient.ResolveHostNameAsync(item.HostName);
                    if (ipAddress != null)
                    {
                        if (!ipAddress.Equals(item.IPAddress))
                        {
                            newItem.IPAddress = ipAddress;
                            newItem.Status = "updated";
                        }
                        else
                        {
                            // Same value, so just use its default value.
                            newItem.IPAddress = item.IPAddress;
                        }
                    }
                    else
                    {
                        // Failed to resolve, so use its default IP address.
                        newItem.IPAddress = item.IPAddress;
                        newItem.Status = "not found";
                    }

                    concurrentBag.Add(newItem);
                }));
            });

            Task.WaitAll(tasks.ToArray());

            return concurrentBag;
        }
    }
}
