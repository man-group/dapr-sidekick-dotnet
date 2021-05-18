using System;
using System.Collections.Generic;

namespace Dapr.Sidekick.Logging
{
    /// <summary>
    /// Options for the colored console logger.
    /// </summary>
    public class DaprColoredConsoleLoggerOptions
    {
        /// <summary>
        /// Options for a specific <see cref="DaprLogLevel"/>.
        /// </summary>
        public class LogLevelOptions
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LogLevelOptions"/> class
            /// to the specified <paramref name="logLevel"/>.
            /// </summary>
            /// <param name="logLevel">A <see cref="DaprLogLevel"/> instance.</param>
            public LogLevelOptions(DaprLogLevel logLevel)
                : this(logLevel, ConsoleColor.White)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="LogLevelOptions"/> class
            /// to the specified <paramref name="logLevel"/>.
            /// </summary>
            /// <param name="logLevel">A <see cref="DaprLogLevel"/> instance.</param>
            /// <param name="color">A <see cref="ConsoleColor"/> instance.</param>
            public LogLevelOptions(DaprLogLevel logLevel, ConsoleColor color)
            {
                LogLevel = logLevel;
                Color = color;
                Enabled = true;
            }

            /// <summary>
            /// Gets the log level.
            /// </summary>
            public DaprLogLevel LogLevel { get; }

            /// <summary>
            /// Gets or sets the console color.
            /// </summary>
            public ConsoleColor Color { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the log level is enabled.
            /// </summary>
            public bool Enabled { get; set; }
        }

        private readonly Dictionary<DaprLogLevel, LogLevelOptions> _logLevels = new Dictionary<DaprLogLevel, LogLevelOptions>();
        private readonly object _sync = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DaprColoredConsoleLoggerOptions"/> class.
        /// </summary>
        public DaprColoredConsoleLoggerOptions()
        {
            // Initialize defaults
            void AddValue(DaprLogLevel logLevel, ConsoleColor color)
            {
                var options = Get(logLevel);
                options.Color = color;
            }

            AddValue(DaprLogLevel.Information, ConsoleColor.Cyan);
            AddValue(DaprLogLevel.Trace, ConsoleColor.DarkGreen);
            AddValue(DaprLogLevel.Debug, ConsoleColor.Green);
            AddValue(DaprLogLevel.Warning, ConsoleColor.Yellow);
            AddValue(DaprLogLevel.Error, ConsoleColor.Red);
            AddValue(DaprLogLevel.Critical, ConsoleColor.DarkRed);
        }

        /// <summary>
        /// Gets the <see cref="LogLevelOptions"/> for the specified <paramref name="logLevel"/>.
        /// </summary>
        /// <param name="logLevel">A <see cref="DaprLogLevel"/> instance.</param>
        /// <returns>A <see cref="LogLevelOptions"/> instance for the specified <paramref name="logLevel"/>.</returns>
        public LogLevelOptions Get(DaprLogLevel logLevel)
        {
            // Cannot use ConcurrentDictionary as need to support net35.
            // Standard check-lock-check approach
            if (!_logLevels.TryGetValue(logLevel, out var value))
            {
                lock (_sync)
                {
                    if (!_logLevels.TryGetValue(logLevel, out value))
                    {
                        value = new LogLevelOptions(logLevel);
                        _logLevels[logLevel] = value;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the <see cref="LogLevelOptions"/> for the specified <paramref name="logLevel"/>.
        /// </summary>
        /// <param name="logLevel">A <see cref="DaprLogLevel"/> instance.</param>
        /// <returns>A <see cref="LogLevelOptions"/> instance for the specified <paramref name="logLevel"/>.</returns>
        public LogLevelOptions this[DaprLogLevel logLevel] => Get(logLevel);
    }
}
