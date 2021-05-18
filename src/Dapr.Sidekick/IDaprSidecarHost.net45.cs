#if !NET35
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick
{
    public partial interface IDaprSidecarHost
    {
        /// <summary>
        /// Retrieves metadata from the Dapr Sidecar and writes to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">A writeable stream to which the metadata will be written.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        Task<int> WriteMetadataAsync(Stream stream, CancellationToken cancellationToken);
    }
}
#endif
