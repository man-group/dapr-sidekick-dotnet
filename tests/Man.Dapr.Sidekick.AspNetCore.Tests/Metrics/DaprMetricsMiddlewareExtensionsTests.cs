using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.AspNetCore.Metrics
{
    public class DaprMetricsMiddlewareExtensionsTests
    {
        public class MapDaprMetrics
        {
            [Test]
            public void Should_add_default_endpoint()
            {
                var builder = Substitute.For<Microsoft.AspNetCore.Routing.IEndpointRouteBuilder>();
                builder.MapDaprMetrics();
            }

            [Test]
            public void Should_add_named_endpoint()
            {
                var builder = Substitute.For<Microsoft.AspNetCore.Routing.IEndpointRouteBuilder>();
                builder.MapDaprMetrics("TEST");
            }
        }

        public class UseDaprMetricsServer
        {
            [Test]
            public void Should_map_specified_port()
            {
                var builder = Substitute.For<IApplicationBuilder>();
                builder.UseDaprMetricsServer(1234);
            }

            [Test]
            public void Should_map_default_url()
            {
                var builder = Substitute.For<IApplicationBuilder>();
                builder.UseDaprMetricsServer();
            }

            [Test]
            public void Should_map_specified_url()
            {
                var builder = Substitute.For<IApplicationBuilder>();
                builder.UseDaprMetricsServer("/TEST");
            }

            [Test]
            public void Should_add_without_url()
            {
                var builder = Substitute.For<IApplicationBuilder>();
                builder.UseDaprMetricsServer(null);
                builder.Received(1).Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>());
            }
        }
    }
}
