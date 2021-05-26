// This implementation is based on the approach used by prometheus-net.
// See https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/CollectorRegistry.cs
// See PROMETHEUS_LICENSE in this directory for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class PrometheusCollectorRegistry : IDaprMetricsCollectorRegistry
    {
        private readonly IEnumerable<IPrometheusCollector> _collectors;
        private readonly IEnumerable<IPrometheusMetricFilter> _filters;

        public PrometheusCollectorRegistry(
            IEnumerable<IPrometheusCollector> collectors,
            IEnumerable<IPrometheusMetricFilter> filters)
        {
            _collectors = collectors;
            _filters = filters;
        }

        public async Task CollectAndExportAsTextAsync(IDaprMetricsSerializer serializer, CancellationToken cancellationToken = default)
        {
            if (_collectors == null)
            {
                return;
            }

            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }

            // Create a model to store the intermediate data
            var model = new PrometheusModel();

            // Write each collector to the model sequentially
            foreach (var collector in _collectors)
            {
                await collector.CollectTextAsync(model, cancellationToken);
            }

            // Now serialize the model to the serializer
            if (model.Metrics?.Any() == true)
            {
                foreach (var metric in model.Metrics.Values)
                {
                    await serializer.WriteLineAsync(metric.HelpLine, cancellationToken);
                    await serializer.WriteLineAsync(metric.TypeLine, cancellationToken);
                    foreach (var metricLine in metric.MetricLines)
                    {
                        if (_filters?.Any(x => x.ExcludeMetricLine(metric.Name, metricLine)) == true)
                        {
                            // Filter applied, exclude metric from output
                            continue;
                        }

                        await serializer.WriteLineAsync(metricLine, cancellationToken);
                    }
                }
            }

            await serializer.FlushAsync(cancellationToken);
        }
    }
}
