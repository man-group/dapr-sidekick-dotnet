namespace Dapr.Sidekick.Process
{
    public enum DaprProcessStatus
    {
        Stopped = 0,
        Initializing = 1,
        Starting = 2,
        Started = 3,
        Stopping = 4
    }

    internal enum ProcessComparison
    {
        None = 0,
        Duplicate = 1,
        Attachable = 2
    }
}
