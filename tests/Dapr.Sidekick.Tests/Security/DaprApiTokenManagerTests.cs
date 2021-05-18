using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Security
{
    public class DaprApiTokenManagerTests
    {
        [Test]
        public void Should_initialize_tokens()
        {
            var provider = Substitute.For<IDaprApiTokenProvider>();
            provider.GetAppApiToken().Returns("APP_TOKEN");
            provider.GetDaprApiToken().Returns("DAPR_TOKEN");
            var manager = new DaprApiTokenManager(provider);

            Assert.That(manager.AppApiToken.Value, Is.EqualTo("APP_TOKEN"));
            Assert.That(manager.DaprApiToken.Value, Is.EqualTo("DAPR_TOKEN"));
        }

        [Test]
        public void Should_set_app_api_token()
        {
            var provider = Substitute.For<IDaprApiTokenProvider>();
            var manager = new DaprApiTokenManager(provider);

            manager.SetAppApiToken("SET_APP_TOKEN");
            Assert.That(manager.AppApiToken.Value, Is.EqualTo("SET_APP_TOKEN"));
        }

        [Test]
        public void Should_set_dapr_api_token()
        {
            var provider = Substitute.For<IDaprApiTokenProvider>();
            var manager = new DaprApiTokenManager(provider);

            manager.SetDaprApiToken("SET_DAPR_TOKEN");
            Assert.That(manager.DaprApiToken.Value, Is.EqualTo("SET_DAPR_TOKEN"));
        }
    }
}
