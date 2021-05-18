using NUnit.Framework;

namespace Dapr.Sidekick.Security
{
    public class RandomStringApiTokenProviderTests
    {
        public class GetAppApiToken
        {
            [Test]
            public void Should_return_different_tokens_each_call()
            {
                var provider = new RandomStringApiTokenProvider();
                var token1 = provider.GetAppApiToken();
                var token2 = provider.GetAppApiToken();

                Assert.That(token1, Is.Not.Null);
                Assert.That(token2, Is.Not.Null);
                Assert.That(token1, Is.Not.EqualTo(token2));
            }
        }

        public class GetDaprApiToken
        {
            [Test]
            public void Should_return_different_tokens_each_call()
            {
                var provider = new RandomStringApiTokenProvider();
                var token1 = provider.GetDaprApiToken();
                var token2 = provider.GetDaprApiToken();

                Assert.That(token1, Is.Not.Null);
                Assert.That(token2, Is.Not.Null);
                Assert.That(token1, Is.Not.EqualTo(token2));
            }
        }
    }
}
