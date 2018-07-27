using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using HostFile.Libs.Updater.Dns;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Updater.DataFiles
{
    public class FileUpdater : IFileUpdater
    {
        private readonly IAppSetting _appSetting;
        private readonly ILogger _logger;

        public FileUpdater(IAppSetting appSetting, ILogger logger)
        {
            _appSetting = appSetting;
            _logger = logger;
        }

        public void UpdateTargetFile(string dataName, string targetFilePath)
        {
            // Backup the target file. Remove all custom lines first.
            // Remove our lines from the target file (if any) using the start and end texts as guideline.
            string backupFilePath = $"{targetFilePath}.bak";
            RemoveLinesFromTargetFile(targetFilePath);
            File.Copy(targetFilePath, backupFilePath, true);
            
            // Add lines from the data file into the target file.
            AddLinesIntoTargetFile(dataName, targetFilePath);

            // Flush DNS.
            FlushDNS();

            _logger.LogInfo($"File '{targetFilePath}' updated.");
        }

        public bool RestoreTargetFile(string targetFilePath)
        {
            try
            {
                string backupFilePath = $"{targetFilePath}.bak";
                if (File.Exists(backupFilePath))
                {
                    RemoveLinesFromTargetFile(backupFilePath);
                    File.Copy(backupFilePath, targetFilePath, true);
                    File.Delete(backupFilePath);
                }
                else
                {
                    RemoveLinesFromTargetFile(targetFilePath);
                }

                FlushDNS();

                _logger.LogInfo($"File '{targetFilePath}' restored.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return false;
            }
        }

        public bool IsTargetFileDirty(string targetFilePath)
        {
            // Check if the target file still have our custom data.
            // Get the first mentioned environment name in the host file (if any).
            string envLine = File.ReadLines(targetFilePath).FirstOrDefault(x => x.Contains(_appSetting.StartLine));
            bool isDirty = !string.IsNullOrEmpty(envLine);

            _logger.LogInfo($"Check if file is dirty: {isDirty}");

            return isDirty;
        }

        /// <summary>
        /// Returns a string of current environment name set in the target file. If none, then returns an emoty string.
        /// </summary>
        /// <param name="targetFilePath"></param>
        /// <returns></returns>
        public string GetExistingTargetFileEnvironment(string targetFilePath)
        {
            string currentEnv;
            string envLine = File.ReadLines(targetFilePath).FirstOrDefault(x => x.Contains(_appSetting.StartLine));
            if (!string.IsNullOrEmpty(envLine))
            {
                currentEnv = envLine.Substring(_appSetting.StartLine.Length).Trim();
            }
            else
            {
                currentEnv = string.Empty;
            }

            _logger.LogInfo($"Current environment in host file: '{currentEnv}'");

            return currentEnv;
        }

        private void FlushDNS()
        {
            Task.Run(() => new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "cmd.exe",
                    Arguments = "/C ipconfig /flushdns"
                }
            }.Start());

            _logger.LogInfo("DNS flushed.");
        }

        private void AddLinesIntoTargetFile(string dataName, string targetFilePath)
        {
            string[] newContent;

            // Check which storage to use (local or external).
            if (_appSetting.UseLocalData)
            {

            }
            else
            {
                // TO-DO: Get data from external source.
            }

            // Get from local storage. The location is always in folder 'Data' in the app's directory.
            string dataDir = Path.Combine(Environment.CurrentDirectory, "Data");
            string filePath = Path.Combine(dataDir, $"{dataName}");
            if (!Directory.Exists(dataDir))
            {
                // The folder is missing, so create a new one, and also an empty file.
                Directory.CreateDirectory(dataDir);
                File.WriteAllLines(filePath, Array.Empty<string>());
            }

            // Read the data file.
            newContent = File.ReadAllLines(filePath);

            _logger.LogInfo($"{newContent.Count()} lines retrieved from source for {dataName}.");

            // Read strings from the file.
            var content = File.ReadAllLines(targetFilePath).ToList();

            // Add the start text.
            content.Add($"{_appSetting.StartLine} {dataName}");

            // Add new content into the list.
            content.AddRange(newContent);

            // Add the end text.
            content.Add($"{_appSetting.EndLine} {dataName}");

            _logger.LogInfo($"{content.Count} lines ready to be written into file '{targetFilePath}'.");

            // Write into the file.
            File.WriteAllLines(targetFilePath, content);
        }

        /// <summary>
        /// Remove all lines between the specified start and end strings for the specified text file.
        /// </summary>
        /// <param name="targetFilePath">The file path of targeted text file.</param>
        private void RemoveLinesFromTargetFile(string targetFilePath)
        {
            // Returns if not exists.
            if (!File.Exists(targetFilePath))
            {
                _logger.LogError($"File '{targetFilePath}' does not exists. No file is modified.");
                return;
            }
            
            // Read strings from file.
            var contents = File.ReadAllLines(targetFilePath).ToList();
            _logger.LogInfo($"{contents.Count} lines read from file '{targetFilePath}'");

            // Get the start and end indexes from the list.
            int startIndex = contents.FindIndex(line => line.Contains(_appSetting.StartLine));
            int endIndex = contents.FindIndex(line => line.Contains(_appSetting.EndLine));

            // Only modify the file if the conditions below are satisfied.
            if (startIndex > -1 && (endIndex > 0 && endIndex > startIndex))
            {
                // Remove lines from the list based on the start and end indexes.
                int removeCount = endIndex + 1 - startIndex;
                contents.RemoveRange(startIndex, removeCount);
                _logger.LogInfo($"{removeCount} lines removed with start index '{startIndex}'.");

                // Overwrite all lines in the file.
                File.WriteAllLines(targetFilePath, contents);
            }
        }
    }
}
