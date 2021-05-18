#if NET35
using System.IO;

namespace Dapr.Sidekick
{
    public partial interface IDaprSidecarHost
    {
        /// <summary>
        /// Retrieves metadata from the Dapr Sidecar and writes to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">A writeable stream to which the metadata will be written.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        int WriteMetadata(Stream stream);
    }
}
#endif
