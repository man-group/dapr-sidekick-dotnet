using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick.AspNetCore.Metrics
{
    public interface IDaprMetricsSerializer
    {
        Task WriteLineAsync(string value, CancellationToken cancellationToken);

        Task FlushAsync(CancellationToken cancellationToken);
    }
}
