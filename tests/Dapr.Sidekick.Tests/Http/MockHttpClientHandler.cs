#if !NET35
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Dapr.Sidekick.Http
{
    public class MockHttpClientHandler : HttpClientHandler
    {
        public HttpStatusCode ResponseCode { get; set; } = HttpStatusCode.NoContent;

        public bool SendAsyncInvoked => SendAsyncRequest != null;

        public HttpRequestMessage SendAsyncRequest { get; private set; }

        public Action<HttpRequestMessage> OnSendAsyncRequest { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SendAsyncRequest = request;
            OnSendAsyncRequest?.Invoke(request);
            return Task.FromResult(new HttpResponseMessage(ResponseCode));
        }
    }
}
#endif
