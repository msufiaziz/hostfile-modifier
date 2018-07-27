using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.UI.Updater.Handlers
{
    public class UpdateHandler
    {
        private readonly IAppSetting _appSetting;
        private readonly ILogger _logger;

        public UpdateHandler(IAppSetting appSetting, ILogger logger)
        {
            _appSetting = appSetting;
            _logger = logger;
        }

        public async Task<bool> UpdateDataAsync(IEnumerable<Source> sources, IProgress<string> updateProgress = null)
        {
            updateProgress.Report("Data update operation started.");

            bool isSuccess = true;
            string errorMessage = "";

            // TO-DO: Store this exclusion list somewhere else.
            var excludeList = new string[] { "Default", "Google" };
            foreach (Source sourceData in sources.Where(s => !excludeList.Contains(s.Name)))
            {
                // Get data for current source.
                string filePath = Path.Combine(_appSetting.StorageInfo.Path, sourceData.Name);
                var contents = File.ReadAllLines(filePath);
                
                try
                {
                    // Update data.
                    var updatedList = (await GetUpdatedListAsync(sourceData, contents)).OrderBy(item => item.HostName)
                                                                                       .Select(item => $"{item.IPAddress}\t\t{item.HostName}");

                    // Rewrite the source file.
                    File.WriteAllLines(filePath, updatedList);

                    // Set to 'True'.
                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    isSuccess = true;
                    errorMessage = ex.ToString();
                }
                
                string status = isSuccess ? "Success" : "Fail";
                updateProgress?.Report($"Update status ({sourceData.Name}): {status}.");

                // Immediately break this loop if failed.
                if (!isSuccess)
                {
                    updateProgress?.Report($"Update operation failed and stopped immediately. \n{errorMessage}");
                    break;
                }
            }

            updateProgress.Report("Data update operation finished.");

            return isSuccess;
        }

        private async Task<IEnumerable<HostItem>> GetUpdatedListAsync(Source sourceData, IEnumerable<string> contents)
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
                    // Initialize with default values.
                    var newItem = new HostItem { HostName = item.HostName, IPAddress = item.IPAddress };

                    IPAddress ipAddress = null;
                    try
                    {
                        ipAddress = await dnsClient.ResolveHostNameAsync(item.HostName);
                        if (ipAddress != null)
                        {
                            // Value not same, so update it.
                            if (!ipAddress.Equals(item.IPAddress))
                            {
                                newItem.IPAddress = ipAddress;
                                newItem.Status = "updated";
                            }
                        }
                        else
                        {
                            // Failed to resolve, so use its default IP address.
                            newItem.Status = "not found";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogException(ex);
                        newItem.Status = ex.Message;
                    }
                    
                    concurrentBag.Add(newItem);
                }));
            });

            await Task.WhenAll(tasks);

            return concurrentBag;
        }
    }
}
