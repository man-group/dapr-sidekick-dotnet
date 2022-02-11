using System;
using System.Threading.Tasks;
using Dapr.Client;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Threading;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.IntegrationTests
{
    public class Tests
    {
        [Test]
        public async Task PublishEvent()
        {
            var client = new DaprClientBuilder()
                .UseGrpcEndpoint("http://localhost:3001")
                .Build();

            var sidekick = new DaprSidekickBuilder().WithLoggerFactory(new DaprColoredConsoleLoggerFactory()).Build();
            sidekick.Sidecar.Start(
                () => new DaprOptions
                {
                    Sidecar = new DaprSidecarOptions { ComponentsDirectory = "components", DaprGrpcPort = 3001 }
                }, DaprCancellationToken.None);

            await Task.Delay(TimeSpan.FromMilliseconds(200)); // <-- try commenting this out

            await client.PublishEventAsync("my-pubsub", "topic", "data");
        }
    }
}
