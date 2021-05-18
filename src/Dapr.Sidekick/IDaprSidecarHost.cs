using Dapr.Sidekick.Process;

namespace Dapr.Sidekick
{
    public partial interface IDaprSidecarHost : IDaprProcessHost<DaprSidecarOptions>
    {
    }
}
