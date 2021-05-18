using System;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Logging
{
    public class DaprLoggerOfTTests
    {
        public class Constructor
        {
            [Test]
            public void Should_throw_exception_when_null_factory()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("factory"),
                    () => new DaprLogger<DaprDisposable>(null));
            }
        }

        public class BeginScope
        {
            [Test]
            public void Should_return_disposable()
            {
                var factory = Substitute.For<IDaprLoggerFactory>();
                var logger = (IDaprLogger)new DaprLogger<DaprDisposable>(factory);
                Assert.That(logger.BeginScope("TEST"), Is.Not.Null);
            }
        }

        public class IsEnabled
        {
            [Test]
            public void Should_return_loglevel_value()
            {
                var innerLogger = Substitute.For<IDaprLogger>();
                var factory = Substitute.For<IDaprLoggerFactory>();
                factory.CreateLogger(typeof(DaprDisposable).FullName).Returns(innerLogger);
                var logger = (IDaprLogger)new DaprLogger<DaprDisposable>(factory);

                innerLogger.IsEnabled(DaprLogLevel.Critical).Returns(false);
                innerLogger.IsEnabled(DaprLogLevel.Information).Returns(false);
                Assert.That(logger.IsEnabled(DaprLogLevel.Critical), Is.False);
                Assert.That(logger.IsEnabled(DaprLogLevel.Information), Is.False);

                innerLogger.IsEnabled(DaprLogLevel.Critical).Returns(true);
                Assert.That(logger.IsEnabled(DaprLogLevel.Critical), Is.True);
                Assert.That(logger.IsEnabled(DaprLogLevel.Information), Is.False);

                innerLogger.IsEnabled(DaprLogLevel.Critical).Returns(false);
                innerLogger.IsEnabled(DaprLogLevel.Information).Returns(true);
                Assert.That(logger.IsEnabled(DaprLogLevel.Critical), Is.False);
                Assert.That(logger.IsEnabled(DaprLogLevel.Information), Is.True);
            }
        }

        public class Log
        {
            [Test]
            public void Should_pass_all_parameters_to_inner_logger()
            {
                var innerLogger = Substitute.For<IDaprLogger>();
                var factory = Substitute.For<IDaprLoggerFactory>();
                factory.CreateLogger(typeof(DaprDisposable).FullName).Returns(innerLogger);
                var logger = (IDaprLogger)new DaprLogger<DaprDisposable>(factory);

                var logLevel = DaprLogLevel.Debug;
                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var state = "TEST";

                static string Formatter(string s, Exception e) => string.Format("{0}:{1}", s, e);

                logger.Log(logLevel, eventId, state, ex, Formatter);
                innerLogger.Received(1).Log(logLevel, eventId, state, ex, Formatter);
            }
        }
    }
}
