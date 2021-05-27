namespace Man.Dapr.Sidekick.Process
{
    public interface IDaprProcessUpdater
    {
        void UpdateStatus(DaprProcessStatus status);

        void UpdateVersion(string version);
    }
}
