using System;

namespace Man.Dapr.Sidekick.Logging.Internal
{
    internal class DaprColoredConsoleLogger : IDaprLogger
    {
        private readonly string _name;
        private readonly DaprColoredConsoleLoggerOptions _options;
        private readonly ISystemConsole _systemConsole;

        public DaprColoredConsoleLogger(string categoryName)
            : this(categoryName, new DaprColoredConsoleLoggerOptions(), new SystemConsole())
        {
        }

        public DaprColoredConsoleLogger(string categoryName, DaprColoredConsoleLoggerOptions options)
            : this(categoryName, options, new SystemConsole())
        {
        }

        public DaprColoredConsoleLogger(string categoryName, DaprColoredConsoleLoggerOptions options, ISystemConsole systemConsole)
        {
            _name = categoryName;
            _options = options;
            _systemConsole = systemConsole;
        }

        public IDisposable BeginScope<TState>(TState state) => new DaprDisposable();

        public bool IsEnabled(DaprLogLevel logLevel) => _options[logLevel].Enabled;

        public void Log<TState>(DaprLogLevel logLevel, DaprEventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var options = _options[logLevel];
            if (!options.Enabled)
            {
                return;
            }

            var level = GetShortLogLevel(logLevel);
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var color = _systemConsole.ForegroundColor;
            _systemConsole.ForegroundColor = options.Color;
            _systemConsole.WriteLine($"{timestamp} [{level}] [{_name}] {formatter(state, exception)}");
            _systemConsole.ForegroundColor = color;
        }

        private string GetShortLogLevel(DaprLogLevel logLevel) =>
            logLevel switch
            {
                DaprLogLevel.Critical => "CRIT",
                DaprLogLevel.Debug => "DBG",
                DaprLogLevel.Error => "ERR",
                DaprLogLevel.Trace => "TRC",
                DaprLogLevel.Warning => "WRN",
                _ => "INF",
            };
    }
}
