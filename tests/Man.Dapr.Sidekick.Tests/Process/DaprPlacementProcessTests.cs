using System;
using System.IO;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprPlacementProcessTests
    {
        public class GetProcessOptions
        {
            [Test]
            public void Should_create_placement_section_if_null()
            {
                var p = new MockDaprPlacementProcess();
                var options = new DaprOptions();

                Assert.That(options.Placement, Is.Null);

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
            }

            [Test]
            public void Should_use_existing_placement_section()
            {
                var p = new MockDaprPlacementProcess();
                var options = new DaprOptions
                {
                    Placement = new DaprPlacementOptions
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
                var p = new MockDaprPlacementProcess();
                var builder = new PortAssignmentBuilder<DaprPlacementOptions>(new MockPortAvailabilityChecker());
                var options = new DaprPlacementOptions();
                var logger = Substitute.For<IDaprLogger>();

                p.AssignPorts(builder);
                builder.Build(options, new DaprPlacementOptions(), logger);
                Assert.That(options.HealthPort, Is.EqualTo(8081));
                Assert.That(options.MetricsPort, Is.EqualTo(9091));
                Assert.That(options.Port, Is.EqualTo(DaprConstants.IsWindows ? 6050 : 50005));
            }
        }

        public class AssignLocations
        {
            [Test]
            public void Should_assign_default_paths()
            {
                var p = new MockDaprPlacementProcess();
                var folder = Path.GetTempPath();
                var options_1_11_2 = new DaprPlacementOptions
                {
                    RuntimeVersion = new Version("1.11.2")
                };
                var options_1_12_0 = new DaprPlacementOptions
                {
                    RuntimeVersion = new Version("1.12.0")
                };

                p.AssignLocations(options_1_11_2, folder);
                Assert.That(options_1_11_2.CertsDirectory, Is.EqualTo(Path.Combine(folder, DaprConstants.DaprCertsDirectory)));
                Assert.That(options_1_11_2.TrustAnchorsFile, Is.Null);

                p.AssignLocations(options_1_12_0, folder);
                Assert.That(options_1_12_0.CertsDirectory, Is.Null);
                Assert.That(options_1_12_0.TrustAnchorsFile, Is.EqualTo(Path.Combine(folder, Path.Combine(DaprConstants.DaprCertsDirectory, DaprConstants.TrustAnchorsCertificateFilename))));
            }

            [Test]
            public void Should_assign_specified_paths()
            {
                var p = new MockDaprPlacementProcess();
                var folder = Path.GetTempPath();
                var options = new DaprPlacementOptions
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
                var p = new MockDaprPlacementProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprPlacementOptions();

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo("--log-as-json"));
            }

            [Test]
            public void Should_add_all_arguments()
            {
                var p = new MockDaprPlacementProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprPlacementOptions
                {
                    CertsDirectory = "CertsDirectory",
                    EnableMetrics = true,
                    HealthPort = 1234,
                    Id = "Id",
                    InitialCluster = "InitialCluster",
                    InmemStoreEnabled = true,
                    LogLevel = "LogLevel",
                    MetricsPort = 2345,
                    Port = 3456,
                    RaftLogstorePath = "RaftLogstorePath",
                    ReplicationFactor = 100,
                    Mtls = true,
                    CustomArguments = "arg1 val1"
                };

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo(
                    "--certchain CertsDirectory " +
                    "--enable-metrics " +
                    "--healthz-port 1234 " +
                    "--id Id " +
                    "--initial-cluster InitialCluster " +
                    "--inmem-store-enabled " +
                    "--log-as-json " +
                    "--log-level LogLevel " +
                    "--metrics-port 2345 " +
                    "--port 3456 " +
                    "--raft-logstore-path RaftLogstorePath " +
                    "--replicationfactor 100 " +
                    "--tls-enabled " +
                    "--arg1 val1"));
            }
        }

        public class AddEnvironmentVariables
        {
            [Test]
            public void Should_add_expected_values()
            {
                var p = new MockDaprPlacementProcess();
                var builder = new EnvironmentVariableBuilder();
                var options = new DaprPlacementOptions
                {
                    HealthPort = 1234,
                    MetricsPort = 2345,
                    Port = 3456
                };

                p.AddEnvironmentVariables(options, builder);

                var values = builder.ToDictionary();
                Assert.That(values["DAPR_PLACEMENT_HEALTH_PORT"], Is.EqualTo(1234));
                Assert.That(values["DAPR_PLACEMENT_METRICS_PORT"], Is.EqualTo(2345));
                Assert.That(values["DAPR_PLACEMENT_PORT"], Is.EqualTo(3456));
            }
        }

        public class ParseCommandLineArgument
        {
            [Test]
            public void Should_parse_all_arguments()
            {
                var p = new MockDaprPlacementProcess();
                var options = new DaprPlacementOptions();

                p.ParseCommandLineArgument(options, "certchain", "CertsDirectory");
                p.ParseCommandLineArgument(options, "enable-metrics", null);
                p.ParseCommandLineArgument(options, "healthz-port", "1234");
                p.ParseCommandLineArgument(options, "id", "Id");
                p.ParseCommandLineArgument(options, "initial-cluster", "InitialCluster");
                p.ParseCommandLineArgument(options, "inmem-store-enabled", null);
                p.ParseCommandLineArgument(options, "log-level", "LogLevel");
                p.ParseCommandLineArgument(options, "metrics-port", "2345");
                p.ParseCommandLineArgument(options, "port", "3456");
                p.ParseCommandLineArgument(options, "raft-logstore-path", "RaftLogstorePath");
                p.ParseCommandLineArgument(options, "replicationfactor", "100");
                p.ParseCommandLineArgument(options, "tls-enabled", null);

                Assert.That(options.CertsDirectory, Is.EqualTo("CertsDirectory"));
                Assert.That(options.EnableMetrics, Is.True);
                Assert.That(options.HealthPort, Is.EqualTo(1234));
                Assert.That(options.Id, Is.EqualTo("Id"));
                Assert.That(options.InitialCluster, Is.EqualTo("InitialCluster"));
                Assert.That(options.InmemStoreEnabled, Is.True);
                Assert.That(options.LogLevel, Is.EqualTo("LogLevel"));
                Assert.That(options.MetricsPort, Is.EqualTo(2345));
                Assert.That(options.Port, Is.EqualTo(3456));
                Assert.That(options.RaftLogstorePath, Is.EqualTo("RaftLogstorePath"));
                Assert.That(options.ReplicationFactor, Is.EqualTo(100));
                Assert.That(options.Mtls, Is.True);
            }
        }

        public class CompareProcessOptions
        {
            [Test]
            public void Should_return_none()
            {
                var p = new MockDaprPlacementProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprPlacementOptions
                {
                    Id = "P1"
                };

                var o2 = new DaprPlacementOptions
                {
                    Id = "P2"
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.None));
            }

            [Test]
            public void Should_return_duplicate()
            {
                var p = new MockDaprPlacementProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprPlacementOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                var o2 = new DaprPlacementOptions
                {
                    Id = "P1",
                    Port = 2345
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Duplicate));
            }

            [Test]
            public void Should_return_attachable()
            {
                var p = new MockDaprPlacementProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprPlacementOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                var o2 = new DaprPlacementOptions
                {
                    Id = "P1",
                    Port = 1234
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Attachable));
            }
        }

        private class MockDaprPlacementProcess : DaprPlacementProcess
        {
            public new DaprPlacementOptions GetProcessOptions(DaprOptions daprOptions) => base.GetProcessOptions(daprOptions);

            public new void AssignPorts(PortAssignmentBuilder<DaprPlacementOptions> builder) => base.AssignPorts(builder);

            public new void AssignLocations(DaprPlacementOptions options, string daprFolder) => base.AssignLocations(options, daprFolder);

            public new void AddCommandLineArguments(DaprPlacementOptions source, CommandLineArgumentBuilder builder) => base.AddCommandLineArguments(source, builder);

            public new void AddEnvironmentVariables(DaprPlacementOptions source, EnvironmentVariableBuilder builder) => base.AddEnvironmentVariables(source, builder);

            public new void ParseCommandLineArgument(DaprPlacementOptions target, string name, string value) => base.ParseCommandLineArgument(target, name, value);

            public new ProcessComparison CompareProcessOptions(DaprPlacementOptions proposedProcessOptions, DaprPlacementOptions existingProcessOptions, IProcess existingProcess) => base.CompareProcessOptions(proposedProcessOptions, existingProcessOptions, existingProcess);
        }
    }
}
