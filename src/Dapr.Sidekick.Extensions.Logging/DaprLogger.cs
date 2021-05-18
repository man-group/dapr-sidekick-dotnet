using System;
using Dapr.Sidekick.Logging;
using Microsoft.Extensions.Logging;

namespace Dapr.Sidekick.Extensions.Logging
{
    /// <summary>
    /// A <see cref="IDaprLogger"/> that delegates calls to an underling <see cref="ILogger"/> instance.
    /// </summary>
    public class DaprLogger : IDaprLogger
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprLogger"/> class using the underlying <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The underlying <see cref="ILogger"/> instance to which logger calls will be delegated.</param>
        public DaprLogger(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

        public bool IsEnabled(DaprLogLevel logLevel) => _logger.IsEnabled(ToLogLevel(logLevel));

        public void Log<TState>(DaprLogLevel logLevel, DaprEventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(ToLogLevel(logLevel), ToEventId(eventId), state, exception, formatter);

        private LogLevel ToLogLevel(DaprLogLevel logLevel) => (LogLevel)logLevel;

        private EventId ToEventId(DaprEventId eventId) => new EventId(eventId.Id, eventId.Name);
    }
}
