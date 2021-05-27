// This implementation is based on the approach used by prometheus-net.
// See https://github.com/prometheus-net/prometheus-net/blob/master/Prometheus.NetStandard/PrometheusConstants.cs
// See PROMETHEUS_LICENSE in this directory for license information.

using System.Text;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public static class DaprMetricsConstants
    {
        public static readonly string AppLabelName = "app";
        public static readonly string ServiceLabelName = "service";

        public static readonly string DaprSidecarLabel = "dapr-sidecar";
        public static readonly string DaprPlacementLabel = "dapr-placement";
        public static readonly string DaprSentryLabel = "dapr-sentry";

        public static readonly string ExporterContentType = "text/plain; charset=utf-8";

        // ASP.NET does not want to accept the parameters in PushStreamContent for whatever reason...
        public static readonly string ExporterContentTypeMinimal = "text/plain";

        // Use UTF-8 encoding, but provide the flag to ensure the Unicode Byte Order Mark is never
        // pre-pended to the output stream.
        public static readonly Encoding ExportEncoding = new UTF8Encoding(false);
    }
}
