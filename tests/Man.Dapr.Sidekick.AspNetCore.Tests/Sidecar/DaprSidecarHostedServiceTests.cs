using System;
using System.Threading;
using System.Threading.Tasks;
using Man.Dapr.Sidekick.Threading;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Sidecar
{
    public class DaprSidecarHostedServiceTests
    {
        public class OnStarting
        {
            [Test]
            public async Task Should_set_options()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP"
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serverAddressesFeature = Substitute.For<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Returns(new[] { "http://127.0.0.1:1234" });
                server.Features.Get<IServerAddressesFeature>().Returns(serverAddressesFeature);
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Sidecar, Is.Not.Null);
                    Assert.That(options.Sidecar.AppPort, Is.EqualTo(1234)); // From Server Feature
                    Assert.That(options.Sidecar.Metrics, Is.Not.Null);
                    Assert.That(options.Sidecar.Metrics.Labels["app"], Is.EqualTo("dapr-sidecar"));
                    Assert.That(options.Sidecar.Metrics.Labels["service"], Is.EqualTo("TEST_APP"));
                });

                await hostedService.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_select_specific_https_port()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP",
                        AppSsl = true
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serverAddressesFeature = Substitute.For<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Returns(new[] { "http://127.0.0.1:1234", "https://127.0.0.1:2345" });
                server.Features.Get<IServerAddressesFeature>().Returns(serverAddressesFeature);
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Sidecar, Is.Not.Null);
                    Assert.That(options.Sidecar.AppPort, Is.EqualTo(2345)); // HTTPS PORT
                });

                await hostedService.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_select_specific_http_port()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP",
                        AppSsl = false
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serverAddressesFeature = Substitute.For<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Returns(new[] { "http://127.0.0.1:1234", "https://127.0.0.1:2345", "@INVALID_URI\\" });
                server.Features.Get<IServerAddressesFeature>().Returns(serverAddressesFeature);
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Sidecar, Is.Not.Null);
                    Assert.That(options.Sidecar.AppPort, Is.EqualTo(1234)); // HTTP PORT
                });

                await hostedService.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_select_default_http_port()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP"
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serverAddressesFeature = Substitute.For<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Returns(new[] { "https://127.0.0.1:2345", "http://127.0.0.1:3456", "::__@INVALID_URI\\!+&", "https://127.0.0.1:2345" });
                server.Features.Get<IServerAddressesFeature>().Returns(serverAddressesFeature);
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Sidecar, Is.Not.Null);
                    Assert.That(options.Sidecar.AppPort, Is.EqualTo(3456)); // HTTP PORT
                });

                await hostedService.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_select_default_https_port()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP"
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serverAddressesFeature = Substitute.For<IServerAddressesFeature>();
                serverAddressesFeature.Addresses.Returns(new[] { "https://127.0.0.1:2345", "::__@INVALID_URI\\!+&" });
                server.Features.Get<IServerAddressesFeature>().Returns(serverAddressesFeature);
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cancellationToken = CancellationToken.None;

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                    Assert.That(newOptions, Is.SameAs(options));

                    // Check the options have been updated
                    Assert.That(options.Sidecar, Is.Not.Null);
                    Assert.That(options.Sidecar.AppPort, Is.EqualTo(2345)); // HTTPS PORT
                });

                await hostedService.StartAsync(cancellationToken);
            }

            [Test]
            public async Task Should_not_wait_for_hosting_if_cancelled()
            {
                var options = new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions
                    {
                        AppId = "TEST_APP"
                    }
                };

                var optionsAccessor = Substitute.For<IOptionsMonitor<DaprOptions>>();
                optionsAccessor.CurrentValue.Returns(options);
                var host = Substitute.For<IDaprSidecarHost>();
                var server = Substitute.For<IServer>();
                var serviceProvider = Substitute.For<IServiceProvider>();
                serviceProvider.GetService(typeof(IServer)).Returns(server);
                var hostedService = new DaprSidecarHostedService(host, optionsAccessor, serviceProvider);
                var cts = new CancellationTokenSource();

                host.When(x => x.Start(Arg.Any<Func<DaprOptions>>(), Arg.Any<DaprCancellationToken>())).Do(ci =>
                {
                    // Execute the accessor
                    var accessor = (Func<DaprOptions>)ci[0];
                    var newOptions = accessor();
                });

                // Start a timer to cancel the process
                var timer = new System.Timers.Timer(20);
                timer.Elapsed += (sender, args) =>
                {
                    cts.Cancel();
                };

                timer.Start();
                await hostedService.StartAsync(cts.Token);

                // Sidecar should not be null (so accessor was invoked) and AppPort not set
                Assert.That(options.Sidecar, Is.Not.Null);
                Assert.That(options.Sidecar.AppPort, Is.Null);
            }
        }
    }
}
