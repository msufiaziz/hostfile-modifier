using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Updater.Configuration
{
    public class AppSetting : IAppSetting
    {
        public string HostFilePath { get; set; }
        public string StartLine { get; set; }
        public string EndLine { get; set; }
        public ExternalStorage ExternalStorage { get; set; }
        public IEnumerable<Source> Sources { get; set; }
        public string LogDir { get; set; }

        public static IAppSetting GetAppSettings(string path)
        {
            string jsonString = File.ReadAllText(path);
            return JObject.Parse(jsonString).ToObject<AppSetting>();
        }
    }
}
