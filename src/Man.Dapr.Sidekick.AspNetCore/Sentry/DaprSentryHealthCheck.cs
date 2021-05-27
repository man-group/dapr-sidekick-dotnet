namespace Man.Dapr.Sidekick.AspNetCore.Sentry
{
    public class DaprSentryHealthCheck : DaprProcessHealthCheck
    {
        public DaprSentryHealthCheck(
            IDaprSentryHost daprSentryHost)
            : base(daprSentryHost)
        {
        }
    }
}
