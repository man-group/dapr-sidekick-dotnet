#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Dapr.Sidekick.Http
{
    internal class MockHttpContext : HttpContext
    {
        private readonly HttpRequest _request;
        private readonly HttpResponse _response;

        public MockHttpContext(MockHttpRequest request = null, MockHttpResponse response = null)
        {
            _request = request;
            _response = response;
        }

        public override ConnectionInfo Connection => throw new NotImplementedException();

        public override IFeatureCollection Features => throw new NotImplementedException();

        public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override HttpRequest Request => _request;

        public override CancellationToken RequestAborted { get; set; } = CancellationToken.None;

        public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override HttpResponse Response => _response;

        public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override WebSocketManager WebSockets => throw new NotImplementedException();

        public override void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
