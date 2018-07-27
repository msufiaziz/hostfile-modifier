using HostFile.Libs.Updater;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Modifier
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Make sure that this application is run with admin privileges.
            bool isElevated;
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (!isElevated)
                MessageBox.Show("Please run this application with administrator privileges.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                // Load the dependency injection component.
                var kernel = new StandardKernel();
                kernel.Load(Assembly.GetExecutingAssembly());

                var mainAppContext = kernel.Get<MainAppContext>();
                Application.Run(mainAppContext);
            }
        }
    }
}
