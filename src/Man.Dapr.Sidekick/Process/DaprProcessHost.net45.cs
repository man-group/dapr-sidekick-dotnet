#if !NET35
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Man.Dapr.Sidekick.Process
{
    public partial class DaprProcessHost<TOptions>
    {
        public async Task<bool> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var result = await GetHealthAsync(cancellationToken);
            return result.IsHealthy;
        }

        public async Task<DaprHealthResult> GetHealthAsync(CancellationToken cancellationToken)
        {
            var client = DaprHttpClientFactory.CreateDaprHttpClient();
            var uri = Process?.LastSuccessfulOptions?.GetHealthUri();
            if (uri == null || client == null)
            {
                return DaprHealthResult.Unknown;
            }

            // Check the endpoint - this will throw an exception if an error occurs.
            var result = await client.GetAsync(uri, cancellationToken);
            return new DaprHealthResult(result.StatusCode, result.ReasonPhrase);
        }

        public async Task<int> WriteMetricsAsync(Stream stream, CancellationToken cancellationToken)
        {
            var client = DaprHttpClientFactory.CreateDaprHttpClient();
            var uri = Process?.LastSuccessfulOptions?.GetMetricsUri();
            if (uri == null || client == null)
            {
                return 0;
            }

            try
            {
                var response = await client.GetAsync(uri, cancellationToken);
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
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
