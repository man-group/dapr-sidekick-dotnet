using System;
using System.Collections.Generic;
using Dapr.Sidekick.Http;
using Dapr.Sidekick.Logging;
using Dapr.Sidekick.Process;
using Dapr.Sidekick.Security;

namespace Dapr.Sidekick
{
    public class DaprSidekickBuilder
    {
        private readonly List<IDaprSidecarProcessInterceptor> _sidecarInterceptors = new List<IDaprSidecarProcessInterceptor>();
        private IDaprApiTokenManager _apiTokenManager = null;
        private IDaprSidecarHttpClientFactory _httpClientFactory = null;
        private IDaprLoggerFactory _loggerFactory = null;
        private IDaprProcessFactory _processFactory = null;

        public DaprSidekickBuilder WithApiTokenManager(IDaprApiTokenManager value)
        {
            _apiTokenManager = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        public DaprSidekickBuilder WithHttpClientFactory(IDaprSidecarHttpClientFactory value)
        {
            _httpClientFactory = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        public DaprSidekickBuilder WithLoggerFactory(IDaprLoggerFactory value)
        {
            _loggerFactory = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        public DaprSidekickBuilder WithProcessFactory(IDaprProcessFactory value)
        {
            _processFactory = value ?? throw new ArgumentNullException(nameof(value));
            return this;
        }

        public DaprSidekickBuilder WithSidecarInterceptor(IDaprSidecarProcessInterceptor value)
        {
            _sidecarInterceptors.Add(value ?? throw new ArgumentNullException(nameof(value)));
            return this;
        }

        public DaprSidekick Build()
        {
            var apiTokenManager = _apiTokenManager ?? new DaprApiTokenManager(new RandomStringApiTokenProvider());
            return new DaprSidekick
            {
                ApiTokenManager = apiTokenManager,
                LoggerFactory = _loggerFactory ?? new DaprColoredConsoleLoggerFactory(),
                ProcessFactory = _processFactory ?? new DaprProcessFactory(),
                SidecarInterceptors = _sidecarInterceptors.Count > 0 ? _sidecarInterceptors.ToArray() : null,
                HttpClientFactory = _httpClientFactory ?? new DaprSidecarHttpClientFactory(apiTokenManager)
            };
        }
    }
}
