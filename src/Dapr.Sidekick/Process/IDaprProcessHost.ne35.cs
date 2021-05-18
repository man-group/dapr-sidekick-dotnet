#if NET35
using System.IO;

namespace Dapr.Sidekick.Process
{
    public partial interface IDaprProcessHost
    {
        /// <summary>
        /// Gets the current health of the Dapr Process.
        /// </summary>
        /// <returns>A <see cref="DaprHealthResult"/>.</returns>
        DaprHealthResult GetHealth();

        /// <summary>
        /// Retrieves metrics from the Dapr Process and writes to the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">A writeable stream to which the metrics will be written.</param>
        /// <returns>The number of bytes written to the stream.</returns>
        int WriteMetrics(Stream stream);
    }
}
#endif
