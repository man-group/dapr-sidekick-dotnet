using NUnit.Framework;

namespace Dapr.Sidekick.Options
{
    public class DaprPlacementOptionsTests
    {
        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprPlacementOptions().Clone());
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
                var source = new DaprPlacementOptions();
                Assert.That(source.GetHealthUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprPlacementOptions
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
                var source = new DaprPlacementOptions();
                Assert.That(source.GetMetricsUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprPlacementOptions
                {
                    MetricsPort = 23456
                };

                Assert.That(source.GetMetricsUri().ToString(), Is.EqualTo("http://127.0.0.1:23456/"));
            }
        }

        private static void Compare(DaprPlacementOptions source, DaprPlacementOptions target, bool sameInstances = false)
        {
            if (!sameInstances)
            {
                Assert.That(source, Is.Not.SameAs(target));
            }

            Assert.That(target.CertsDirectory, Is.EqualTo(source.CertsDirectory));
            Assert.That(target.CustomArguments, Is.EqualTo(source.CustomArguments));
            Assert.That(target.Id, Is.EqualTo(source.Id));
            Assert.That(target.InitialCluster, Is.EqualTo(source.InitialCluster));
            Assert.That(target.InmemStoreEnabled, Is.EqualTo(source.InmemStoreEnabled));
            Assert.That(target.EnableMetrics, Is.EqualTo(source.EnableMetrics));
            Assert.That(target.HealthPort, Is.EqualTo(source.HealthPort));
            Assert.That(target.MetricsPort, Is.EqualTo(source.MetricsPort));
            Assert.That(target.Mtls, Is.EqualTo(source.Mtls));
            Assert.That(target.Port, Is.EqualTo(source.Port));
            Assert.That(target.RaftLogstorePath, Is.EqualTo(source.RaftLogstorePath));
            Assert.That(target.ReplicationFactor, Is.EqualTo(source.ReplicationFactor));
        }

        private static DaprPlacementOptions InitValue() => new DaprPlacementOptions
        {
            CertsDirectory = "CertChain",
            CustomArguments = "CustomArguments",
            EnableMetrics = true,
            HealthPort = 100,
            Id = "Id",
            InitialCluster = "InitialCluster",
            InmemStoreEnabled = true,
            MetricsPort = 200,
            Mtls = true,
            Port = 300,
            RaftLogstorePath = "RaftLogstorePath",
            ReplicationFactor = 400
        };
    }
}
