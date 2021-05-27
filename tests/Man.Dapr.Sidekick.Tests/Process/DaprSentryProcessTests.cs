using System.IO;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprSentryProcessTests
    {
        public class GetProcessOptions
        {
            [Test]
            public void Should_create_sentry_section_if_null()
            {
                var p = new MockDaprSentryProcess();
                var options = new DaprOptions();

                Assert.That(options.Sentry, Is.Null);

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
            }

            [Test]
            public void Should_use_existing_sentry_section()
            {
                var p = new MockDaprSentryProcess();
                var options = new DaprOptions
                {
                    Sentry = new DaprSentryOptions
                    {
                        TrustDomain = "TEST",
                        RetainPortsOnRestart = false
                    }
                };

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
                Assert.That(newOptions.TrustDomain, Is.EqualTo("TEST"));
                Assert.That(newOptions.RetainPortsOnRestart, Is.True);
            }
        }

        public class AssignPorts
        {
            [Test]
            public void Should_assign_expected_values()
            {
                var p = new MockDaprSentryProcess();
                var builder = new PortAssignmentBuilder<DaprSentryOptions>(new MockPortAvailabilityChecker());
                var options = new DaprSentryOptions();
                var logger = Substitute.For<IDaprLogger>();

                p.AssignPorts(builder);
                builder.Build(options, new DaprSentryOptions(), logger);
                Assert.That(options.MetricsPort, Is.EqualTo(9092));
            }
        }

        public class AssignLocations
        {
            [Test]
            public void Should_assign_default_paths()
            {
                var p = new MockDaprSentryProcess();
                var options = new DaprSentryOptions();
                var folder = Path.GetTempPath();

                p.AssignLocations(options, folder);

                Assert.That(options.CertsDirectory, Is.EqualTo(Path.Combine(folder, "certs")));
                Assert.That(options.ConfigFile, Is.EqualTo(Path.Combine(folder, "config.yaml")));
            }

            [Test]
            public void Should_assign_specified_paths()
            {
                var p = new MockDaprSentryProcess();
                var folder = Path.GetTempPath();
                var options = new DaprSentryOptions
                {
                    CertsDirectory = folder + "creds.txt",
                    ConfigFile = folder + "config.txt"
                };

                p.AssignLocations(options, folder);

                Assert.That(options.CertsDirectory, Is.EqualTo(Path.Combine(folder, "creds.txt")));
                Assert.That(options.ConfigFile, Is.EqualTo(Path.Combine(folder, "config.txt")));
            }
        }

        public class AddCommandLineArguments
        {
            [Test]
            public void Should_add_default_arguments()
            {
                var p = new MockDaprSentryProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprSentryOptions();

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo("--log-as-json"));
            }

            [Test]
            public void Should_add_all_arguments()
            {
                var p = new MockDaprSentryProcess();
                var builder = new CommandLineArgumentBuilder();
                var configFile = Path.GetTempFileName();
                var options = new DaprSentryOptions
                {
                    EnableMetrics = true,
                    CertsDirectory = "CertsDirectory",
                    LogLevel = "LogLevel",
                    MetricsPort = 2345,
                    TrustDomain = "TrustDomain",
                    ConfigFile = configFile, // Must exist
                    CustomArguments = "arg1 val1"
                };

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo(
                    "--enable-metrics " +
                    "--issuer-credentials CertsDirectory " +
                    "--log-as-json " +
                    "--log-level LogLevel " +
                    "--metrics-port 2345 " +
                    "--trust-domain TrustDomain " +
                    "--config " + configFile + " " +
                    "--arg1 val1"));

                File.Delete(configFile);
            }
        }

        public class AddEnvironmentVariables
        {
            [Test]
            public void Should_add_expected_values()
            {
                var p = new MockDaprSentryProcess();
                var builder = new EnvironmentVariableBuilder();
                var options = new DaprSentryOptions
                {
                    MetricsPort = 2345
                };

                p.AddEnvironmentVariables(options, builder);

                var values = builder.ToDictionary();
                Assert.That(values["DAPR_SENTRY_METRICS_PORT"], Is.EqualTo(2345));
            }
        }

        public class ParseCommandLineArgument
        {
            [Test]
            public void Should_parse_all_arguments()
            {
                var p = new MockDaprSentryProcess();
                var options = new DaprSentryOptions();

                p.ParseCommandLineArgument(options, "config", "ConfigFile");
                p.ParseCommandLineArgument(options, "enable-metrics", null);
                p.ParseCommandLineArgument(options, "issuer-credentials", "CertsDirectory");
                p.ParseCommandLineArgument(options, "log-level", "LogLevel");
                p.ParseCommandLineArgument(options, "metrics-port", "2345");
                p.ParseCommandLineArgument(options, "trust-domain", "TrustDomain");

                Assert.That(options.ConfigFile, Is.EqualTo("ConfigFile"));
                Assert.That(options.EnableMetrics, Is.True);
                Assert.That(options.CertsDirectory, Is.EqualTo("CertsDirectory"));
                Assert.That(options.LogLevel, Is.EqualTo("LogLevel"));
                Assert.That(options.MetricsPort, Is.EqualTo(2345));
                Assert.That(options.TrustDomain, Is.EqualTo("TrustDomain"));
            }
        }

        public class CompareProcessOptions
        {
            [Test]
            public void Should_return_duplicate()
            {
                var p = new MockDaprSentryProcess();
                Assert.That(p.CompareProcessOptions(new DaprSentryOptions(), new DaprSentryOptions(), null), Is.EqualTo(ProcessComparison.Duplicate));
            }
        }

        private class MockDaprSentryProcess : DaprSentryProcess
        {
            public new DaprSentryOptions GetProcessOptions(DaprOptions daprOptions) => base.GetProcessOptions(daprOptions);

            public new void AssignPorts(PortAssignmentBuilder<DaprSentryOptions> builder) => base.AssignPorts(builder);

            public new void AssignLocations(DaprSentryOptions options, string daprFolder) => base.AssignLocations(options, daprFolder);

            public new void AddCommandLineArguments(DaprSentryOptions source, CommandLineArgumentBuilder builder) => base.AddCommandLineArguments(source, builder);

            public new void AddEnvironmentVariables(DaprSentryOptions source, EnvironmentVariableBuilder builder) => base.AddEnvironmentVariables(source, builder);

            public new void ParseCommandLineArgument(DaprSentryOptions target, string name, string value) => base.ParseCommandLineArgument(target, name, value);

            public new ProcessComparison CompareProcessOptions(DaprSentryOptions proposedProcessOptions, DaprSentryOptions existingProcessOptions, IProcess existingProcess) => base.CompareProcessOptions(proposedProcessOptions, existingProcessOptions, existingProcess);
        }
    }
}
