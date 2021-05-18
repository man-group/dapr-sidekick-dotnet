// This implementation is based on the approach used by prometheus-net.
// See https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.AspNetCore/MetricServerMiddlewareExtensions.cs
// and PROMETHEUS_LICENSE in this folder.

using System;
using Dapr.Sidekick.AspNetCore.Metrics;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public static class DaprMetricsMiddlewareExtensions
    {
#if NETCOREAPP
        private const string DefaultDisplayName = "Dapr Prometheus metrics";

        public static IEndpointConventionBuilder MapDaprMetrics(
            this Routing.IEndpointRouteBuilder endpoints,
            string pattern = "/metrics")
        {
            var pipeline = endpoints
                .CreateApplicationBuilder()
                .UseMiddleware<DaprMetricsServerMiddleware>()
                .Build();

            return endpoints
                .Map(pattern, pipeline)
                .WithDisplayName(DefaultDisplayName);
        }

#endif
        public static IApplicationBuilder UseDaprMetricsServer(this IApplicationBuilder builder, int port, string url = "/metrics")
        {
            return builder
                .Map(url, b => b.MapWhen(PortMatches(), b1 => b1.InternalUseMiddleware()));

            Func<HttpContext, bool> PortMatches()
            {
                return c => c.Connection.LocalPort == port;
            }
        }

        public static IApplicationBuilder UseDaprMetricsServer(this IApplicationBuilder builder, string url = "/metrics")
        {
            // If there is a URL to map, map it and re-enter without the URL.
            if (url != null)
            {
                return builder.Map(url, b => b.InternalUseMiddleware());
            }
            else
            {
                return builder.InternalUseMiddleware();
            }
        }

        private static IApplicationBuilder InternalUseMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DaprMetricsServerMiddleware>();
        }
    }
}
