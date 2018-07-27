using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Utilities
{
    /// <summary>
    /// Represents a thread-safe class that able to log info and errors into hard disk. Implements the <see cref="ILogger"/> interface.
    /// </summary>
    public class Logger : ILogger
    {
        private readonly string _logFilePath;
        private readonly string _fullLogFilePath;

        private readonly BlockingCollection<string> _bc = new BlockingCollection<string>();

        public Logger(string name, string logFilePath)
        {
            _logFilePath = logFilePath;
            
            _fullLogFilePath = Path.Combine(logFilePath, $"{name}.log");

            // Constructor create the consumer thread that wait for work on .GetConsumingEnumerable().
            Task.Factory.StartNew(() => LogConsumer());
        }

        ~Logger()
        {
            // Free the writing thread.
            _bc.CompleteAdding();
        }

        public void LogError(string message)
        {
            string formattedString = FormatString("ERROR", message);
            _bc.Add(formattedString);
        }

        public void LogException(Exception ex)
        {
            string message = ex.ToString();
            string formattedString = FormatString("EXCEPTION", message);
            _bc.Add(formattedString);
        }

        public void LogInfo(string message)
        {
            string formattedString = FormatString("INFO", message);
            _bc.Add(formattedString);
        }

        public void LogWarning(string message)
        {
            string formattedString = FormatString("WARNING", message);
            _bc.Add(formattedString);
        }

        private string FormatString(string type, string message)
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
            return $"[{dateString}][{type}] - {message}{Environment.NewLine}";
        }

        /// <summary>
        /// Represents the consumer part for the logging mechanism.
        /// </summary>
        private void LogConsumer()
        {
            // Return immediately if the directory specified is not exists.
            if (!Directory.Exists(_logFilePath))
            {
                return;
            }

            foreach (string line in _bc.GetConsumingEnumerable())
            {
                File.AppendAllText(_fullLogFilePath, line);
            }
        }
    }
}
