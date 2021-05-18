#if NET35
using System;
using System.IO;
using System.Net;

namespace Dapr.Sidekick.Process
{
    public partial class DaprProcessHost<TOptions>
    {
        public DaprHealthResult GetHealth()
        {
            var client = DaprHttpClientFactory.CreateDaprWebClient();
            var uri = Process?.LastSuccessfulOptions?.GetHealthUri();
            if (uri == null || client == null)
            {
                return DaprHealthResult.Unknown;
            }

            try
            {
                // If this succeeds result is healthy. If a WebException occurs extract status code and return response.
                // All other exceptions are propagated.
                client.DownloadData(uri);
                return new DaprHealthResult(HttpStatusCode.NoContent, "No Content");
            }
            catch (WebException ex)
            {
                // Non-200 result
                var response = (HttpWebResponse)ex.Response;
                return new DaprHealthResult(response.StatusCode, response.StatusDescription);
            }
        }

        public int WriteMetrics(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("Stream provided for Dapr Process Metrics is not writeable");
            }

            var client = DaprHttpClientFactory.CreateDaprWebClient();
            var uri = Process?.LastSuccessfulOptions?.GetMetricsUri();
            if (uri == null || client == null)
            {
                // Nothing to do
                return 0;
            }

            try
            {
                var bytes = client.DownloadData(uri);
                stream.Write(bytes, 0, bytes.Length);
                return bytes.Length;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while obtaining the Dapr process metrics from {DaprProcessMetricsUri}", uri);
                return 0;
            }
        }
    }
}
#endif
