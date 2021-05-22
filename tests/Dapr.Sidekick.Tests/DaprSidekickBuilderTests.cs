using System;
using System.Linq;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;
using NSubstitute;
using NUnit.Framework;

namespace Dapr.Sidekick
{
    public class DaprSidekickBuilderTests
    {
        public class WithApiTokenManager
        {
            [Test]
            public void Should_throw_exception_when_null_value()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () => new DaprSidekickBuilder().WithApiTokenManager(null));
            }

            [Test]
            public void Should_build_with_specific_value()
            {
                var value = Substitute.For<IDaprApiTokenManager>();
                var sidekick = new DaprSidekickBuilder().WithApiTokenManager(value).Build();
                Assert.That(sidekick.ApiTokenManager, Is.SameAs(value));
            }

            [Test]
            public void Should_build_with_default_value()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.ApiTokenManager, Is.TypeOf<DaprApiTokenManager>());
            }
        }

        public class WithHttpClientFactory
        {
            [Test]
            public void Should_throw_exception_when_null_value()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () => new DaprSidekickBuilder().WithHttpClientFactory(null));
            }

            [Test]
            public void Should_build_with_specific_value()
            {
                var value = Substitute.For<IDaprSidecarHttpClientFactory>();
                var sidekick = new DaprSidekickBuilder().WithHttpClientFactory(value).Build();
                Assert.That(sidekick.HttpClientFactory, Is.SameAs(value));
            }

            [Test]
            public void Should_build_with_default_value()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.HttpClientFactory, Is.TypeOf<DaprSidecarHttpClientFactory>());
            }
        }

        public class WithLoggerFactory
        {
            [Test]
            public void Should_throw_exception_when_null_value()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () => new DaprSidekickBuilder().WithLoggerFactory(null));
            }

            [Test]
            public void Should_build_with_specific_value()
            {
                var value = Substitute.For<IDaprLoggerFactory>();
                var sidekick = new DaprSidekickBuilder().WithLoggerFactory(value).Build();
                Assert.That(sidekick.LoggerFactory, Is.SameAs(value));
            }

            [Test]
            public void Should_build_with_default_value()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.LoggerFactory, Is.TypeOf<DaprColoredConsoleLoggerFactory>());
            }
        }

        public class WithProcessFactory
        {
            [Test]
            public void Should_throw_exception_when_null_value()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () => new DaprSidekickBuilder().WithProcessFactory(null));
            }

            [Test]
            public void Should_build_with_specific_value()
            {
                var value = Substitute.For<IDaprProcessFactory>();
                var sidekick = new DaprSidekickBuilder().WithProcessFactory(value).Build();
                Assert.That(sidekick.ProcessFactory, Is.SameAs(value));
            }

            [Test]
            public void Should_build_with_default_value()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.ProcessFactory, Is.TypeOf<DaprProcessFactory>());
            }
        }

        public class WithSidecarInterceptor
        {
            [Test]
            public void Should_throw_exception_when_null_value()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentNullException>().With.Message.Contains("value"),
                    () => new DaprSidekickBuilder().WithSidecarInterceptor(null));
            }

            [Test]
            public void Should_build_with_specific_value()
            {
                var value1 = Substitute.For<IDaprSidecarProcessInterceptor>();
                var value2 = Substitute.For<IDaprSidecarProcessInterceptor>();
                var sidekick = new DaprSidekickBuilder().WithSidecarInterceptor(value1).WithSidecarInterceptor(value2).Build();
                Assert.That(sidekick.SidecarInterceptors.Count(), Is.EqualTo(2));
                Assert.That(sidekick.SidecarInterceptors, Does.Contain(value1));
                Assert.That(sidekick.SidecarInterceptors, Does.Contain(value2));
            }

            [Test]
            public void Should_build_with_default_value()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.SidecarInterceptors, Is.Null);
            }
        }

        public class Build
        {
            [Test]
            public void Builds_hosts()
            {
                var sidekick = new DaprSidekickBuilder().Build();
                Assert.That(sidekick.Placement, Is.Not.Null);
                Assert.That(sidekick.Sentry, Is.Not.Null);
                Assert.That(sidekick.Sidecar, Is.Not.Null);
            }
        }
    }
}
