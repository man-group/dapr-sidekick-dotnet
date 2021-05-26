using System;

namespace Man.Dapr.Sidekick.Security
{
    public class RandomStringApiTokenProvider : IDaprApiTokenProvider
    {
        public string GetAppApiToken() => Guid.NewGuid().ToString();

        public string GetDaprApiToken() => Guid.NewGuid().ToString();
    }
}
