namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarHealthCheck : DaprProcessHealthCheck
    {
        public DaprSidecarHealthCheck(
            IDaprSidecarHost daprSidecarHost)
            : base(daprSidecarHost)
        {
        }
    }
}
