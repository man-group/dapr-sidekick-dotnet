using System;
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
        public class Constructor
        {
            [Test]
            public void Should_set_properties()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();

                var host = new DaprSidecarHost(daprProcessFactory, daprHttpClientFactory, apiTokenManager, loggerFactory);

                // Check HttpClientFactory
                Assert.That(host.DaprHttpClientFactory, Is.SameAs(daprHttpClientFactory));

                // Check Logger
                Assert.That(host.Logger, Is.InstanceOf<DaprLogger<DaprSidecarHost>>());
                loggerFactory.Received(1).CreateLogger("Dapr.Sidekick.DaprSidecarHost");

                // Check ProcessFactory
                var cancellationToken = DaprCancellationToken.None;
                host.Start(null, cancellationToken);
                daprSidecarProcess.Received(1).Start(Arg.Any<Func<DaprOptions>>(), host.Logger, cancellationToken);
            }
        }

        public class OnProcessStarting
        {
            [Test]
            public void Should_invoke_interceptors()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var interceptor1 = Substitute.For<IDaprSidecarProcessInterceptor>();
                var interceptor2 = Substitute.For<IDaprSidecarProcessInterceptor>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory,
                    new[] { interceptor1, interceptor2 });

                var options = new DaprSidecarOptions();
                var args = new DaprProcessStartingEventArgs<DaprSidecarOptions>(options);
                host.Start(null, default);
                daprSidecarProcess.Starting += Raise.Event<EventHandler<DaprProcessStartingEventArgs<DaprSidecarOptions>>>(null, args);

                interceptor1.Received(1).OnStarting(options);
                interceptor2.Received(1).OnStarting(options);
            }

            [Test]
            public void Should_set_specified_api_tokens()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions()
                {
                    AppApiToken = "APP_TOKEN",
                    DaprApiToken = "DAPR_TOKEN"
                };

                var args = new DaprProcessStartingEventArgs<DaprSidecarOptions>(options);
                host.Start(null, default);
                daprSidecarProcess.Starting += Raise.Event<EventHandler<DaprProcessStartingEventArgs<DaprSidecarOptions>>>(null, args);

                apiTokenManager.Received(1).SetAppApiToken("APP_TOKEN");
                apiTokenManager.Received(1).SetDaprApiToken("DAPR_TOKEN");
            }

            [Test]
            public void Should_clear_api_tokens()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions()
                {
                    UseDefaultAppApiToken = false,
                    UseDefaultDaprApiToken = false
                };

                var args = new DaprProcessStartingEventArgs<DaprSidecarOptions>(options);
                host.Start(null, default);
                daprSidecarProcess.Starting += Raise.Event<EventHandler<DaprProcessStartingEventArgs<DaprSidecarOptions>>>(null, args);

                apiTokenManager.Received(1).SetAppApiToken(null);
                apiTokenManager.Received(1).SetDaprApiToken(null);
            }

            [Test]
            public void Should_use_default_app_api_token()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                apiTokenManager.AppApiToken.Returns(new SensitiveString("APP_TOKEN"));
                apiTokenManager.DaprApiToken.Returns(new SensitiveString("DAPR_TOKEN"));
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions()
                {
                    UseDefaultAppApiToken = true,
                    UseDefaultDaprApiToken = false
                };

                var args = new DaprProcessStartingEventArgs<DaprSidecarOptions>(options);
                host.Start(null, default);
                daprSidecarProcess.Starting += Raise.Event<EventHandler<DaprProcessStartingEventArgs<DaprSidecarOptions>>>(null, args);

                Assert.That(options.AppApiToken?.Value, Is.EqualTo("APP_TOKEN"));
                Assert.That(options.DaprApiToken, Is.Null);

                apiTokenManager.DidNotReceive().SetAppApiToken(Arg.Any<SensitiveString>());
                apiTokenManager.Received(1).SetDaprApiToken(null);
            }

            [Test]
            public void Should_use_default_dapr_api_token()
            {
                var daprProcessFactory = Substitute.For<IDaprProcessFactory>();
                var daprSidecarProcess = Substitute.For<IDaprSidecarProcess>();
                daprProcessFactory.CreateDaprSidecarProcess().Returns(daprSidecarProcess);
                var daprHttpClientFactory = Substitute.For<IDaprProcessHttpClientFactory>();
                var apiTokenManager = Substitute.For<IDaprApiTokenManager>();
                apiTokenManager.AppApiToken.Returns(new SensitiveString("APP_TOKEN"));
                apiTokenManager.DaprApiToken.Returns(new SensitiveString("DAPR_TOKEN"));
                var loggerFactory = Substitute.For<IDaprLoggerFactory>();
                var host = new DaprSidecarHost(
                    daprProcessFactory,
                    daprHttpClientFactory,
                    apiTokenManager,
                    loggerFactory);

                var options = new DaprSidecarOptions()
                {
                    UseDefaultAppApiToken = false,
                    UseDefaultDaprApiToken = true
                };

                var args = new DaprProcessStartingEventArgs<DaprSidecarOptions>(options);
                host.Start(null, default);
                daprSidecarProcess.Starting += Raise.Event<EventHandler<DaprProcessStartingEventArgs<DaprSidecarOptions>>>(null, args);

                Assert.That(options.AppApiToken, Is.Null);
                Assert.That(options.DaprApiToken?.Value, Is.EqualTo("DAPR_TOKEN"));

                apiTokenManager.Received(1).SetAppApiToken(null);
                apiTokenManager.DidNotReceive().SetDaprApiToken(Arg.Any<SensitiveString>());
            }
        }
    }
}
