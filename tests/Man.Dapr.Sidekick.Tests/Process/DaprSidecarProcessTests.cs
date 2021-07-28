using System.IO;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Security;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprSidecarProcessTests
    {
        public class GetProcessOptions
        {
            [Test]
            public void Should_assign_defaults()
            {
                var p = new MockDaprSidecarProcess();
                var options = new DaprOptions();

                Assert.That(options.Sidecar, Is.Null);

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
                Assert.That(newOptions.AppId, Is.Not.Null);
                Assert.That(newOptions.Namespace, Is.EqualTo("default"));
            }

            [Test]
            public void Should_use_existing_sidecar_section()
            {
                var p = new MockDaprSidecarProcess();
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST",
                        Namespace = "TESTNS"
                    }
                };

                var newOptions = p.GetProcessOptions(options);
                Assert.That(newOptions, Is.Not.Null);
                Assert.That(newOptions.AppId, Is.EqualTo("TEST"));
                Assert.That(newOptions.Namespace, Is.EqualTo("TESTNS"));
            }
        }

        public class AssignPorts
        {
            [Test]
            public void Should_assign_expected_values()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new PortAssignmentBuilder<DaprSidecarOptions>(new MockPortAvailabilityChecker());
                var options = new DaprSidecarOptions();
                var logger = Substitute.For<IDaprLogger>();

                p.AssignPorts(builder);
                builder.Build(options, new DaprSidecarOptions(), logger);
                Assert.That(options.AppPort, Is.EqualTo(8500));
                Assert.That(options.DaprGrpcPort, Is.EqualTo(50001));
                Assert.That(options.DaprHttpPort, Is.EqualTo(3500));
                Assert.That(options.MetricsPort, Is.EqualTo(9090));
                Assert.That(options.ProfilePort, Is.EqualTo(7777));
            }
        }

        public class AssignLocations
        {
            [Test]
            public void Should_assign_default_paths()
            {
                var p = new MockDaprSidecarProcess();
                var options = new DaprSidecarOptions();
                var folder = Path.GetTempPath();

                p.AssignLocations(options, folder);

                Assert.That(options.ComponentsDirectory, Is.EqualTo(Path.Combine(folder, "components")));
                Assert.That(options.ConfigFile, Is.EqualTo(Path.Combine(folder, "config.yaml")));
            }

            [Test]
            public void Should_assign_specified_paths()
            {
                var p = new MockDaprSidecarProcess();
                var folder = Path.GetTempPath();
                var options = new DaprSidecarOptions
                {
                    ComponentsDirectory = folder + "comps",
                    ConfigFile = folder + "config.txt"
                };

                p.AssignLocations(options, folder);

                Assert.That(options.ComponentsDirectory, Is.EqualTo(Path.Combine(folder, "comps")));
                Assert.That(options.ConfigFile, Is.EqualTo(Path.Combine(folder, "config.txt")));
            }
        }

        public class AddCommandLineArguments
        {
            [Test]
            public void Should_add_default_arguments()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprSidecarOptions();

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo("--log-as-json"));
            }

            [Test]
            public void Should_not_add_empty_sentry_address()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new CommandLineArgumentBuilder();
                var options = new DaprSidecarOptions
                {
                    SentryAddress = string.Empty
                };

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Does.Not.Contain("sentry-address"));
            }

            [Test]
            public void Should_add_all_arguments()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new CommandLineArgumentBuilder();
                var configFile = Path.GetTempFileName();
                var componentsPath = Path.GetDirectoryName(configFile);
                var options = new DaprSidecarOptions
                {
                    AllowedOrigins = "AllowedOrigins",
                    AppId = "AppId",
                    AppMaxConcurrency = 100,
                    AppPort = 1234,
                    AppProtocol = "AppProtocol",
                    AppSsl = true,
                    ComponentsDirectory = componentsPath, // Must exist
                    ConfigFile = configFile, // Must exist
                    ControlPlaneAddress = "ControlPlaneAddress",
                    DaprGrpcPort = 2345,
                    DaprHttpMaxRequestSize = 200,
                    DaprHttpPort = 3456,
                    DaprInternalGrpcPort = 4567,
                    Mtls = true,
                    Profiling = true,
                    KubeConfig = "KubeConfig",
                    LogLevel = "LogLevel",
                    MetricsPort = 5678,
                    Mode = "Mode",
                    PlacementHostAddress = "PlacementHostAddress",
                    ProfilePort = 6789,
                    SentryAddress = "SentryAddress",
                    CustomArguments = "arg1 val1"
                };

                p.AddCommandLineArguments(options, builder);

                Assert.That(builder.ToString(), Is.EqualTo(
                    "--allowed-origins AllowedOrigins " +
                    "--app-id AppId " +
                    "--app-max-concurrency 100 " +
                    "--app-port 1234 " +
                    "--app-protocol AppProtocol " +
                    "--app-ssl " +
                    "--control-plane-address ControlPlaneAddress " +
                    "--dapr-grpc-port 2345 " +
                    "--dapr-http-max-request-size 200 " +
                    "--dapr-http-port 3456 " +
                    "--dapr-internal-grpc-port 4567 " +
                    "--enable-mtls " +
                    "--enable-profiling " +
                    "--kubeconfig KubeConfig " +
                    "--log-as-json " +
                    "--log-level LogLevel " +
                    "--metrics-port 5678 " +
                    "--mode Mode " +
                    "--placement-host-address PlacementHostAddress " +
                    "--profile-port 6789 " +
                    "--sentry-address SentryAddress " +
                    "--config " + configFile + " " +
                    "--components-path " + componentsPath + " " +
                    "--arg1 val1"));

                File.Delete(configFile);
            }
        }

        public class AddEnvironmentVariables
        {
            [Test]
            public void Should_add_expected_values()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new EnvironmentVariableBuilder();
                var options = new DaprSidecarOptions
                {
                    AppApiToken = "AppApiToken",
                    DaprApiToken = "DaprApiToken",
                    IssuerCertificate = "IssuerCertificate",
                    IssuerKey = "IssuerKey",
                    DaprGrpcPort = 1234,
                    DaprHttpPort = 2345,
                    Profiling = true,
                    ProfilePort = 3456,
                    TrustAnchorsCertificate = "TrustAnchorsCertificate",
                    Namespace = "Namespace",
                    Mtls = true
                };

                p.AddEnvironmentVariables(options, builder);

                var values = builder.ToDictionary();
                Assert.That(((SensitiveString)values["APP_API_TOKEN"]).Value, Is.EqualTo("AppApiToken"));
                Assert.That(((SensitiveString)values["DAPR_API_TOKEN"]).Value, Is.EqualTo("DaprApiToken"));
                Assert.That(((SensitiveString)values["DAPR_CERT_CHAIN"]).Value, Is.EqualTo("IssuerCertificate"));
                Assert.That(((SensitiveString)values["DAPR_CERT_KEY"]).Value, Is.EqualTo("IssuerKey"));
                Assert.That(((SensitiveString)values["DAPR_TRUST_ANCHORS"]).Value, Is.EqualTo("TrustAnchorsCertificate"));
                Assert.That(values["DAPR_GRPC_PORT"], Is.EqualTo(1234));
                Assert.That(values["DAPR_HTTP_PORT"], Is.EqualTo(2345));
                Assert.That(values["DAPR_PROFILE_PORT"], Is.EqualTo(3456));
                Assert.That(values["NAMESPACE"], Is.EqualTo("Namespace"));
            }

            [Test]
            public void Should_not_add_predicated_values()
            {
                var p = new MockDaprSidecarProcess();
                var builder = new EnvironmentVariableBuilder();
                var options = new DaprSidecarOptions
                {
                    ProfilePort = 3456,
                    Namespace = "Namespace",
                };

                p.AddEnvironmentVariables(options, builder);

                var values = builder.ToDictionary();
                Assert.That(values, Is.Empty);
            }
        }

        public class ParseCommandLineArgument
        {
            [Test]
            public void Should_parse_all_arguments()
            {
                var p = new MockDaprSidecarProcess();
                var options = new DaprSidecarOptions();

                p.ParseCommandLineArgument(options, "allowed-origins", "AllowedOrigins");
                p.ParseCommandLineArgument(options, "app-id", "AppId");
                p.ParseCommandLineArgument(options, "app-max-concurrency", "100");
                p.ParseCommandLineArgument(options, "app-port", "1234");
                p.ParseCommandLineArgument(options, "app-protocol", "AppProtocol");
                p.ParseCommandLineArgument(options, "app-ssl", null);
                p.ParseCommandLineArgument(options, "components-path", "ComponentsPath");
                p.ParseCommandLineArgument(options, "config", "ConfigFile");
                p.ParseCommandLineArgument(options, "control-plane-address", "ControlPlaneAddress");
                p.ParseCommandLineArgument(options, "dapr-grpc-port", "2345");
                p.ParseCommandLineArgument(options, "dapr-http-max-request-size", "200");
                p.ParseCommandLineArgument(options, "dapr-http-port", "3456");
                p.ParseCommandLineArgument(options, "dapr-internal-grpc-port", "4567");
                p.ParseCommandLineArgument(options, "enable-mtls", null);
                p.ParseCommandLineArgument(options, "kubeconfig", "KubeConfig");
                p.ParseCommandLineArgument(options, "log-level", "LogLevel");
                p.ParseCommandLineArgument(options, "metrics-port", "5678");
                p.ParseCommandLineArgument(options, "mode", "Mode");
                p.ParseCommandLineArgument(options, "placement-host-address", "PlacementHostAddress");
                p.ParseCommandLineArgument(options, "profile-port", "6789");
                p.ParseCommandLineArgument(options, "sentry-address", "SentryAddress");

                Assert.That(options.AllowedOrigins, Is.EqualTo("AllowedOrigins"));
                Assert.That(options.AppId, Is.EqualTo("AppId"));
                Assert.That(options.AppMaxConcurrency, Is.EqualTo(100));
                Assert.That(options.AppPort, Is.EqualTo(1234));
                Assert.That(options.AppProtocol, Is.EqualTo("AppProtocol"));
                Assert.That(options.AppSsl, Is.True);
                Assert.That(options.ComponentsDirectory, Is.EqualTo("ComponentsPath"));
                Assert.That(options.ConfigFile, Is.EqualTo("ConfigFile"));
                Assert.That(options.ControlPlaneAddress, Is.EqualTo("ControlPlaneAddress"));
                Assert.That(options.DaprGrpcPort, Is.EqualTo(2345));
                Assert.That(options.DaprHttpMaxRequestSize, Is.EqualTo(200));
                Assert.That(options.DaprHttpPort, Is.EqualTo(3456));
                Assert.That(options.DaprInternalGrpcPort, Is.EqualTo(4567));
                Assert.That(options.Mtls, Is.True);
                Assert.That(options.KubeConfig, Is.EqualTo("KubeConfig"));
                Assert.That(options.LogLevel, Is.EqualTo("LogLevel"));
                Assert.That(options.MetricsPort, Is.EqualTo(5678));
                Assert.That(options.Mode, Is.EqualTo("Mode"));
                Assert.That(options.PlacementHostAddress, Is.EqualTo("PlacementHostAddress"));
                Assert.That(options.ProfilePort, Is.EqualTo(6789));
                Assert.That(options.SentryAddress, Is.EqualTo("SentryAddress"));
            }
        }

        public class CompareProcessOptions
        {
            [Test]
            public void Should_return_none()
            {
                var p = new MockDaprSidecarProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSidecarOptions
                {
                    AppId = "P1"
                };

                var o2 = new DaprSidecarOptions
                {
                    AppId = "P2"
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.None));
            }

            [Test]
            public void Should_return_duplicate()
            {
                var p = new MockDaprSidecarProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSidecarOptions
                {
                    AppId = "P1",
                    AppPort = 1234
                };

                var o2 = new DaprSidecarOptions
                {
                    AppId = "P1",
                    AppPort = 2345
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Duplicate));
            }

            [Test]
            public void Should_return_attachable()
            {
                var p = new MockDaprSidecarProcess();
                var process = Substitute.For<IProcess>();
                var o1 = new DaprSidecarOptions
                {
                    AppId = "P1",
                    AppPort = 1234
                };

                var o2 = new DaprSidecarOptions
                {
                    AppId = "P1",
                    AppPort = 1234
                };

                Assert.That(p.CompareProcessOptions(o1, o2, process), Is.EqualTo(ProcessComparison.Attachable));
            }
        }

        private class MockDaprSidecarProcess : DaprSidecarProcess
        {
            public new DaprSidecarOptions GetProcessOptions(DaprOptions daprOptions) => base.GetProcessOptions(daprOptions);

            public new void AssignPorts(PortAssignmentBuilder<DaprSidecarOptions> builder) => base.AssignPorts(builder);

            public new void AssignLocations(DaprSidecarOptions options, string daprFolder) => base.AssignLocations(options, daprFolder);

            public new void AddCommandLineArguments(DaprSidecarOptions source, CommandLineArgumentBuilder builder) => base.AddCommandLineArguments(source, builder);

            public new void AddEnvironmentVariables(DaprSidecarOptions source, EnvironmentVariableBuilder builder) => base.AddEnvironmentVariables(source, builder);

            public new void ParseCommandLineArgument(DaprSidecarOptions target, string name, string value) => base.ParseCommandLineArgument(target, name, value);

            public new ProcessComparison CompareProcessOptions(DaprSidecarOptions proposedProcessOptions, DaprSidecarOptions existingProcessOptions, IProcess existingProcess) => base.CompareProcessOptions(proposedProcessOptions, existingProcessOptions, existingProcess);
        }
    }
}
