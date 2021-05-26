using System;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Logging
{
    public class DaprColoredConsoleLoggerFactoryTests
    {
        public class CreateLogger
        {
            [Test]
            public void Should_create_colored_console_logger()
            {
                var factory = new DaprColoredConsoleLoggerFactory();
                var logger = factory.CreateLogger("test");
                Assert.That(logger, Is.TypeOf<Internal.DaprColoredConsoleLogger>());
            }

            [Test]
            public void Should_create_new_instance_by_category()
            {
                var factory = new DaprColoredConsoleLoggerFactory();
                Assert.That(factory.CreateLogger("test"), Is.Not.SameAs(factory.CreateLogger("test2")));
            }

            [Test]
            public void Should_reuse_instance_for_same_category()
            {
                var factory = new DaprColoredConsoleLoggerFactory();
                Assert.That(factory.CreateLogger("test"), Is.SameAs(factory.CreateLogger("test")));
            }

            [Test]
            public void Should_throw_exception_when_disposed()
            {
                var factory = new DaprColoredConsoleLoggerFactory();
                factory.Dispose();
                Assert.Throws(Is.TypeOf<ObjectDisposedException>(), () => factory.CreateLogger("test"));
            }
        }
    }
}
