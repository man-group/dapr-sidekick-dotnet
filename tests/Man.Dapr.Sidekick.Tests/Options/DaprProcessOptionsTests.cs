using System.Collections.Generic;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Options
{
    public class DaprProcessOptionsTests
    {
        public class Clone
        {
            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new MockDaprProcessOptions().Clone());
            }

            [Test]
            public void Should_clone_all_members()
            {
                var source = InitValue();
                var target = source.Clone();
                Compare(source, target);
            }
        }

        public class EnrichFrom
        {
            [Test]
            public void Should_not_throw_exception_when_null_source()
            {
                Assert.DoesNotThrow(() => new MockDaprProcessOptions().EnrichFrom(null));
            }

            [Test]
            public void Should_not_throw_exception_when_uninitialized_properties()
            {
                Assert.DoesNotThrow(() => new MockDaprProcessOptions().EnrichFrom(new MockDaprProcessOptions()));
            }

            [Test]
            public void Should_not_overwrite_specified_properties()
            {
                var source = InitValue();
                var target = InitValue();
                target.EnrichFrom(new MockDaprProcessOptions());
                Compare(source, target, true); // Target should still entirely match original values
            }

            [Test]
            public void Should_enrich_all_unspecified_properties()
            {
                var source = InitValue();
                var target = new MockDaprProcessOptions();
                target.EnrichFrom(source);
                Compare(source, target); // Target should entirely match new values in source
            }

            [Test]
            public void Should_enrich_environmentvariables()
            {
                var source = new MockDaprProcessOptions();
                var target = new MockDaprProcessOptions();

                // Both null dictionaries - ensure no exception
                Assert.DoesNotThrow(() => target.EnrichFrom(source));
                Assert.That(target.EnvironmentVariables, Is.Null);

                // Target null, source valid - ensure enrichment
                source.EnvironmentVariables = new Dictionary<string, string>
                {
                    { "1", "VALUE_1" },
                    { "2", "VALUE_2" }
                };
                target.EnrichFrom(source);
                Assert.That(target.EnvironmentVariables, Is.Not.Null);
                Assert.That(target.EnvironmentVariables, Is.Not.SameAs(source.EnvironmentVariables));
                Assert.That(target.EnvironmentVariables.Count, Is.EqualTo(2));
                Assert.That(target.EnvironmentVariables["1"], Is.EqualTo("VALUE_1"));
                Assert.That(target.EnvironmentVariables["2"], Is.EqualTo("VALUE_2"));

                // Make sure duplicate value doesn't throw error and overrides
                source.EnvironmentVariables["2"] = "VALUE_2_CHANGED";
                source.EnvironmentVariables["3"] = "VALUE_3";
                target.EnrichFrom(source);
                Assert.That(target.EnvironmentVariables.Count, Is.EqualTo(3));
                Assert.That(target.EnvironmentVariables["2"], Is.EqualTo("VALUE_2_CHANGED"));
                Assert.That(target.EnvironmentVariables["3"], Is.EqualTo("VALUE_3"));
            }
        }

        public class GetHealthUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new MockDaprProcessOptions();
                Assert.That(source.GetHealthUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new MockDaprProcessOptions
                {
                    HealthPort = 12345
                };

                Assert.That(source.GetHealthUri().ToString(), Is.EqualTo("http://127.0.0.1:12345/"));
            }

            [Test]
            public void Should_return_null_when_not_overridden()
            {
                Assert.That(new MockDaprProcessOptionsNoOverrides().GetHealthUri(), Is.Null);
            }
        }

        public class GetMetricsUri
        {
            [Test]
            public void Should_return_null_when_invalid_port()
            {
                var source = new MockDaprProcessOptions();
                Assert.That(source.GetMetricsUri(), Is.Null);
            }

            [Test]
            public void Should_return_uri_when_valid_port()
            {
                var source = new MockDaprProcessOptions
                {
                    MetricsPort = 23456
                };

                Assert.That(source.GetMetricsUri().ToString(), Is.EqualTo("http://127.0.0.1:23456/"));
            }

            [Test]
            public void Should_return_null_when_not_overridden()
            {
                Assert.That(new MockDaprProcessOptionsNoOverrides().GetMetricsUri(), Is.Null);
            }
        }

        public class IsRuntimeVersionEarlierThan
        {
            [TestCase(null, null, false)]
            [TestCase(null, "0.0.0", false)]
            [TestCase("0.0.0", "0.0.0", false)]
            [TestCase("1.0.0", "0.1.1", false)]
            [TestCase("1.2.3", "1.2.3", false)]
            [TestCase("1.0.0", "1.0.1", true)]
            public void Should_compare_versions(string runtimeVersion, string inputVersion, bool expected)
            {
                var source = new MockDaprProcessOptions
                {
                    RuntimeVersion = runtimeVersion != null ? new System.Version(runtimeVersion) : null
                };

                Assert.That(source.IsRuntimeVersionEarlierThan(inputVersion), Is.EqualTo(expected));
            }
        }

        private static void Compare(DaprProcessOptions source, DaprProcessOptions target, bool sameInstances = false)
        {
            if (!sameInstances)
            {
                Assert.That(source, Is.Not.SameAs(target));
                Assert.That(source.Metrics, Is.Not.SameAs(target.Metrics));
            }

            Assert.That(target.BinDirectory, Is.EqualTo(source.BinDirectory));
            Assert.That(target.CopyProcessFile, Is.EqualTo(source.CopyProcessFile));
            Assert.That(target.Enabled, Is.EqualTo(source.Enabled));
            Assert.That(target.InitialDirectory, Is.EqualTo(source.InitialDirectory));
            Assert.That(target.IssuerCertificate, Is.EqualTo(source.IssuerCertificate));
            Assert.That(target.IssuerKey, Is.EqualTo(source.IssuerKey));
            Assert.That(target.LogLevel, Is.EqualTo(source.LogLevel));
            Assert.That(target.Metrics.EnableCollector, Is.EqualTo(source.Metrics.EnableCollector));
            Assert.That(target.ProcessFile, Is.EqualTo(source.ProcessFile));
            Assert.That(target.ProcessName, Is.EqualTo(source.ProcessName));
            Assert.That(target.RestartAfterMillseconds, Is.EqualTo(source.RestartAfterMillseconds));
            Assert.That(target.RetainPortsOnRestart, Is.EqualTo(source.RetainPortsOnRestart));
            Assert.That(target.RuntimeDirectory, Is.EqualTo(source.RuntimeDirectory));
            Assert.That(target.RuntimeVersion, Is.EqualTo(source.RuntimeVersion));
            Assert.That(target.WaitForShutdownSeconds, Is.EqualTo(source.WaitForShutdownSeconds));
            Assert.That(target.TrustAnchorsCertificate, Is.EqualTo(source.TrustAnchorsCertificate));
        }

        private static MockDaprProcessOptions InitValue() => new MockDaprProcessOptions
        {
            BinDirectory = "BinDirectory",
            CopyProcessFile = true,
            Enabled = false,
            InitialDirectory = "InitialDirectory",
            IssuerCertificate = "IssuerCertificate",
            IssuerKey = "IssuerKey",
            LogLevel = "LogLevel",
            Metrics = new DaprMetricsOptions
            {
                EnableCollector = true
            },
            ProcessFile = "ProcessFile",
            ProcessName = "ProcessName",
            RestartAfterMillseconds = 100,
            RetainPortsOnRestart = true,
            RuntimeDirectory = "RuntimeDirectory",
            RuntimeVersion = new System.Version("1.2.3.4"),
            WaitForShutdownSeconds = 200,
            TrustAnchorsCertificate = "TrustAnchorsCertificate"
        };
    }
}
