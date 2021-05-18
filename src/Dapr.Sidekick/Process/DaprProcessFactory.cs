namespace Dapr.Sidekick.Process
{
    public class DaprProcessFactory : IDaprProcessFactory
    {
        public IDaprSidecarProcess CreateDaprSidecarProcess() => new DaprSidecarProcess();

        public IDaprPlacementProcess CreateDaprPlacementProcess() => new DaprPlacementProcess();

        public IDaprSentryProcess CreateDaprSentryProcess() => new DaprSentryProcess();
    }
}
