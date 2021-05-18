using System;

namespace Dapr.Sidekick.Security
{
    public class RandomStringApiTokenProvider : IDaprApiTokenProvider
    {
        public string GetAppApiToken() => Guid.NewGuid().ToString();

        public string GetDaprApiToken() => Guid.NewGuid().ToString();
    }
}
