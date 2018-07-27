using HostFile.Libs.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HostFile.UI.Updater.Handlers
{
    class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(ILogger logger)
        {
            InnerHandler = new HttpClientHandler();

            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInfo($"Request: {request.Method} {request.RequestUri.AbsolutePath}");
            var response = await base.SendAsync(request, cancellationToken);
            _logger.LogInfo($"Response: {response.StatusCode}");
            return response;
        }
    }
}
