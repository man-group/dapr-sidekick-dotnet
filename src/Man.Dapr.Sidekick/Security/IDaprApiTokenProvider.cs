namespace Man.Dapr.Sidekick.Security
{
    public interface IDaprApiTokenProvider
    {
        string GetDaprApiToken();

        string GetAppApiToken();
    }
}
