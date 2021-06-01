#if !NET35
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.Process;

namespace Man.Dapr.Sidekick
{
    public partial class DaprSidecarHost
    {
        public async Task<int> WriteMetadataAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("Stream provided for Dapr Sidecar Metadata is not writeable");
            }

            var client = DaprHttpClientFactory.CreateDaprHttpClient();
            var uri = Process?.LastSuccessfulOptions?.GetMetadataUri();
            if (uri == null || client == null)
            {
                return 0;
            }

            try
            {
                var response = await GetAsync(client, uri, cancellationToken);
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();
                stream.Write(bytes, 0, bytes.Length);
                return bytes.Length;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred while obtaining the Dapr sidecar metadata from {DaprSidecarMetadataUri}", uri);
                return 0;
            }
        }

        // For testing
        internal Func<HttpClient, Uri, CancellationToken, Task<HttpResponseMessage>> GetAsync { get; set; } = (client, uri, cancellationToken) => client.GetAsync(uri, cancellationToken);

        // For testing
        internal Func<HttpClient, Uri, HttpContent, CancellationToken, Task<HttpResponseMessage>> PostAsync { get; set; } = (client, uri, content, cancellationToken) => client.PostAsync(uri, content, cancellationToken);

        protected override void OnProcessStopping(DaprProcessStoppingEventArgs args)
        {
            // If running in attached mode, do not send the shutdown command
            var processInfo = Process?.GetProcessInfo();
            if (processInfo?.IsAttached == true)
            {
                return;
            }

            // Send the shutdown command to the sidecar
            var httpClient = DaprHttpClientFactory.CreateDaprHttpClient();
            var uri = Process?.LastSuccessfulOptions?.GetShutdownUri();
            if (uri != null && httpClient != null)
            {
                // Execute synchronously
                Logger.LogInformation("Sending Shutdown command to Sidecar");
                var content = new StringContent(string.Empty);
                var result = PostAsync(httpClient, uri, content, args.CancellationToken.CancellationToken).Result; // Requires Dapr 1.1.1
                if (!result.IsSuccessStatusCode)
                {
                    // Shutdown command not received - log an error and don't wait for shutdown.
                    Logger.LogError("Shutdown command was not processed successfully by Sidecar. Reason: {DaprSidecarShutdownError}", result.ReasonPhrase);
                    Process.LastSuccessfulOptions.WaitForShutdownSeconds = null;
                }
            }
        }
    }
}
#endif
