#if !NET35
using System;
using NUnit.Framework;

namespace Man.Dapr.Sidekick.DaprClient
{
    public class DaprClientTests
    {
        public class CreateInvokeHttpClient
        {
            [Test]
            public void Should_create_client()
            {
                var client = DaprClient.CreateInvokeHttpClient(
                    "APP_ID",
                    "http://127.0.0.1:1234",
                    "API_TOKEN");

                Assert.That(client.BaseAddress.ToString(), Is.EqualTo("http://app_id/"));   
            }

            [Test]
            public void Should_throw_exception_when_invalid_appid()
            {
                Assert.Throws(
                    Is.InstanceOf<ArgumentException>().With.Message.StartsWith("The appId must be a valid hostname"),
                    () => DaprClient.CreateInvokeHttpClient("!£$%^&*()_"));
            }
        }
    }
}
#endif
