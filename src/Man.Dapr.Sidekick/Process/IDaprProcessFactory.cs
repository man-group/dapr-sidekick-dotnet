namespace Man.Dapr.Sidekick.Process
{
    public interface IDaprProcessFactory
    {
        IDaprSidecarProcess CreateDaprSidecarProcess();

        IDaprPlacementProcess CreateDaprPlacementProcess();

        IDaprSchedulerProcess CreateDaprSchedulerProcess();

        IDaprSentryProcess CreateDaprSentryProcess();
    }
}
