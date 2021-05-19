// This implementation is based on the approach used by prometheus-net.
// See https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.AspNetCore/MetricServerMiddleware.cs
// See PROMETHEUS_LICENSE in this directory for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public class DaprMetricsServerMiddleware
    {
        private readonly IDaprMetricsCollectorRegistry _collectorRegistry;

#pragma warning disable RCS1163 // Unused parameter.
        public DaprMetricsServerMiddleware(RequestDelegate next, IDaprMetricsCollectorRegistry collectorRegistry)
#pragma warning restore RCS1163 // Unused parameter.
        {
            _collectorRegistry = collectorRegistry;
        }

        public async Task Invoke(HttpContext context)
        {
            var response = context.Response;

            try
            {
                // We first touch the response.Body only in the callback because touching
                // it means we can no longer send headers (the status code).
                var serializer = new DaprMetricsTextSerializer(() =>
                {
                    response.ContentType = DaprMetricsConstants.ExporterContentType;
                    response.StatusCode = StatusCodes.Status200OK;
                    return response.Body;
                });

                // Write all collectors
                await _collectorRegistry.CollectAndExportAsTextAsync(serializer, context.RequestAborted);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                // The scrape was cancalled by the client. This is fine. Just swallow the exception to not generate pointless spam.
            }
            catch (Exception ex)
            {
                // Update the status code and write an error message.
                response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    using var writer = new StreamWriter(response.Body, DaprMetricsConstants.ExportEncoding, bufferSize: -1, leaveOpen: true);
                    await writer.WriteAsync(ex.Message);
                }
            }
        }
    }
}
