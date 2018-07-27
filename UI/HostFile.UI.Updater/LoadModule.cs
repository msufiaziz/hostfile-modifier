using HostFile.Libs.Contracts.Interfaces;
using HostFile.Libs.Utilities;
using HostFile.UI.Updater.Handlers;
using HostFile.UI.Updater.Interfaces.Factory;
using Ninject;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HostFile.UI.Updater
{
    public class LoadModule : NinjectModule
    {
        public override void Load()
        {
            var appSetting = AppSetting.GetAppSettings("appSettings.json");
            Bind<IAppSetting>().ToConstant(appSetting)
                               .NamedLikeFactoryMethod((IAppFactory f) => f.GetAppSetting());
            Bind<ILogger>().To<Logger>()
                           .InSingletonScope()
                           .NamedLikeFactoryMethod((IAppFactory f) => f.GetLogger())
                           .WithConstructorArgument("name", "updater")
                           .WithConstructorArgument("logFilePath", appSetting.LogDir);

            var logger = Kernel.Get<ILogger>();
            Bind<HttpClient>().ToSelf()
                              .InSingletonScope()
                              .WithConstructorArgument("handler", new LoggingHandler(logger));

            Bind<IAppFactory>().ToFactory();
        }
    }
}
