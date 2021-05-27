#if NET35

using System;
using System.IO;
using System.Net;
using Man.Dapr.Sidekick.Process;

namespace Man.Dapr.Sidekick
{
    public partial class DaprSidecarHost
    {
        public int WriteMetadata(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("Stream provided for Dapr Sidecar Metadata is not writeable");
            }

            var client = DaprHttpClientFactory.CreateDaprWebClient();
            var uri = Process?.LastSuccessfulOptions?.GetMetadataUri();
            if (uri == null || client == null)
            {
                // Nothing to do
                return 0;
            }

            try
            {
                var bytes = DownloadData(client, uri);
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
        internal Func<WebClient, Uri, byte[]> DownloadData { get; set; } = (client, uri) => client.DownloadData(uri);

        internal Func<WebClient, Uri, byte[], byte[]> UploadData { get; set; } = (client, uri, data) => client.UploadData(uri, data);

        protected override void OnProcessStopping(DaprProcessStoppingEventArgs args)
        {
            // Send the shutdown command to the sidecar
            var client = DaprHttpClientFactory.CreateDaprWebClient();
            var uri = Process?.LastSuccessfulOptions?.GetShutdownUri();
            if (uri != null && client != null)
            {
                // Execute synchronously
                Logger.LogInformation("Sending Shutdown command to Sidecar");

                try
                {
                    UploadData(client, uri, new byte[0]);
                }
                catch (Exception ex)
                {
                    // Shutdown command not received - log an error and don't wait for shutdown.
                    Logger.LogError(ex, "Shutdown command was not processed successfully by Sidecar. Reason: {DaprSidecarShutdownError}", ex.Message);
                    Process.LastSuccessfulOptions.WaitForShutdownSeconds = null;
                }
            }
        }
    }
}
#endif
