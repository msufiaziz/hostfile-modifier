using HostFile.Libs.Contracts.Interfaces;
using HostFile.Libs.Updater.DataFiles;
using HostFile.Libs.Updater.Dns;
using HostFile.Libs.Utilities;
using Ninject.Modules;
using System;
using System.Net.Http;

namespace HostFile.UI.Modifier
{
    public class LoadModule : NinjectModule
    {
        public override void Load()
        {
            var settings = AppSetting.GetAppSettings("appSettings.json");
            Bind<IAppSetting>().ToConstant(settings);
            Bind<IDnsManager>().To<DnsManager>();
            Bind<IFileUpdater>().To<FileUpdater>();
            Bind<ILogger>().To<Logger>()
                           .InSingletonScope()
                           .WithConstructorArgument("name", "dnsResolver")
                           .WithConstructorArgument("logFilePath", settings.LogDir);
            
            Bind<HttpClient>().ToSelf()
                              .InSingletonScope();
        }
    }
}
