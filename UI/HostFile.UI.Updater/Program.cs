using HostFile.UI.Updater.Interfaces.Factory;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Updater
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Startup();
        }

        private static void Startup()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var kernel = new StandardKernel();
            kernel.Load(new LoadModule());

            var mainform = kernel.Get<MainForm>();
            Application.Run(mainform);
        }
    }
}
