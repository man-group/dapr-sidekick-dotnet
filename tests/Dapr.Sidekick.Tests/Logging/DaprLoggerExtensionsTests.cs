using System;
using Dapr.Sidekick.Logging.Internal;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick.Logging
{
    public class DaprLoggerExtensionsTests
    {
        public class LogDebug
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogDebug(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Debug,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogDebug(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Debug,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogDebug(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Debug,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogDebug(message, args);
                logger.Received().Log(
                    DaprLogLevel.Debug,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class LogTrace
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogTrace(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Trace,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogTrace(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Trace,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogTrace(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Trace,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogTrace(message, args);
                logger.Received().Log(
                    DaprLogLevel.Trace,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class LogInformation
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogInformation(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Information,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogInformation(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Information,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogInformation(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Information,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogInformation(message, args);
                logger.Received().Log(
                    DaprLogLevel.Information,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class LogWarning
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogWarning(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Warning,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogWarning(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Warning,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogWarning(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Warning,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogWarning(message, args);
                logger.Received().Log(
                    DaprLogLevel.Warning,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class LogError
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogError(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Error,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogError(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Error,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogError(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Error,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogError(message, args);
                logger.Received().Log(
                    DaprLogLevel.Error,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class LogCritical
        {
            [Test]
            public void Should_log_with_eventid_and_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogCritical(eventId, ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Critical,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_eventid()
            {
                var logger = Substitute.For<IDaprLogger>();

                var eventId = new DaprEventId(100);
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogCritical(eventId, message, args);
                logger.Received().Log(
                    DaprLogLevel.Critical,
                    eventId,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_exception()
            {
                var logger = Substitute.For<IDaprLogger>();

                var ex = new Exception();
                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogCritical(ex, message, args);
                logger.Received().Log(
                    DaprLogLevel.Critical,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    ex,
                    Arg.Any<Func<object, Exception, string>>());
            }

            [Test]
            public void Should_log_with_message()
            {
                var logger = Substitute.For<IDaprLogger>();

                var message = "TEST {0} {1}";
                var args = new[] { "ONE", "TWO" };

                logger.LogCritical(message, args);
                logger.Received().Log(
                    DaprLogLevel.Critical,
                    0,
                    Arg.Is<DaprFormattedLogValues>(x => x.ToString() == "TEST ONE TWO"),
                    null,
                    Arg.Any<Func<object, Exception, string>>());
            }
        }

        public class Log
        {
            [Test]
            public void Should_throw_exception_when_null_logger()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("logger"),
                    () => DaprLoggerExtensions.Log(null, DaprLogLevel.Critical, "TEST", null));
            }
        }
    }
}
