namespace Dapr.Sidekick.Security
{
    public class DaprApiTokenManager : IDaprApiTokenManager
    {
        private readonly IDaprApiTokenProvider _tokenProvider;
        private SensitiveString _daprApiToken;
        private SensitiveString _appApiToken;

        public DaprApiTokenManager(IDaprApiTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;

            // Set the default values
            SetDaprApiToken(tokenProvider.GetDaprApiToken());
            SetAppApiToken(tokenProvider.GetAppApiToken());
        }

        public SensitiveString DaprApiToken => _daprApiToken;

        public SensitiveString AppApiToken => _appApiToken;

        public void SetDaprApiToken(SensitiveString apiToken) => _daprApiToken = apiToken;

        public void SetAppApiToken(SensitiveString apiToken) => _appApiToken = apiToken;
    }
}
