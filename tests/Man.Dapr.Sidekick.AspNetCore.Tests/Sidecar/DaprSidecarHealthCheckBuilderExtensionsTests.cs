using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarHealthCheckBuilderExtensionsTests
    {
        public class AddDaprSidecar
        {
            [Test]
            public void Should_add_check_with_default_name()
            {
                var builder = Substitute.For<IHealthChecksBuilder>();
                builder.When(x => x.Add(Arg.Any<HealthCheckRegistration>())).Do(ci =>
                {
                    var registration = (HealthCheckRegistration)ci[0];
                    Assert.That(registration.Name, Is.EqualTo("dapr-sidecar"));
                });

                builder.AddDaprSidecar();
                builder.Received(1).Add(Arg.Any<HealthCheckRegistration>());
            }

            [Test]
            public void Should_add_check_with_specified_name()
            {
                var builder = Substitute.For<IHealthChecksBuilder>();
                builder.When(x => x.Add(Arg.Any<HealthCheckRegistration>())).Do(ci =>
                {
                    var registration = (HealthCheckRegistration)ci[0];
                    Assert.That(registration.Name, Is.EqualTo("HOST_NAME"));
                });

                builder.AddDaprSidecar("HOST_NAME");
                builder.Received(1).Add(Arg.Any<HealthCheckRegistration>());
            }
        }
    }
}
