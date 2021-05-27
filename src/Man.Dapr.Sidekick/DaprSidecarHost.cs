using System;
using System.Collections.Generic;
using System.Linq;
using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Process;
using Man.Dapr.Sidekick.Security;

namespace Man.Dapr.Sidekick
{
    public partial class DaprSidecarHost : DaprProcessHost<DaprSidecarOptions>, IDaprSidecarHost
    {
        private readonly IDaprApiTokenManager _daprApiTokenManager;
        private readonly IEnumerable<IDaprSidecarProcessInterceptor> _daprSidecarInterceptors;

        public DaprSidecarHost(
            IDaprProcessFactory daprProcessFactory,
            IDaprProcessHttpClientFactory daprHttpClientFactory,
            IDaprApiTokenManager daprApiTokenManager,
            IDaprLoggerFactory loggerFactory,
            IEnumerable<IDaprSidecarProcessInterceptor> daprSidecarInterceptors = null)
            : base(() => daprProcessFactory.CreateDaprSidecarProcess(), daprHttpClientFactory, new DaprLogger<DaprSidecarHost>(loggerFactory))
        {
            _daprApiTokenManager = daprApiTokenManager;
            _daprSidecarInterceptors = daprSidecarInterceptors;
        }

        protected override void OnProcessStarting(DaprProcessStartingEventArgs<DaprSidecarOptions> args)
        {
            var options = args.Options;

            // Invoke any interceptors
            InvokeInterceptors(interceptor => interceptor.OnStarting(options));

            // Update the API tokens - this can happen here because Environment Variables are set after this method returns
            // If the API token is not defined in configuration then replace it with the default API token if required.
            if (string.IsNullOrEmpty(options.DaprApiToken) && options.UseDefaultDaprApiToken == true)
            {
                // Use the default Dapr API token
                options.DaprApiToken = _daprApiTokenManager.DaprApiToken;
            }
            else
            {
                // Use the specified Dapr API token
                _daprApiTokenManager.SetDaprApiToken(options.DaprApiToken);
            }

            if (string.IsNullOrEmpty(options.AppApiToken) && options.UseDefaultAppApiToken == true)
            {
                // Use the default App API token
                options.AppApiToken = _daprApiTokenManager.AppApiToken;
            }
            else
            {
                // Use the specified App API token
                _daprApiTokenManager.SetAppApiToken(options.AppApiToken);
            }
        }

        private void InvokeInterceptors(Action<IDaprSidecarProcessInterceptor> invoke)
        {
            if (_daprSidecarInterceptors?.Any() != true)
            {
                return;
            }

            // Invoke each interceptor
            foreach (var interceptor in _daprSidecarInterceptors)
            {
                invoke(interceptor);
            }
        }
    }
}
