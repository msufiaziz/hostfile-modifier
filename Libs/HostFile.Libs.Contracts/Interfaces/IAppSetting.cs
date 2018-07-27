using HostFile.Libs.Contracts.DataContracts;
using System.Collections.Generic;

namespace HostFile.Libs.Contracts.Interfaces
{
    public interface IAppSetting
    {
        string EndLine { get; set; }
        StorageInfo StorageInfo { get; set; }
        string HostFilePath { get; set; }
        IEnumerable<Source> Sources { get; set; }
        string StartLine { get; set; }
        string LogDir { get; set; }
        bool UseLocalData { get; set; }
    }
}