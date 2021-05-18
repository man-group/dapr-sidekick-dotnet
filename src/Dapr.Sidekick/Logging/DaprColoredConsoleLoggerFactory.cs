using System;

namespace Dapr.Sidekick.Logging
{
    /// <summary>
    /// An implementation of <see cref="IDaprLoggerFactory"/> that logs all events to the system console using colors to differentiate log event levels.
    /// </summary>
    public class DaprColoredConsoleLoggerFactory : DaprLoggerFactoryBase
    {
        private readonly DaprColoredConsoleLoggerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprColoredConsoleLoggerFactory"/> class.
        /// </summary>
        public DaprColoredConsoleLoggerFactory()
            : this(new DaprColoredConsoleLoggerOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprColoredConsoleLoggerFactory"/> class
        /// with the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The console logger options.</param>
        public DaprColoredConsoleLoggerFactory(DaprColoredConsoleLoggerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        protected override IDaprLogger CreateLoggerImpl(string categoryName) => new Internal.DaprColoredConsoleLogger(categoryName, _options);
    }
}
