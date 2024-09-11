namespace Man.Dapr.Sidekick.AspNetCore.Scheduler
{
    public class DaprSchedulerHealthCheck : DaprProcessHealthCheck
    {
        public DaprSchedulerHealthCheck(
            IDaprSchedulerHost daprSchedulerHost)
            : base(daprSchedulerHost)
        {
        }
    }
}
