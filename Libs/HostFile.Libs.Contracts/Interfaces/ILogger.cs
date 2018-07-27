using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.Interfaces
{
    public interface ILogger
    {
        void LogInfo(string message);

        void LogError(string message);

        void LogException(Exception ex);

        void LogWarning(string message);
    }
}
