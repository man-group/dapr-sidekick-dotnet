using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Man.Dapr.Sidekick.Logging;

namespace Man.Dapr.Sidekick.Process
{
    internal class DaprProcessLogger
    {
        public const string DebugLevel = "debug";
        public const string InfoLevel = "info";
        public const string WarningLevel = "warning";
        public const string ErrorLevel = "error";
        public const string FatalLevel = "fatal";

        private readonly IDaprLogger _logger;
        private readonly IDaprProcessUpdater _processUpdater;

        internal static DaprLogLevel ToDaprLogLevel(string daprDaprLogLevel) =>
            daprDaprLogLevel switch
            {
                FatalLevel => DaprLogLevel.Critical,
                DebugLevel => DaprLogLevel.Debug,
                ErrorLevel => DaprLogLevel.Error,
                WarningLevel => DaprLogLevel.Warning,
                _ => DaprLogLevel.Information,
            };

        public DaprProcessLogger(IDaprLogger logger, IDaprProcessUpdater processUpdater)
        {
            _logger = logger;
            _processUpdater = processUpdater;
        }

        public void LogData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            var message = data;
            var properties = new Dictionary<string, object>();
            var logLevel = DaprLogLevel.Information;
            try
            {
#if NETCOREAPP
                // Parse the log json text using System.Text.Json
                var logRecord = System.Text.Json.JsonSerializer.Deserialize<DaprProcessLogRecord>(
                    data,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
#else
                // Parse the log json text using Newtonsoft Json
                var logRecord = Newtonsoft.Json.JsonConvert.DeserializeObject<DaprProcessLogRecord>(data);
#endif

                // Extract the message.
                if (!string.IsNullOrEmpty(logRecord.Msg))
                {
                    message = logRecord.Msg;
                }

                // Log Level
                if (!string.IsNullOrEmpty(logRecord.Level))
                {
                    logLevel = ToDaprLogLevel(logRecord.Level);
                }

                // Extract the version
                if (!string.IsNullOrEmpty(logRecord.Ver))
                {
                    AddProperty(properties, "DaprVersion", logRecord.Ver);
                    _processUpdater.UpdateVersion(logRecord.Ver);
                }

                // Extract known additional properties
                AddProperty(properties, "DaprAppId", logRecord.App_id);
                AddProperty(properties, "DaprInstance", logRecord.Instance);
                AddProperty(properties, "DaprScope", logRecord.Scope);
                AddProperty(properties, "DaprTime", logRecord.Time);
                AddProperty(properties, "DaprType", logRecord.Type);
            }
            catch
            {
                // Cannot parse, will just be stored as a string
            }

            // Log it out
            if (properties.Count > 0)
            {
                // Include additional properties as a scope.
                using (_logger.BeginScope(properties))
                {
                    _logger.Log(logLevel, message);
                }
            }
            else
            {
                _logger.Log(logLevel, message);
            }

            // Now interpret the message
            if (_processUpdater != null)
            {
                InterpretMessage(message);
            }
        }

        private void AddProperty(Dictionary<string, object> values, string key, string value)
        {
            if (!value.IsNullOrWhiteSpaceEx())
            {
                values.Add(key, value);
            }
        }

        private void InterpretMessage(string message)
        {
            if (Regex.Match(message, "([Dd]apr).+([Ii]nitialized).+([Rr]unning)").Success)
            {
                // Dapr Sidecar
                // "dapr initialized. Status: Running. Init Elapsed 243.003ms"
                _processUpdater.UpdateStatus(DaprProcessStatus.Started);
            }
            else if (Regex.Match(message, "([Pp]lacement).+([Ss]tarted).+([Pp]ort)").Success)
            {
                // Dapr Placement
                // "placement service started on port 50005"
                _processUpdater.UpdateStatus(DaprProcessStatus.Started);
            }
            else if (Regex.Match(message, "([Ss]entry).+([Rr]unning).+([Pp]rotecting)").Success)
            {
                // Dapr Sentry
                // "sentry certificate authority is running, protecting ya'll"
                // Removed in v1.12 by this commit:
                // https://github.com/dapr/dapr/commit/c5857298afb76a8391af661c60f871f619f5e802#diff-3632294a2f84c023f9eb91f36196518238d6c2c1497167573b10ea986f128203L106
                _processUpdater.UpdateStatus(DaprProcessStatus.Started);
            }
            else if (Regex.Match(message, "([Rr]unning [Gg][Rr][Pp][Cc]) server").Success)
            {
                // Dapr Sentry
                // "running grpc server on port 50001"
                // Added in v1.12 by this commit:
                // https://github.com/dapr/dapr/commit/c5857298afb76a8391af661c60f871f619f5e802#diff-5a930421edf435ae1ca5033ee89f375cd66781a7fd90f7c99db494242eda0eb1R83
                _processUpdater.UpdateStatus(DaprProcessStatus.Started);
            }
            else if (Regex.Match(message, "([Ss]top).+([Ss]hutting).+([Dd]own)").Success)
            {
                // Dapr Placement
                // "stop command issued. Shutting down all operations"
                _processUpdater.UpdateStatus(DaprProcessStatus.Stopping);
            }
        }
    }
}
