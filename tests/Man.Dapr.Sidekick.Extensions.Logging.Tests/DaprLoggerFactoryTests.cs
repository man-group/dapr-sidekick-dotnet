using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Extensions.Logging
{
    public class DaprLoggerFactoryTests
    {
        public class Constructor
        {
            [Test]
            public void Should_throw_exception_when_null_loggerfactory()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("loggerFactory"),
                    () => new DaprLoggerFactory(null));
            }
        }

        public class CreateLogger
        {
            [Test]
            public void Should_create_different_instances_for_category()
            {
                var loggerFactory = Substitute.For<ILoggerFactory>();
                var daprLoggerFactory = new DaprLoggerFactory(loggerFactory);

                var daprLogger1 = daprLoggerFactory.CreateLogger("TEST1");
                var daprLogger2 = daprLoggerFactory.CreateLogger("TEST2");

                Assert.That(daprLogger1, Is.Not.Null);
                Assert.That(daprLogger2, Is.Not.Null);
                Assert.That(daprLogger2, Is.Not.SameAs(daprLogger1));
            }

            [Test]
            public void Should_reuse_same_instance_for_category()
            {
                var loggerFactory = Substitute.For<ILoggerFactory>();
                var daprLoggerFactory = new DaprLoggerFactory(loggerFactory);

                var daprLogger1 = daprLoggerFactory.CreateLogger("TEST");
                var daprLogger2 = daprLoggerFactory.CreateLogger("TEST");

                Assert.That(daprLogger1, Is.Not.Null);
                Assert.That(daprLogger2, Is.Not.Null);
                Assert.That(daprLogger2, Is.SameAs(daprLogger1));
            }
        }
    }
}
