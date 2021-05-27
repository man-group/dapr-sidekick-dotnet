#if !NET35
// Original source : https://github.com/dapr/dotnet-sdk/blob/master/src/Dapr.Client/InvocationHandler.cs
// See DAPR_DOTNET_SDK_LICENSE in this directory for license information.

// IMPORTANT : This class has been modified to add the OnBeforeSendAsync() and OnAfterSendAsync() virtual methods.

// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Man.Dapr.Sidekick.DaprClient
{
    /// <summary>
    /// <para>
    /// A <see cref="DelegatingHandler" /> implementation that rewrites URIs of outgoing requests
    /// to use the Dapr service invocation protocol. This handle allows code using <see cref="HttpClient" />
    /// to use the client as-if it were communciating with the destination application directly.
    /// </para>
    /// <para>
    /// The handler will read the <see cref="HttpRequestMessage.RequestUri" /> property, and 
    /// interpret the hostname as the destination <c>app-id</c>. The <see cref="HttpRequestMessage.RequestUri" /> 
    /// property will be replaced with a new URI with the authority section replaced by <see cref="DaprEndpoint" />
    /// and the path portion of the URI rewitten to follow the format of a Dapr service invocation request.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This message handler does not distinguish between requests destined for Dapr service invocation and general
    /// HTTP traffic, and will attempt to forward all traffic to the Dapr endpoint. Do not attempt to set
    /// <see cref="DaprEndpoint" /> to a publicly routable URI, this will result in leaking of traffic and the Dapr
    /// security token.
    /// </remarks>
    public class InvocationHandler : DelegatingHandler
    {
        private Uri parsedEndpoint;
        private string? apiToken;

        /// <summary>
        /// Initializes a new instance of <see cref="InvocationHandler" />.
        /// </summary>
        public InvocationHandler()
        {
            this.parsedEndpoint = new Uri(Http.DaprSidecarHttpClientFactory.GetDefaultDaprEndpoint(), UriKind.Absolute);
            if (string.IsNullOrWhiteSpace(this.apiToken))
            {
                this.apiToken = Environment.GetEnvironmentVariable(DaprConstants.DaprApiTokenEnvironmentVariable);
            }
        }

        /// <summary>
        /// Gets or the sets the URI of the Dapr HTTP endpoint used for service invocation.
        /// </summary>
        /// <returns>The URI of the Dapr HTTP endpoint used for service invocation.</returns>
        public string DaprEndpoint
        {
            get
            {
                return this.parsedEndpoint.OriginalString;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                // This will throw a reasonable exception if the URI is invalid.
                var uri = new Uri(value, UriKind.Absolute);
                if (uri.Scheme != "http" && uri.Scheme != "https")
                {
                    throw new ArgumentException("The URI scheme of the Dapr endpoint must be http or https.", "value");
                }

                this.parsedEndpoint = uri;
            }
        }

        // Internal for testing
        internal string? DaprApiToken
        {
            get => this.apiToken;
            set => this.apiToken = value;
        }

        // Internal for testing
        internal HttpResponseMessage Send(HttpRequestMessage request) => SendAsync(request, CancellationToken.None).Result;

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var original = request.RequestUri;
            if (!this.TryRewriteUri(original, out var rewritten))
            {
                throw new ArgumentException($"The request URI '{original}' is not a valid Dapr service invocation destination.", nameof(request));
            }

            var apiTokenHeader = DaprClient.GetDaprApiTokenHeader(this.apiToken);
            var isValidToken = apiTokenHeader.Key != null && apiTokenHeader.Value != null;

            try
            {
                if (isValidToken)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    request.Headers.Add(apiTokenHeader.Key, apiTokenHeader.Value);
#pragma warning restore CS8604 // Possible null reference argument.
                }
                request.RequestUri = rewritten;

                OnBeforeSendAsync(request, cancellationToken);
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                OnAfterSendAsync(request, cancellationToken);
                request.RequestUri = original;
                if (isValidToken)
                {
                    request.Headers.Remove(DaprConstants.DaprApiTokenHeader);
                }
            }
        }

        protected virtual void OnBeforeSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
        }

        protected virtual void OnAfterSendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
        }

        // Internal for testing
        internal bool TryRewriteUri(Uri? uri, out Uri? rewritten)
        {
            // For now the only invalid cases are when the request URI is missing or just silly.
            // We may support additional cases for validation in the future (like an allow-list of App-Ids).
            if (uri is null || !uri.IsAbsoluteUri || (uri.Scheme != "http" && uri.Scheme != "https"))
            {
                // do nothing
                rewritten = null;
                return false;
            }


            var builder = new UriBuilder(uri)
            {
                Scheme = this.parsedEndpoint.Scheme,
                Host = this.parsedEndpoint.Host,
                Port = this.parsedEndpoint.Port,
                Path = $"/v1.0/invoke/{uri.Host}/method" + uri.AbsolutePath,
            };

            rewritten = builder.Uri;
            return true;
        }
    }
}
#endif
