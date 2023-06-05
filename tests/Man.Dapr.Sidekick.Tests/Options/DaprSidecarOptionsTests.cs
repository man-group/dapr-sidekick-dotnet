using NUnit.Framework;

namespace Man.Dapr.Sidekick.Options
{
    public class DaprSidecarOptionsTests
    {
        public class Constructor
        {
            [Test]
            public void Should_set_defaults()
            {
                var options = new DaprSidecarOptions();
                Assert.That(options.WaitForShutdownSeconds, Is.EqualTo(10));
            }
        }

        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new DaprSidecarOptions().Clone());
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
                var source = new DaprSidecarOptions();
                Assert.That(source.GetHealthUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSidecarOptions
                {
                    DaprHttpPort = 12345
                };

                Assert.That(source.GetHealthUri().ToString(), Is.EqualTo("http://127.0.0.1:12345/v1.0/healthz"));
            }
        }

        public class GetMetricsUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new DaprSidecarOptions();
                Assert.That(source.GetMetricsUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSidecarOptions
                {
                    MetricsPort = 23456
                };

                Assert.That(source.GetMetricsUri().ToString(), Is.EqualTo("http://127.0.0.1:23456/"));
            }
        }

        public class GetMetadataUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new DaprSidecarOptions();
                Assert.That(source.GetMetadataUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSidecarOptions
                {
                    DaprHttpPort = 34567
                };

                Assert.That(source.GetMetadataUri().ToString(), Is.EqualTo("http://127.0.0.1:34567/v1.0/metadata"));
            }
        }

        public class GetShutdownUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new DaprSidecarOptions();
                Assert.That(source.GetShutdownUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new DaprSidecarOptions
                {
                    DaprHttpPort = 45678
                };

                Assert.That(source.GetShutdownUri().ToString(), Is.EqualTo("http://127.0.0.1:45678/v1.0/shutdown"));
            }
        }

        private static void Compare(DaprSidecarOptions source, DaprSidecarOptions target, bool sameInstances = false)
        {
            if (!sameInstances)
            {
                Assert.That(source, Is.Not.SameAs(target));
            }

            Assert.That(target.AllowedOrigins, Is.EqualTo(source.AllowedOrigins));
            Assert.That(target.AppApiToken, Is.EqualTo(source.AppApiToken));
            Assert.That(target.AppId, Is.EqualTo(source.AppId));
            Assert.That(target.AppMaxConcurrency, Is.EqualTo(source.AppMaxConcurrency));
            Assert.That(target.AppPort, Is.EqualTo(source.AppPort));
            Assert.That(target.AppProtocol, Is.EqualTo(source.AppProtocol));
            Assert.That(target.AppSsl, Is.EqualTo(source.AppSsl));
            Assert.That(target.ResourcesDirectory, Is.EqualTo(source.ResourcesDirectory));
            Assert.That(target.ComponentsDirectory, Is.EqualTo(source.ComponentsDirectory));
            Assert.That(target.ConfigFile, Is.EqualTo(source.ConfigFile));
            Assert.That(target.ControlPlaneAddress, Is.EqualTo(source.ControlPlaneAddress));
            Assert.That(target.CustomArguments, Is.EqualTo(source.CustomArguments));
            Assert.That(target.DaprApiToken, Is.EqualTo(source.DaprApiToken));
            Assert.That(target.DaprGrpcPort, Is.EqualTo(source.DaprGrpcPort));
            Assert.That(target.DaprHttpMaxRequestSize, Is.EqualTo(source.DaprHttpMaxRequestSize));
            Assert.That(target.DaprHttpPort, Is.EqualTo(source.DaprHttpPort));
            Assert.That(target.DaprInternalGrpcPort, Is.EqualTo(source.DaprInternalGrpcPort));
            Assert.That(target.EnableMetrics, Is.EqualTo(source.EnableMetrics));
            Assert.That(target.KubeConfig, Is.EqualTo(source.KubeConfig));
            Assert.That(target.MetricsPort, Is.EqualTo(source.MetricsPort));
            Assert.That(target.Mode, Is.EqualTo(source.Mode));
            Assert.That(target.Mtls, Is.EqualTo(source.Mtls));
            Assert.That(target.Namespace, Is.EqualTo(source.Namespace));
            Assert.That(target.PlacementHostAddress, Is.EqualTo(source.PlacementHostAddress));
            Assert.That(target.ProfilePort, Is.EqualTo(source.ProfilePort));
            Assert.That(target.Profiling, Is.EqualTo(source.Profiling));
            Assert.That(target.SentryAddress, Is.EqualTo(source.SentryAddress));
            Assert.That(target.UseDefaultAppApiToken, Is.EqualTo(source.UseDefaultAppApiToken));
            Assert.That(target.UseDefaultDaprApiToken, Is.EqualTo(source.UseDefaultDaprApiToken));
        }

        private static DaprSidecarOptions InitValue() => new DaprSidecarOptions
        {
            AllowedOrigins = "AllowedOrigins",
            AppApiToken = "AppApiToken",
            AppId = "AppId",
            AppMaxConcurrency = 100,
            AppPort = 200,
            AppProtocol = "AppProtocol",
            AppSsl = true,
            ResourcesDirectory = "ResourcesDirectory",
            ComponentsDirectory = "ComponentsDirectory",
            ConfigFile = "ConfigFile",
            ControlPlaneAddress = "ControlPlaneAddress",
            CustomArguments = "CustomArguments",
            DaprApiToken = "DaprApiToken",
            DaprGrpcPort = 300,
            DaprHttpMaxRequestSize = 400,
            DaprHttpPort = 500,
            DaprInternalGrpcPort = 600,
            EnableMetrics = true,
            KubeConfig = "KubeConfig",
            MetricsPort = 700,
            Mode = "Mode",
            Mtls = true,
            Namespace = "Namespace",
            PlacementHostAddress = "PlacementHostAddress",
            ProfilePort = 800,
            Profiling = true,
            SentryAddress = "SentryAddress",
            UseDefaultAppApiToken = true,
            UseDefaultDaprApiToken = true
        };
    }
}
