﻿using System;

namespace Dapr.Sidekick.Http
{
    public partial class DaprSidecarHttpClientFactory : DaprDisposable, IDaprProcessHttpClientFactory, IDaprSidecarHttpClientFactory
    {
        private const string DefaultDaprHttpPort = "3500";

        internal static string GetDefaultDaprEndpoint()
        {
            var port = Environment.GetEnvironmentVariable(DaprConstants.DaprHttpPortEnvironmentVariable) ?? DefaultDaprHttpPort;

            // Since we're dealing with environment variables, treat empty the same as null.
            port = port?.Length == 0 ? DefaultDaprHttpPort : port;
            return $"http://127.0.0.1:{port}";
        }
    }
}
