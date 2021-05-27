using System.Threading;
using System.Threading.Tasks;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public interface IDaprMetricsCollectorRegistry
    {
        /// <summary>
        /// Collects all metrics and exports them in text document format to the provided serializer.
        /// </summary>
        /// <param name="serializer">The serializer to which the metrics will be written.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CollectAndExportAsTextAsync(IDaprMetricsSerializer serializer, CancellationToken cancellationToken = default);
    }
}
