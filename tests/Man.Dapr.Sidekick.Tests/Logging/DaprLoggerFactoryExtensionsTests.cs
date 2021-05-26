using System;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Logging
{
    public class DaprLoggerFactoryExtensionsTests
    {
        public class CreateLogger
        {
            [Test]
            public void Should_throw_exception_when_null_factory()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("factory"),
                    () => DaprLoggerFactoryExtensions.CreateLogger(null, null));
            }

            [Test]
            public void Should_throw_exception_when_null_type()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("type"),
                    () => DaprLoggerFactoryExtensions.CreateLogger(Substitute.For<IDaprLoggerFactory>(), null));
            }

            [Test]
            public void Should_create_for_type()
            {
                var factory = Substitute.For<IDaprLoggerFactory>();
                var expectedLogger = Substitute.For<IDaprLogger>();
                factory.CreateLogger(typeof(DaprDisposable).FullName).Returns(expectedLogger);

                var logger = factory.CreateLogger(typeof(DaprDisposable));
                Assert.That(logger, Is.SameAs(expectedLogger));
            }
        }

        public class CreateLoggerOfT
        {
            [Test]
            public void Should_throw_exception_when_null_factory()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("factory"),
                    () => DaprLoggerFactoryExtensions.CreateLogger<DaprDisposable>(null));
            }

            [Test]
            public void Should_create_for_type()
            {
                var factory = Substitute.For<IDaprLoggerFactory>();
                var expectedLogger = Substitute.For<IDaprLogger>();
                factory.CreateLogger(typeof(DaprDisposable).FullName).Returns(expectedLogger);

                var logger = factory.CreateLogger<DaprDisposable>();
                Assert.That(logger, Is.TypeOf<DaprLogger<DaprDisposable>>());

                // Make sure the inner logger is the expected logger
                _ = logger.IsEnabled(DaprLogLevel.Error);
                expectedLogger.Received(1).IsEnabled(DaprLogLevel.Error);
            }
        }
    }
}
