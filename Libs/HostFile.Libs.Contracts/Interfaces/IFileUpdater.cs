using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.Libs.Contracts.Interfaces
{
    public interface IFileUpdater
    {
        /// <summary>
        /// Returns a string of current environment name set in the target file. If none, then returns an emoty string.
        /// </summary>
        /// <param name="targetFilePath"></param>
        /// <returns></returns>
        string GetExistingTargetFileEnvironment(string targetFilePath);

        bool IsTargetFileDirty(string targetFilePath);

        bool RestoreTargetFile(string targetFilePath);

        void UpdateTargetFile(string dataName, string targetFilePath);
    }
}
