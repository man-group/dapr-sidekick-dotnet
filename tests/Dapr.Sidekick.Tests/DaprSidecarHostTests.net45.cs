#if !NET35
#pragma warning disable RCS1163 // Unused parameter.
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;
using Dapr.Sidekick.Threading;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick
{
    public partial class DaprSidecarHostTests
    {
        public class WriteMetadataAsync
        {
            [Test]
            public void Should_throw_exception_when_null_stream()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                Assert.ThrowsAsync(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("stream"),
                    () => host.WriteMetadataAsync(null, default));
            }

            [Test]
            public void Should_throw_exception_when_stream_not_writeable()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var stream = new MemoryStream();
                stream.Close();
                Assert.ThrowsAsync(
                    Is.InstanceOf<InvalidOperationException>().With.Message.EqualTo("Stream provided for Dapr Sidecar Metadata is not writeable"),
                    () => host.WriteMetadataAsync(stream, default));
            }

            [Test]
            public async Task Should_return_zero_when_invalid_uri()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var stream = new MemoryStream();
                host.Start(null, default);
                Assert.That(await host.WriteMetadataAsync(stream, default), Is.Zero);
            }

            [Test]
            public async Task Should_return_zero_when_invalid_client()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var stream = new MemoryStream();
                daprHttpClientFactory.CreateDaprHttpClient().Returns((HttpClient)null);
                host.Start(null, default);
                Assert.That(await host.WriteMetadataAsync(stream, default), Is.Zero);
            }

            [Test]
            public async Task Should_return_zero_and_log_error_when_client_exception()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var stream = new MemoryStream();
                daprHttpClientFactory.CreateDaprHttpClient().Returns(new HttpClient());
                host.GetAsync = (client, uri, cancellationToken) => throw new Exception("ERROR");
                host.Start(null, default);
                Assert.That(await host.WriteMetadataAsync(stream, default), Is.Zero);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("An error occurred while obtaining the Dapr sidecar metadata from http://127.0.0.1:1234/v1.0/metadata"));
                Assert.That(loggerCalls[0].Exception.Message.Contains("ERROR"));
            }

            [Test]
            public async Task Should_return_bytes_written_to_stream()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var stream = new MemoryStream();
                daprHttpClientFactory.CreateDaprHttpClient().Returns(new HttpClient());
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(new byte[10])
                };
                host.GetAsync = (client, uri, cancellationToken) => Task.FromResult(response);
                host.Start(null, default);
                Assert.That(await host.WriteMetadataAsync(stream, default), Is.EqualTo(10));
                Assert.That(stream.ToArray().Length, Is.EqualTo(10));
            }
        }

        public class OnProcessStopping
        {
            [Test]
            public void Should_not_invoke_shutdown_when_invalid_uri()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions();
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var cancellationToken = DaprCancellationToken.None;
                var args = new DaprProcessStoppingEventArgs(cancellationToken);
                host.Start(null, default);
                daprSidecarProcess.Stopping += Raise.Event<EventHandler<DaprProcessStoppingEventArgs>>(args);
                Assert.That(logger.ReceivedLoggerCalls(), Is.Empty);
            }

            [Test]
            public void Should_not_invoke_shutdown_when_invalid_client()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var cancellationToken = DaprCancellationToken.None;
                var args = new DaprProcessStoppingEventArgs(cancellationToken);
                host.Start(null, default);
                daprSidecarProcess.Stopping += Raise.Event<EventHandler<DaprProcessStoppingEventArgs>>(args);
                Assert.That(logger.ReceivedLoggerCalls(), Is.Empty);
            }

            [Test]
            public void Should_log_error_when_client_exception()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);

                var stream = new MemoryStream();
                daprHttpClientFactory.CreateDaprHttpClient().Returns(new HttpClient());
                var content = new StringContent(string.Empty);
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    ReasonPhrase = "ERROR"
                };
                host.PostAsync = (client, uri, content, cancellationToken) => Task.FromResult(response);

                var cancellationToken = DaprCancellationToken.None;
                var args = new DaprProcessStoppingEventArgs(cancellationToken);
                host.Start(null, default);
                daprSidecarProcess.Stopping += Raise.Event<EventHandler<DaprProcessStoppingEventArgs>>(args);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(2));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("Sending Shutdown command to Sidecar"));
                Assert.That(loggerCalls[1].LogLevel, Is.EqualTo(DaprLogLevel.Error));
                Assert.That(loggerCalls[1].Message, Is.EqualTo("Shutdown command was not processed successfully by Sidecar. Reason: ERROR"));
            }

            [Test]
            public void Should_send_shutdown()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var logger = Substitute.For<IDaprLogger>();
                loggerFactory.CreateLogger(Arg.Any<string>()).Returns(logger);
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions
                {
                    DaprHttpPort = 1234
                };
                daprSidecarProcess.LastSuccessfulOptions.Returns(options);


                var stream = new MemoryStream();
                daprHttpClientFactory.CreateDaprHttpClient().Returns(new HttpClient());
                var content = new ByteArrayContent(new byte[10]);
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content
                };
                host.PostAsync = (client, uri, content, cancellationToken) => Task.FromResult(response);

                var cancellationToken = DaprCancellationToken.None;
                var args = new DaprProcessStoppingEventArgs(cancellationToken);
                host.Start(null, default);
                daprSidecarProcess.Stopping += Raise.Event<EventHandler<DaprProcessStoppingEventArgs>>(args);

                var loggerCalls = logger.ReceivedLoggerCalls();
                Assert.That(loggerCalls.Length, Is.EqualTo(1));
                Assert.That(loggerCalls[0].LogLevel, Is.EqualTo(DaprLogLevel.Information));
                Assert.That(loggerCalls[0].Message, Is.EqualTo("Sending Shutdown command to Sidecar"));
            }
        }
    }
}
#pragma warning restore RCS1163 // Unused parameter.
#endif
