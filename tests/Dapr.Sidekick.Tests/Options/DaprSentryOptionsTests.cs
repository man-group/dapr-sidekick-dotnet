using NUnit.Framework;

namespace Dapr.Sidekick.Options
{
    public class DaprSentryOptionsTests
    {
        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprSentryOptions().Clone());
            }

            [Test]
            public void Should_clone_all_members()
            {
                var source = InitValue();
                var target = source.Clone();
                Compare(source, target);
            }
        }

        public class GetHealthUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new DaprSentryOptions()
                {
                    HealthPort = null
                };

                Assert.That(source.GetHealthUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSentryOptions
                {
                    HealthPort = 12345
                };

                Assert.That(source.GetHealthUri().ToString(), Is.EqualTo("http://127.0.0.1:12345/healthz"));
            }
        }

        public class GetMetricsUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new DaprSentryOptions();
                Assert.That(source.GetMetricsUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSentryOptions
                {
                    MetricsPort = 23456
                };

                Assert.That(source.GetMetricsUri().ToString(), Is.EqualTo("http://127.0.0.1:23456/"));
            }
        }

        private static void Compare(DaprSentryOptions source, DaprSentryOptions target, bool sameInstances = false)
        {
            if (!sameInstances)
            {
                Assert.That(source, Is.Not.SameAs(target));
            }

            Assert.That(target.ConfigFile, Is.EqualTo("ConfigFile"));
            Assert.That(target.CustomArguments, Is.EqualTo("CustomArguments"));
            Assert.That(target.EnableMetrics, Is.True);
            Assert.That(target.HealthPort, Is.EqualTo(100));
            Assert.That(target.MetricsPort, Is.EqualTo(200));
            Assert.That(target.CertsDirectory, Is.EqualTo("CertsDirectory"));
            Assert.That(target.TrustDomain, Is.EqualTo("TrustDomain"));
        }

        private static DaprSentryOptions InitValue() => new DaprSentryOptions
        {
            ConfigFile = "ConfigFile",
            CustomArguments = "CustomArguments",
            EnableMetrics = true,
            HealthPort = 100,
            CertsDirectory = "CertsDirectory",
            MetricsPort = 200,
            TrustDomain = "TrustDomain"
        };
    }
}
