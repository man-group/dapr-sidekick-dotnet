using System.Collections.Generic;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class PortAssignmentBuilderTests
    {
        public class Constructor
        {
            [Test]
            public void Should_create_default_availabilitychecker()
            {
                Assert.That(new PortAssignmentBuilder<DaprSidecarOptions>().PortAvailabilityChecker, Is.InstanceOf<PortAvailabilityChecker>());
            }
        }

        public class Build
        {
            [Test]
            public void Should_assign_all_new_ports()
            {
                var logger = Substitute.For<IDaprLogger>();
                var checker = Substitute.For<IPortAvailabilityChecker>();
                var builder = new PortAssignmentBuilder<DaprSidecarOptions>(checker);
                builder
                    .Add(x => x.AppPort, 1000)
                    .Add(x => x.DaprGrpcPort, 2000)
                    .Add(x => x.DaprHttpPort, 3000)
                    .Add(x => x.MetricsPort, 4000)
                    .Add(x => x.ProfilePort, 5000);
                checker.GetAvailablePort(1000, Arg.Any<IEnumerable<int>>()).Returns(1000);
                checker.GetAvailablePort(2000, Arg.Any<IEnumerable<int>>()).Returns(2000);
                checker.GetAvailablePort(3000, Arg.Any<IEnumerable<int>>()).Returns(3000);
                checker.GetAvailablePort(4000, Arg.Any<IEnumerable<int>>()).Returns(4000);
                checker.GetAvailablePort(5000, Arg.Any<IEnumerable<int>>()).Returns(5000);

                var lastSuccessfulOptions = new DaprSidecarOptions();
                var proposedOptions = new DaprSidecarOptions()
                {
                    RetainPortsOnRestart = false
                };

                builder.Build(proposedOptions, lastSuccessfulOptions, logger);
                Assert.That(proposedOptions.AppPort, Is.EqualTo(1000));
                Assert.That(proposedOptions.DaprGrpcPort, Is.EqualTo(2000));
                Assert.That(proposedOptions.DaprHttpPort, Is.EqualTo(3000));
                Assert.That(proposedOptions.MetricsPort, Is.EqualTo(4000));
                Assert.That(proposedOptions.ProfilePort, Is.EqualTo(5000));
            }

            [Test]
            public void Should_use_specified_ports()
            {
                var logger = Substitute.For<IDaprLogger>();
                var checker = Substitute.For<IPortAvailabilityChecker>();
                var builder = new PortAssignmentBuilder<DaprSidecarOptions>(checker);
                builder
                    .Add(x => x.AppPort, 1000)
                    .Add(x => x.DaprGrpcPort, 2000)
                    .Add(x => x.DaprHttpPort, 2000);

                checker.GetAvailablePort(2000, Arg.Any<IEnumerable<int>>()).Returns(3456); // Return a different port

                var lastSuccessfulOptions = new DaprSidecarOptions();
                var proposedOptions = new DaprSidecarOptions()
                {
                    AppPort = 1234,
                    DaprGrpcPort = 2345
                };

                builder.Build(proposedOptions, lastSuccessfulOptions, logger);
                Assert.That(proposedOptions.AppPort, Is.EqualTo(1234));
                Assert.That(proposedOptions.DaprGrpcPort, Is.EqualTo(2345));
                Assert.That(proposedOptions.DaprHttpPort, Is.EqualTo(3456));
            }

            [Test]
            public void Should_retain_previous_ports()
            {
                var logger = Substitute.For<IDaprLogger>();
                var checker = Substitute.For<IPortAvailabilityChecker>();
                var builder = new PortAssignmentBuilder<DaprSidecarOptions>(checker);
                builder
                    .Add(x => x.AppPort, 1000)
                    .Add(x => x.DaprGrpcPort, 2000)
                    .Add(x => x.DaprHttpPort, 3000)
                    .Add(x => x.MetricsPort, 4000);

                var lastSuccessfulOptions = new DaprSidecarOptions()
                {
                    AppPort = 1234,
                    DaprGrpcPort = 2345,
                    DaprHttpPort = 3456
                };

                var proposedOptions = new DaprSidecarOptions()
                {
                    DaprHttpPort = 3000, // Overrides previous port
                    MetricsPort = 4000
                };

                builder.Build(proposedOptions, lastSuccessfulOptions, logger);
                Assert.That(proposedOptions.AppPort, Is.EqualTo(1234));
                Assert.That(proposedOptions.DaprGrpcPort, Is.EqualTo(2345));
                Assert.That(proposedOptions.DaprHttpPort, Is.EqualTo(3000));
                Assert.That(proposedOptions.MetricsPort, Is.EqualTo(4000));
            }

            [Test]
            public void Should_use_starting_ports()
            {
                var logger = Substitute.For<IDaprLogger>();
                var checker = Substitute.For<IPortAvailabilityChecker>();
                var builder = new PortAssignmentBuilder<DaprSidecarOptions>(checker)
                {
                    AlwaysUseStartingPort = true
                };

                builder
                    .Add(x => x.AppPort, 1000)
                    .Add(x => x.DaprGrpcPort, 2000)
                    .Add(x => x.DaprHttpPort, 3000)
                    .Add(x => x.MetricsPort, 4000);

                var proposedOptions = new DaprSidecarOptions()
                {
                    DaprHttpPort = 200 // Overrides previous port
                };

                builder.Build(proposedOptions, null, logger);
                Assert.That(proposedOptions.AppPort, Is.EqualTo(1000));
                Assert.That(proposedOptions.DaprGrpcPort, Is.EqualTo(2000));
                Assert.That(proposedOptions.DaprHttpPort, Is.EqualTo(200));
                Assert.That(proposedOptions.MetricsPort, Is.EqualTo(4000));
            }
        }
    }
}
