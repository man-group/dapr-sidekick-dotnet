using System.Net;

namespace Man.Dapr.Sidekick.Process
{
    /// <summary>
    /// The result of a Health Check executed against a Dapr process.
    /// </summary>
    public class DaprHealthResult
    {
        /// <summary>
        /// Gets a health result with an unknown status.
        /// </summary>
        public static DaprHealthResult Unknown => new DaprHealthResult(HttpStatusCode.ServiceUnavailable, "Unknown or unreachable");

        public DaprHealthResult(HttpStatusCode statusCode, string statusDescription = null)
        {
            StatusCode = statusCode;
            StatusDescription = statusDescription;
        }

        /// <summary>
        /// Gets a value indicating whether the Dapr process is reachable and healthy.
        /// </summary>
        public bool IsHealthy => (int)StatusCode >= 200 && (int)StatusCode < 400;

        public HttpStatusCode StatusCode { get; }

        public string StatusDescription { get; }

        public override string ToString() =>
            IsHealthy ? "Healthy" :
            string.IsNullOrEmpty(StatusDescription) ? string.Concat("Unhealthy (", StatusCode, ")") :
            string.Concat("Unhealthy (", StatusCode, ", ", StatusDescription, ")");
    }
}
