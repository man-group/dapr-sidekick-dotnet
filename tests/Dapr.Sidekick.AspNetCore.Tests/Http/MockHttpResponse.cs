﻿#pragma warning disable RCS1079 // Throwing of new NotImplementedException.
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dapr.Sidekick.Http
{
    internal class MockHttpResponse : HttpResponse
    {
        public override Stream Body { get; set; }

        public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ContentType { get; set; }

        public override IResponseCookies Cookies => throw new NotImplementedException();

        public override bool HasStarted => throw new NotImplementedException();

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override HttpContext HttpContext => throw new NotImplementedException();

        public override int StatusCode { get; set; }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }
    }
}
#pragma warning restore RCS1079 // Throwing of new NotImplementedException.
