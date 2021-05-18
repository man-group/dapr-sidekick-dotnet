using System;
using NUnit.Framework;

namespace Dapr.Sidekick.Process
{
    public class PortAvailabilityCheckerTests
    {
        public class GetAvailablePort
        {
            [Test]
            public void Should_throw_exception_when_invalid_startingport()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentException>().With.Message.StartsWith("Starting Port cannot be greater than 65535"),
                    () => new PortAvailabilityChecker().GetAvailablePort(65536));
            }

            [Test]
            public void Should_assign_next_available_port()
            {
                var result = new PortAvailabilityChecker().GetAvailablePort(65500);
                Assert.That(result, Is.GreaterThanOrEqualTo(65500));
            }

            [Test]
            public void Should_not_assign_reserved_port()
            {
                var result = new PortAvailabilityChecker().GetAvailablePort(65500, new[] { 65500, 65501, 65502 });
                Assert.That(result, Is.GreaterThan(65502));
            }
        }
    }
}
