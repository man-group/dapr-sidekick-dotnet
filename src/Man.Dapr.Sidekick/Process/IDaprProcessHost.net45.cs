#if !NET35
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Man.Dapr.Sidekick.Process
{
    public partial interface IDaprProcessHost
    {
        /// <summary>
        /// Gets the current health of the Dapr Process.
        /// </summary>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A <see cref="DaprHealthResult"/>.</returns>
        Task<DaprHealthResult> GetHealthAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves metrics from the Dapr Process and writes to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">A writeable stream to which the metrics will be written.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        Task<int> WriteMetricsAsync(Stream stream, CancellationToken cancellationToken);
    }
}
#endif
