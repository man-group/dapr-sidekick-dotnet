using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Http
{
    public class HttpContextInvocationHandlerTests
    {
        public class OnBeforeSendAsync
        {
            [Test]
            public void Should_not_add_authorization_header()
            {
                var httpRequest = new MockHttpRequest();
                var httpContext = new MockHttpContext(httpRequest);
                var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                httpContextAccessor.HttpContext.Returns(httpContext);
                var handler = new MockHttpContextInvocationHandler(httpContextAccessor);

                var request = new HttpRequestMessage();
                var cancellationToken = CancellationToken.None;

                handler.InvokeBeforeSendAsync(request, cancellationToken);
                Assert.That(request.Headers.Count(), Is.Zero);
            }

            [Test]
            public void Should_add_authorization_header()
            {
                var httpRequest = new MockHttpRequest();
                var httpContext = new MockHttpContext(httpRequest);
                var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                httpContextAccessor.HttpContext.Returns(httpContext);
                var handler = new MockHttpContextInvocationHandler(httpContextAccessor);

                httpRequest.Headers.Add("Authorization", "AUTH1");
                var request = new HttpRequestMessage();
                var cancellationToken = CancellationToken.None;

                handler.InvokeBeforeSendAsync(request, cancellationToken);
                Assert.That(request.Headers.Count(), Is.EqualTo(1));
                Assert.That(request.Headers.Authorization.ToString(), Is.EqualTo("AUTH1"));
            }

            [Test]
            public void Should_throw_exception_when_multiple_authorization_header_values()
            {
                var httpRequest = new MockHttpRequest();
                var httpContext = new MockHttpContext(httpRequest);
                var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                httpContextAccessor.HttpContext.Returns(httpContext);
                var handler = new MockHttpContextInvocationHandler(httpContextAccessor);

                var values = new StringValues(new[] { "AUTH1", "AUTH2" });
                httpRequest.Headers.Add("Authorization", values);
                var request = new HttpRequestMessage();
                var cancellationToken = CancellationToken.None;

                Assert.Throws(
                    Is.InstanceOf<FormatException>().With.Message.EqualTo("Cannot add value because header 'Authorization' does not support multiple values."),
                    () => handler.InvokeBeforeSendAsync(request, cancellationToken));
            }

            public class OnAfterSendAsync
            {
                [Test]
                public void Should_not_remove_authorization_header()
                {
                    var httpRequest = new MockHttpRequest();
                    var httpContext = new MockHttpContext(httpRequest);
                    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                    httpContextAccessor.HttpContext.Returns(httpContext);
                    var handler = new MockHttpContextInvocationHandler(httpContextAccessor);

                    var request = new HttpRequestMessage();
                    request.Headers.Add("Authorization",  "AUTH1");
                    var cancellationToken = CancellationToken.None;

                    handler.InvokeAfterSendAsync(request, cancellationToken);
                    Assert.That(request.Headers.Count(), Is.EqualTo(1));
                }

                [Test]
                public void Should_remove_authorization_header()
                {
                    var httpRequest = new MockHttpRequest();
                    var httpContext = new MockHttpContext(httpRequest);
                    var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
                    httpContextAccessor.HttpContext.Returns(httpContext);
                    var handler = new MockHttpContextInvocationHandler(httpContextAccessor);

                    httpRequest.Headers.Add("Authorization", "AUTH1");
                    var request = new HttpRequestMessage();
                    request.Headers.Add("Authorization", "AUTH1");
                    var cancellationToken = CancellationToken.None;

                    handler.InvokeAfterSendAsync(request, cancellationToken);
                    Assert.That(request.Headers.Count(), Is.Zero);
                }
            }
        }
    }
}
