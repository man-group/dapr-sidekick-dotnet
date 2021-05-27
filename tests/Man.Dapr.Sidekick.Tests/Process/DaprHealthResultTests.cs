using System.Net;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.Process
{
    public class DaprHealthResultTests
    {
        public class Unknown
        {
            [Test]
            public void Should_return_new_instance_each_call()
            {
                Assert.That(DaprHealthResult.Unknown, Is.Not.SameAs(DaprHealthResult.Unknown));
            }
        }

        public class IsHealthy
        {
            [TestCase(HttpStatusCode.OK, true)]
            [TestCase(HttpStatusCode.Created, true)]
            [TestCase(HttpStatusCode.NoContent, true)]
            [TestCase(HttpStatusCode.BadRequest, false)]
            [TestCase(HttpStatusCode.InternalServerError, false)]
            [TestCase(HttpStatusCode.PreconditionFailed, false)]
            [TestCase(HttpStatusCode.NotFound, false)]
            [TestCase(HttpStatusCode.Unauthorized, false)]
            public void Should_return_expected_value(HttpStatusCode statusCode, bool expected)
            {
                Assert.That(new DaprHealthResult(statusCode).IsHealthy, Is.EqualTo(expected));
            }
        }

        public class ToStringMethod
        {
            [Test]
            public void Should_return_healthy()
            {
                Assert.That(new DaprHealthResult(HttpStatusCode.OK).ToString(), Is.EqualTo("Healthy"));
            }

            [Test]
            public void Should_return_unhealthy_without_description()
            {
                Assert.That(new DaprHealthResult(HttpStatusCode.BadRequest).ToString(), Is.EqualTo("Unhealthy (BadRequest)"));
            }

            [Test]
            public void Should_return_unhealthy_with_description()
            {
                Assert.That(new DaprHealthResult(HttpStatusCode.InternalServerError, "DESCRIPTION").ToString(), Is.EqualTo("Unhealthy (InternalServerError, DESCRIPTION)"));
            }
        }
    }
}
