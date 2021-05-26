using System;
using System.Linq;
using System.Threading;
using Man.Dapr.Sidekick.AspNetCore.Metrics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarHostedService : DaprHostedService<IDaprSidecarHost, DaprSidecarOptions>
    {
        private readonly IServiceProvider _serviceProvider;

        public DaprSidecarHostedService(
            IDaprSidecarHost daprSidecarHost,
            IOptionsMonitor<DaprOptions> optionsAccessor,
            IServiceProvider serviceProvider = null)
            : base(daprSidecarHost, optionsAccessor)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnStarting(DaprOptions options, CancellationToken cancellationToken)
        {
            // Assign metrics
            options.Sidecar ??= new DaprSidecarOptions();
            options.Sidecar.Metrics ??= new DaprMetricsOptions();
            options.Sidecar.Metrics.SetLabel(DaprMetricsConstants.ServiceLabelName, options.Sidecar.AppId);
            options.Sidecar.Metrics.SetLabel(DaprMetricsConstants.AppLabelName, DaprMetricsConstants.DaprSidecarLabel);

            // Assign the HTTP App Port from the ASP.NET Core server if not already defined.
            // This will block waiting for the hosting to start if necessary.
            if (options.Sidecar.AppPort == null)
            {
                // Attempt to get the Hosting server to extract the port
                var server = _serviceProvider?.GetService<IServer>();
                if (server != null)
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Get all server addresses as parsed Uris
                        var serverAddresses = server.Features?
                            .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>()?
                            .Addresses?
                            .Select(x => Http.UriHelper.Parse(x))
                            .Where(x => x != null)
                            .ToArray()
                            ?? Array.Empty<Uri>();

                        if (serverAddresses.Length > 0)
                        {
                            // Find the best match based on the scheme and if AppSsl is specified.
                            // Default to the first entry
                            var selectedAddress = serverAddresses[0];
                            foreach (var address in serverAddresses)
                            {
                                if ((options.Sidecar.AppSsl == true && address.Scheme == Uri.UriSchemeHttps) ||
                                    (options.Sidecar.AppSsl == false && address.Scheme == Uri.UriSchemeHttp))
                                {
                                    // Found an explicit match based on the AppSsl flag. Use it
                                    selectedAddress = address;
                                    break;
                                }
                                else if (address.Scheme == Uri.UriSchemeHttp)
                                {
                                    // First non-SSL address. Set it as preferred match but keep looping to find a better one.
                                    selectedAddress = address;
                                }
                            }

                            // Address found, set the port
                            options.Sidecar.AppPort = selectedAddress.Port;
                            break;
                        }

                        // Sleep - not pretty, but need to wait for hosting
                        Thread.Sleep(10);
                    }
                }
            }
        }
    }
}
