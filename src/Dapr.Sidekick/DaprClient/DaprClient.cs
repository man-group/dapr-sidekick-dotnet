#if !NET35
// Original source : https://github.com/dapr/dotnet-sdk/blob/master/src/Dapr.Client/DaprClient.cs
// See DAPR_DOTNET_SDK_LICENSE in this directory for license information.

// IMPORTANT : This class has been modified to allow injection of the createInvocationHandler() method.

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Dapr.Sidekick.DaprClient
{
    internal class DaprClient
    {
        /// <summary>
        /// <para>
        /// Creates an <see cref="HttpClient" /> that can be used to perform Dapr service
        /// invocation using <see cref="HttpRequestMessage" /> objects.
        /// </para>
        /// <para>
        /// The client will read the <see cref="HttpRequestMessage.RequestUri" /> property, and 
        /// interpret the hostname as the destination <c>app-id</c>. The <see cref="HttpRequestMessage.RequestUri" /> 
        /// property will be replaced with a new URI with the authority section replaced by <paramref name="daprEndpoint" />
        /// and the path portion of the URI rewitten to follow the format of a Dapr service invocation request.
        /// </para>
        /// </summary>
        /// <param name="appId">
        /// An optional <c>app-id</c>. If specified, the <c>app-id</c> will be configured as the value of 
        /// <see cref="HttpClient.BaseAddress" /> so that relative URIs can be used.
        /// </param>
        /// <param name="daprEndpoint">The HTTP endpoint of the Dapr process to use for service invocation calls.</param>
        /// <param name="daprApiToken">The token to be added to all request headers to Dapr runtime.</param>
        /// <param name="createInvocationHandler">An optional method for externally creating the <see cref="InvocationHandler"/> instance.</param>
        /// <param name="createInnerHandler">An optional method for externally creating the inner handler for the <see cref="InvocationHandler"/>. If not specified a <see cref="HttpClientHandler"/> instance will be used.</param>
        /// <returns>An <see cref="HttpClient" /> that can be used to perform service invocation requests.</returns>
        /// <remarks>
        /// <para>
        /// The <see cref="HttpClient" /> object is intended to be a long-lived and holds access to networking resources.
        /// Since the value of <paramref name="daprEndpoint" /> will not change during the lifespan of the application,
        /// a single client object can be reused for the life of the application.
        /// </para>
        /// </remarks>
        public static HttpClient CreateInvokeHttpClient(
            string appId = null,
            string daprEndpoint = null,
            string daprApiToken = null,
            Func<InvocationHandler> createInvocationHandler = null,
            Func<HttpMessageHandler> createInnerHandler = null)
        {
            var handler = createInvocationHandler?.Invoke() ?? new InvocationHandler();
            handler.InnerHandler ??= createInnerHandler?.Invoke() ?? new HttpClientHandler();
            handler.DaprApiToken ??= daprApiToken;

            if (daprEndpoint is string)
            {
                // DaprEndpoint performs validation.
                handler.DaprEndpoint = daprEndpoint;
            }

            var httpClient = new HttpClient(handler);

            if (appId is string)
            {
                try
                {
                    httpClient.BaseAddress = new Uri($"http://{appId}");
                }
                catch (UriFormatException inner)
                {
                    throw new ArgumentException("The appId must be a valid hostname.", nameof(appId), inner);
                }
            }

            return httpClient;
        }

        internal static KeyValuePair<string, string> GetDaprApiTokenHeader(string apiToken)
        {
            KeyValuePair<string, string> apiTokenHeader = default;
            if (!string.IsNullOrWhiteSpace(apiToken))
            {
                apiTokenHeader = new KeyValuePair<string, string>(DaprConstants.DaprApiTokenHeader, apiToken);
            }
            return apiTokenHeader;
        }
    }
}
#endif
