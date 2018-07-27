using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.UI.Updater.Interfaces.Factory
{
    public interface IAppFactory
    {
        IAppSetting GetAppSetting();

        ILogger GetLogger();
    }
}
