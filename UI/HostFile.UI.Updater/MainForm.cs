using HostFile.Libs.Contracts.Interfaces;
using HostFile.UI.Updater.Handlers;
using HostFile.UI.Updater.Interfaces.Factory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Updater
{
    public partial class MainForm : Form
    {
        private readonly IAppFactory _appFactory;
        private readonly ILogger _logger;
        private readonly IAppSetting _appSetting;

        private bool _isUpdating = false;

        public MainForm(IAppFactory appFactory)
        {
            InitializeComponent();

            _appFactory = appFactory;
            _appSetting = appFactory.GetAppSetting();
            _logger = appFactory.GetLogger();
        }

        private async void OnButtonUpdateClicked(object sender, EventArgs e)
        {
            txtOutput.Clear();
            btnUpdate.Enabled = false;
            pgrsBar.Style = ProgressBarStyle.Marquee;
            _isUpdating = true;

            IProgress<string> updateProgress = new Progress<string>(value =>
            {
                txtOutput.AppendText(value + Environment.NewLine);
            });
            
            var updateHandler = new UpdateHandler(_appSetting, _logger);
            bool isSuccess = false;

            try
            {
                // Update data in server.
                isSuccess = await updateHandler.UpdateDataAsync(_appSetting.Sources, updateProgress);

                string status = isSuccess ? "Yes" : "No";
                MessageBox.Show($"Update operation is successful? {status}", "Update Finished", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                updateProgress.Report($"{ex.Message}.");
                
                MessageBox.Show($"Encountered error. Operation stopped immediately.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            btnUpdate.Enabled = true;
            pgrsBar.Style = ProgressBarStyle.Blocks;
            _isUpdating = false;
        }

        private void OnMainFormClosing(object sender, FormClosingEventArgs e)
        {
            // Cancel closing operation if the updating process is still running.
            if (_isUpdating)
            {
                MessageBox.Show("Update is in progress. Please wait before closing.", "Exit Canceled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }
    }
}
