using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using HostFile.UI.Modifier.Helpers;
using HostFile.UI.Modifier.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Modifier
{
    internal class MainAppContext : ApplicationContext
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly IAppSetting _appSetting;
        private readonly IDnsManager _dnsManager;
        private readonly IFileUpdater _fileUpdater;
        private readonly ToolStripMenuItem[] _updateDNSMenus;
        private readonly IProgress<ToolStripMenuItem[]> _hostFileProgress;

        private ToolStripMenuItem[] _updateHostFileMenus;

        public MainAppContext(IAppSetting appSetting, IDnsManager dnsManager, IFileUpdater fileUpdater)
        {
            // Get config settings.
            _appSetting = appSetting;

            // Populate fields.
            _dnsManager = dnsManager;
            _fileUpdater = fileUpdater;

            // Subscribed to the application's exit event.
            Application.ApplicationExit += OnApplicationExit;

            // Make sure the 'Data' folder exists.
            if (!Directory.Exists(_appSetting.StorageInfo.Path))
                Directory.CreateDirectory(_appSetting.StorageInfo.Path);
            
            // Construct the notification's context menu.
            var contextMenu = new ContextMenuStrip();

            // Create menus for DNS entries.
            var tempUpdateDNSMenus = new List<ToolStripItem>();
            foreach (Source source in _appSetting.Sources)
            {
                tempUpdateDNSMenus.Add(MenuBuilder.CreateMenu(source.Name).HookClickEvent(OnSetDNSMenuClicked));
            }
            _updateDNSMenus = new ToolStripMenuItem[tempUpdateDNSMenus.Count];
            tempUpdateDNSMenus.CopyTo(_updateDNSMenus);

            // Other essential menus.
            contextMenu.Items.AddRange(new ToolStripItem[]
            {
                MenuBuilder.CreateMenu($"DNS Resolver v{Application.ProductVersion}"),
                MenuBuilder.CreateMenu("Querying host file data..."),
                MenuBuilder.CreateMenu("DNS").AddManyChildMenu(_updateDNSMenus),
                MenuBuilder.CreateSeparator(),
                MenuBuilder.CreateMenu("Open Host File...").HookClickEvent(OnOpenHostFileMenuClicked),
                MenuBuilder.CreateSeparator(),
                MenuBuilder.CreateMenu("Reset DNS").HookClickEvent(OnDNSResetMenuClicked),
                MenuBuilder.CreateMenu("Clear Host File").HookClickEvent(OnHostFileResetMenuClicked),
                MenuBuilder.CreateSeparator(),
                MenuBuilder.CreateMenu("Exit").HookClickEvent(OnExitMenuClicked),
            });
            contextMenu.Renderer = new CustomToolStripMenuRenderer();

            // Make sure that the menu checked condition is refreshed everytime it is opening.
            contextMenu.Opening += OnNotifyIconContextMenuOpening;

            // Finally, the notification icon itself.
            _notifyIcon = new NotifyIcon
            {
                Icon = Resources.AppIcon,
                Text = Application.ProductName,
                Visible = true,
                ContextMenuStrip = contextMenu,
                BalloonTipIcon = ToolTipIcon.Info,
                BalloonTipTitle = Application.ProductName
            };

            // Update the notify icon's text.
            UpdateNotifyIconText();

            // Create menus for Host file data entries.
            _hostFileProgress = new Progress<ToolStripMenuItem[]>(HostFileMenusProgressHandler);

            Task.Run(() => ConstructHostFileContextMenu());
        }

        #region Events
        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Remove custom DNS settings.
            _dnsManager.SetDNS();

            // Restore the original file before exiting.
            _fileUpdater.RestoreTargetFile(_appSetting.HostFilePath);
        }

        private void OnOpenHostFileMenuClicked(object sender, EventArgs e)
        {
            Process.Start(_appSetting.HostFilePath);
        }

        private void OnSetHostFileMenuClicked(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
#if DEBUG
            Debug.WriteLine($"Updating host file to {menu.Text}...");
#endif
            _fileUpdater.UpdateTargetFile(menu.Text, _appSetting.HostFilePath);

            UpdateNotifyIconText();
#if DEBUG
            Debug.WriteLine("Host file update finished.");
#endif
        }

        private void OnDNSResetMenuClicked(object sender, EventArgs e)
        {
            // Reset DNS.
            _dnsManager.SetDNS();

            UpdateNotifyIconText();
#if DEBUG
            var currentDNS = _dnsManager.GetDNS();
            if (currentDNS != null)
                Debug.WriteLine($"DNS changed: {currentDNS.FirstAddress}, {currentDNS.SecondAddress}");
            else
                Debug.WriteLine($"DNS changed: RESET");
#endif
        }

        private void OnHostFileResetMenuClicked(object sender, EventArgs e)
        {
            // Reset host file.
            _fileUpdater.RestoreTargetFile(_appSetting.HostFilePath);

            UpdateNotifyIconText();
#if DEBUG
            if (_fileUpdater.IsTargetFileDirty(_appSetting.HostFilePath))
                Debug.WriteLine("Host file failed to be reset.");
            else
                Debug.WriteLine("Host file has been reset.");
#endif
        }

        private void OnSetDNSMenuClicked(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
            var source = _appSetting.Sources.Single(s => s.Name.Equals(menu.Text, StringComparison.OrdinalIgnoreCase));
            _dnsManager.SetDNS(new DnsObject(source.FirstDNS, source.SecondDNS));

            UpdateNotifyIconText();
#if DEBUG
            Debug.WriteLine($"DNS changed: {source.FirstDNS}, {source.SecondDNS}");
#endif
        }

        private void OnExitMenuClicked(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            ExitThread();
        }

        private void OnNotifyIconContextMenuOpening(object sender, CancelEventArgs e)
        {
            RefreshMenuCheckItems();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Create host file menus.
        /// </summary>
        /// <returns></returns>
        private void ConstructHostFileContextMenu()
        {
#if DEBUG
            Debug.WriteLine("Getting host file data...");
#endif
            // Get all files name from 'Data' folder.
            var hostFileList = new List<string>();
            hostFileList.AddRange(Directory.EnumerateFiles(_appSetting.StorageInfo.Path).Select(filename => Path.GetFileNameWithoutExtension(filename)));
            
            if (!hostFileList.Any())
            {
                // Empty list, so tell user that it has failed.
                var menu = MenuBuilder.CreateMenu("Failed to retrieve data.");
                _hostFileProgress.Report(new ToolStripMenuItem[] { menu });
            }
            else
            {
                // Create menu for each of the items.
                var tempUpdateHostFileMenus = new List<ToolStripMenuItem>();
                foreach (var name in hostFileList)
                {
                    tempUpdateHostFileMenus.Add(
                        MenuBuilder.CreateMenu(name)
                                   .HookClickEvent(OnSetHostFileMenuClicked)
                    );
                }
                _updateHostFileMenus = new ToolStripMenuItem[tempUpdateHostFileMenus.Count];
                tempUpdateHostFileMenus.CopyTo(_updateHostFileMenus);

                // Send the menus to the notify icon.
                _hostFileProgress.Report(_updateHostFileMenus);
            }
        }

        private void HostFileMenusProgressHandler(ToolStripMenuItem[] menus)
        {
            // Remove the dummy menu as we are intending to replace it.
            _notifyIcon.ContextMenuStrip.Items.RemoveAt(1);

            // Create the parent menu.
            var menu = MenuBuilder.CreateMenu("Host File");

            // If the array has only one item, meaning we have failed to retrieve data from server.
            // So we create a menu specific to tell user about that.
            if (menus.Count() == 1)
            {
                menu.AddChildMenu(menus.First());
            }
            else
            {
                menu.AddManyChildMenu(menus);
            }

            // Add new menus at index '0'.
            _notifyIcon.ContextMenuStrip.Items.Insert(1, menu);

#if DEBUG
            Debug.WriteLine("Host file menus added.");
#endif
        }

        private void UpdateNotifyIconText()
        {
            var builder = new StringBuilder();

            string environmentName = _fileUpdater.GetExistingTargetFileEnvironment(_appSetting.HostFilePath);
            if (!string.IsNullOrEmpty(environmentName))
                builder.AppendLine($"Host file pointed to: {environmentName}");
            else
                builder.AppendLine("Host file pointed to: None");

            var currentDNS = _dnsManager.GetDNS();
            if (currentDNS != null)
            {
                var sourceUsed = _appSetting.Sources.FirstOrDefault(s => s.FirstDNS == currentDNS.FirstAddress && s.SecondDNS == currentDNS.SecondAddress);
                if (sourceUsed != null)
                    builder.AppendLine($"DNS pointed to: {sourceUsed.Name}");
                else
                    builder.AppendLine($"DNS pointed to: Unknown");
            }
            else
            {
                builder.AppendLine("DNS pointed to: None");
            }

            _notifyIcon.Text = builder.ToString();
            _notifyIcon.ShowBalloonTip(0, "Status", builder.ToString(), ToolTipIcon.Info);
#if DEBUG
            Debug.WriteLine(_notifyIcon.Text);
            _notifyIcon.Text += "(DEBUG)";
#endif
        }

        private void RefreshMenuCheckItems()
        {
            // Check if not null and have items.
            if (_updateHostFileMenus != null && _updateHostFileMenus.Any())
            {
                // Uncheck all menu items.
                foreach (var menu in _updateHostFileMenus)
                    menu.Checked = false;

                // If dirty, check the menu.
                if (_fileUpdater.IsTargetFileDirty(_appSetting.HostFilePath))
                {
                    string environmentName = _fileUpdater.GetExistingTargetFileEnvironment(_appSetting.HostFilePath);
                    var menu = _updateHostFileMenus.FirstOrDefault(m => m.Text.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
                    if (menu != null)
                        menu.Checked = true;
                }
            }

            // Uncheck all menu items.
            foreach (var menu in _updateDNSMenus)
                menu.Checked = false;

            // Update DNS info.
            var currentDNS = _dnsManager.GetDNS();
            if (currentDNS != null)
            {
                var sourceUsed = _appSetting.Sources.FirstOrDefault(s => s.FirstDNS == currentDNS.FirstAddress && s.SecondDNS == currentDNS.SecondAddress);
                if (sourceUsed != null)
                {
                    var targetMenu = _updateDNSMenus.FirstOrDefault(m => m.Text == sourceUsed.Name);
                    targetMenu.Checked = targetMenu != null;
                }
            }
        }
        #endregion
    }
}
