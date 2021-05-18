using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public interface IPrometheusCollector
    {
        /// <summary>
        /// Populates the provided model with metrics text lines collected from the underlying source.
        /// </summary>
        /// <param name="model">The model into which the metrics will be stored.</param>
        /// <param name="cancellationToken">A token for cancelling the operation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task CollectTextAsync(PrometheusModel model, CancellationToken cancellationToken = default);
    }
}
