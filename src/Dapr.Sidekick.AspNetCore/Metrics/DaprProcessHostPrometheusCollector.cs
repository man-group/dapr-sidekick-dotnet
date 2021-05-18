using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Sidekick.Process;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public abstract class DaprProcessHostPrometheusCollector : IPrometheusCollector
    {
        private readonly IDaprProcessHost _daprProcessHost;

        protected DaprProcessHostPrometheusCollector(IDaprProcessHost daprProcessHost)
        {
            _daprProcessHost = daprProcessHost;
        }

        public async Task CollectTextAsync(PrometheusModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // Don't do anything if the process host isn't running
            if (_daprProcessHost.GetProcessInfo()?.IsRunning != true)
            {
                return;
            }

            var options = _daprProcessHost.GetProcessOptions()?.Metrics;
            if (options?.EnableCollector == false)
            {
                // Collector disabled
                return;
            }

            using var ms = new MemoryStream();
            var bytesWritten = await _daprProcessHost.WriteMetricsAsync(ms, cancellationToken);
            if (bytesWritten > 0)
            {
                // Read the source stream into the model, enriching with the options labels.
                ms.Position = 0;
                var enricher = new PrometheusLabelEnricher(options?.Labels);
                var reader = new PrometheusTextReader(model, enricher);
                await reader.ReadAsync(ms, cancellationToken);
            }
        }
    }
}
