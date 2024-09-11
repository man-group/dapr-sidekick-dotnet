using System.IO;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprSchedulerProcessTests
    {
        public class GetProcessOptions
        {
            [Test]
            public void Should_create_scheduler_section_if_null()
            {
                var p = new MockDaprSchedulerProcess();
                var options = new DaprOptions();

                Assert.That(options.Scheduler, Is.Null);

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
            }

            [Test]
            public void Should_use_existing_scheduler_section()
            {
                var p = new MockDaprSchedulerProcess();
                var options = new DaprOptions
                {
                    Scheduler = new DaprSchedulerOptions
                    {
                        Id = "TEST",
                        RetainPortsOnRestart = false
                    }
                };

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
                Assert.That(newOptions.Id, Is.EqualTo("TEST"));
                Assert.That(newOptions.RetainPortsOnRestart, Is.True);
            }
        }

        public class AssignPorts
        {
            [Test]
            public void Should_assign_expected_values()
            {
                var p = new MockDaprSchedulerProcess();
                var builder = new PortAssignmentBuilder<DaprSchedulerOptions>(new MockPortAvailabilityChecker());
                var options = new DaprSchedulerOptions();
                var logger = Substitute.For<IDaprLogger>();

                p.AssignPorts(builder);
                builder.Build(options, new DaprSchedulerOptions(), logger);
                Assert.That(options.HealthPort, Is.EqualTo(8082));
                Assert.That(options.MetricsPort, Is.EqualTo(9093));
                Assert.That(options.Port, Is.EqualTo(DaprConstants.IsWindows ? 6060 : 50006));
            }
        }

        public class AssignLocations
        {
            [Test]
            public void Should_assign_default_paths()
            {
                var p = new MockDaprSchedulerProcess();
                var folder = Path.GetTempPath();
                var options = new DaprSchedulerOptions();

                p.AssignLocations(options, folder);
                Assert.That(options.CertsDirectory, Is.Null);
                Assert.That(options.TrustAnchorsFile, Is.EqualTo(Path.Combine(folder, Path.Combine(DaprConstants.DaprCertsDirectory, DaprConstants.TrustAnchorsCertificateFilename))));
            }

            [Test]
            public void Should_assign_specified_paths()
            {
                var p = new MockDaprSchedulerProcess();
                var folder = Path.GetTempPath();
                var options = new DaprSchedulerOptions
                {
                    CertsDirectory = folder + @"relative\path\cert.txt"
                };

                p.AssignLocations(options, folder);

                Assert.That(options.CertsDirectory, Is.EqualTo(Path.Combine(folder, @"relative\path\cert.txt")));
            }
        }

        public class AddCommandLineArguments
        {
            [Test]
            public void Should_add_default_arguments()
            {
                var p = new MockDaprSchedulerProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprSchedulerOptions();

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo("--log-as-json"));
            }

            [Test]
            public void Should_add_all_arguments()
            {
                var p = new MockDaprSchedulerProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprSchedulerOptions
                {
                    CertsDirectory = "CertsDirectory",
                    EnableMetrics = true,
                    EtcdClientHttpPorts = "EtcdClientHttpPorts",
                    EtcdClientPorts = "EtcdClientPorts",
                    EtcdCompactionMode = "EtcdCompactionMode",
                    EtcdCompactionRetention = "EtcdCompactionRetention",
                    EtcdDataDir = "EtcdDataDir",
                    EtcdSpaceQuota = 9876,
                    HealthListenAddress = "HealthzListenAddress",
                    HealthPort = 1234,
                    Id = "Id",
                    InitialCluster = "InitialCluster",
                    ListenAddress = "ListenAddress",
                    LogLevel = "LogLevel",
                    MetricsListenAddress = "MetricsListenAddress",
                    MetricsPort = 2345,
                    Mode = "Mode",
                    Port = 3456,
                    ReplicaCount = 5,
                    TlsEnabled = true,
                    TrustAnchorsFile = "TrustAnchorsFile",
                    TrustDomain = "TrustDomain",
                    CustomArguments = "arg1 val1"
                };

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo(
                    "--enable-metrics " +
                    "--etcd-client-http-ports EtcdClientHttpPorts " +
                    "--etcd-client-ports EtcdClientPorts " +
                    "--etcd-compaction-mode EtcdCompactionMode " +
                    "--etcd-compaction-retention EtcdCompactionRetention " +
                    "--etcd-data-dir EtcdDataDir " +
                    "--etcd-space-quota 9876 " +
                    "--healthz-listen-address HealthzListenAddress " +
                    "--healthz-port 1234 " +
                    "--id Id " +
                    "--initial-cluster InitialCluster " +
                    "--listen-address ListenAddress " +
                    "--log-as-json " +
                    "--log-level LogLevel " +
                    "--metrics-listen-address MetricsListenAddress " +
                    "--metrics-port 2345 " +
                    "--mode Mode " +
                    "--port 3456 " +
                    "--replica-count 5 " +
                    "--tls-enabled " +
                    "--trust-anchors-file TrustAnchorsFile " +
                    "--trust-domain TrustDomain " +
                    "--arg1 val1"));
            }
        }

        public class AddEnvironmentVariables
        {
            [Test]
            public void Should_add_expected_values()
            {
                var p = new MockDaprSchedulerProcess();
                var builder = new EnvironmentVariableBuilder();
                var options = new DaprSchedulerOptions
                {
                    HealthPort = 1234,
                    MetricsPort = 2345,
                    Port = 3456
                };

                p.AddEnvironmentVariables(options, builder);

                var values = builder.ToDictionary();
                Assert.That(values["DAPR_SCHEDULER_HEALTH_PORT"], Is.EqualTo(1234));
                Assert.That(values["DAPR_SCHEDULER_METRICS_PORT"], Is.EqualTo(2345));
                Assert.That(values["DAPR_SCHEDULER_PORT"], Is.EqualTo(3456));
            }
        }

        public class ParseCommandLineArgument
        {
            [Test]
            public void Should_parse_all_arguments()
            {
                var p = new MockDaprSchedulerProcess();
                var options = new DaprSchedulerOptions();

                p.ParseCommandLineArgument(options, "enable-metrics", null);
                p.ParseCommandLineArgument(options, "etcd-client-http-ports", "EtcdClientHttpPorts");
                p.ParseCommandLineArgument(options, "etcd-client-ports", "EtcdClientPorts");
                p.ParseCommandLineArgument(options, "etcd-compaction-mode", "EtcdCompactionMode");
                p.ParseCommandLineArgument(options, "etcd-compaction-retention", "EtcdCompactionRetention");
                p.ParseCommandLineArgument(options, "etcd-data-dir", "EtcdDataDir");
                p.ParseCommandLineArgument(options, "etcd-space-quota", "9876");
                p.ParseCommandLineArgument(options, "healthz-listen-address", "HealthzListenAddress");
                p.ParseCommandLineArgument(options, "healthz-port", "1234");
                p.ParseCommandLineArgument(options, "id", "Id");
                p.ParseCommandLineArgument(options, "initial-cluster", "InitialCluster");
                p.ParseCommandLineArgument(options, "listen-address", "ListenAddress");
                p.ParseCommandLineArgument(options, "log-level", "LogLevel");
                p.ParseCommandLineArgument(options, "metrics-listen-address", "MetricsListenAddress");
                p.ParseCommandLineArgument(options, "metrics-port", "2345");
                p.ParseCommandLineArgument(options, "mode", "Mode");
                p.ParseCommandLineArgument(options, "port", "3456");
                p.ParseCommandLineArgument(options, "port", "3456");
                p.ParseCommandLineArgument(options, "replica-count", "5");
                p.ParseCommandLineArgument(options, "tls-enabled", null);
                p.ParseCommandLineArgument(options, "trust-anchors-file", "TrustAnchorsFile");
                p.ParseCommandLineArgument(options, "trust-domain", "TrustDomain");

                Assert.That(options.EnableMetrics, Is.True);
                Assert.That(options.EtcdClientHttpPorts, Is.EqualTo("EtcdClientHttpPorts"));
                Assert.That(options.EtcdClientPorts, Is.EqualTo("EtcdClientPorts"));
                Assert.That(options.EtcdCompactionMode, Is.EqualTo("EtcdCompactionMode"));
                Assert.That(options.EtcdCompactionRetention, Is.EqualTo("EtcdCompactionRetention"));
                Assert.That(options.EtcdDataDir, Is.EqualTo("EtcdDataDir"));
                Assert.That(options.EtcdSpaceQuota, Is.EqualTo(9876));
                Assert.That(options.HealthListenAddress, Is.EqualTo("HealthzListenAddress"));
                Assert.That(options.HealthPort, Is.EqualTo(1234));
                Assert.That(options.Id, Is.EqualTo("Id"));
                Assert.That(options.InitialCluster, Is.EqualTo("InitialCluster"));
                Assert.That(options.ListenAddress, Is.EqualTo("ListenAddress"));
                Assert.That(options.LogLevel, Is.EqualTo("LogLevel"));
                Assert.That(options.MetricsListenAddress, Is.EqualTo("MetricsListenAddress"));
                Assert.That(options.MetricsPort, Is.EqualTo(2345));
                Assert.That(options.Mode, Is.EqualTo("Mode"));
                Assert.That(options.Port, Is.EqualTo(3456));
                Assert.That(options.ReplicaCount, Is.EqualTo(5));
                Assert.That(options.TlsEnabled, Is.True);
                Assert.That(options.TrustAnchorsFile, Is.EqualTo("TrustAnchorsFile"));
                Assert.That(options.TrustDomain, Is.EqualTo("TrustDomain"));
            }
        }

        public class CompareProcessOptions
        {
            [Test]
            public void Should_return_none()
            {
                var p = new MockDaprSchedulerProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSchedulerOptions
                {
                    Id = "P1"
                };

                var o2 = new DaprSchedulerOptions
                {
                    Id = "P2"
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.None));
            }

            [Test]
            public void Should_return_duplicate()
            {
                var p = new MockDaprSchedulerProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSchedulerOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                var o2 = new DaprSchedulerOptions
                {
                    Id = "P1",
                    Port = 2345
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Duplicate));
            }

            [Test]
            public void Should_return_attachable()
            {
                var p = new MockDaprSchedulerProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSchedulerOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                var o2 = new DaprSchedulerOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Attachable));
            }
        }

        private class MockDaprSchedulerProcess : DaprSchedulerProcess
        {
            public new DaprSchedulerOptions GetProcessOptions(DaprOptions daprOptions) => base.GetProcessOptions(daprOptions);

            public new void AssignPorts(PortAssignmentBuilder<DaprSchedulerOptions> builder) => base.AssignPorts(builder);

            public new void AssignLocations(DaprSchedulerOptions options, string daprFolder) => base.AssignLocations(options, daprFolder);

            public new void AddCommandLineArguments(DaprSchedulerOptions source, CommandLineArgumentBuilder builder) => base.AddCommandLineArguments(source, builder);

            public new void AddEnvironmentVariables(DaprSchedulerOptions source, EnvironmentVariableBuilder builder) => base.AddEnvironmentVariables(source, builder);

            public new void ParseCommandLineArgument(DaprSchedulerOptions target, string name, string value) => base.ParseCommandLineArgument(target, name, value);

            public new ProcessComparison CompareProcessOptions(DaprSchedulerOptions proposedProcessOptions, DaprSchedulerOptions existingProcessOptions, IProcess existingProcess) => base.CompareProcessOptions(proposedProcessOptions, existingProcessOptions, existingProcess);
        }
    }
}
