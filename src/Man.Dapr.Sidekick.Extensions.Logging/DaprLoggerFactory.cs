using System;
using Man.Dapr.Sidekick.Logging;
using Microsoft.Extensions.Logging;

namespace Man.Dapr.Sidekick.Extensions.Logging
{
    /// <summary>
    /// A <see cref="IDaprLoggerFactory"/> that delegates logging calls to an underlying <see cref="ILoggerFactory"/>.
    /// </summary>
    public class DaprLoggerFactory : DaprLoggerFactoryBase
    {
        private readonly ILoggerFactory _loggerFactory;

        public DaprLoggerFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override IDaprLogger CreateLoggerImpl(string categoryName) => new DaprLogger(_loggerFactory.CreateLogger(categoryName));
    }
}
