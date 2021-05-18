using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dapr.Sidekick.Logging;
using Newtonsoft.Json.Linq;

namespace Dapr.Sidekick.Process
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
                // Parse the log json text
                var daprLog = JObject.Parse(data);

                // Extract the message. If not available then continue.
                message = Convert.ToString(daprLog["msg"]);
                if (string.IsNullOrEmpty(message))
                {
                    return;
                }

                // Log Level
                var daprDaprLogLevel = Convert.ToString(daprLog["level"]);
                if (!string.IsNullOrEmpty(daprDaprLogLevel))
                {
                    logLevel = ToDaprLogLevel(daprDaprLogLevel);
                }

                // Extract the version
                var version = Convert.ToString(daprLog["ver"]);
                if (!string.IsNullOrEmpty(version))
                {
                    AddProperty(properties, "DaprVersion", version);
                    _processUpdater.UpdateVersion(version);
                }

                // Extract known additional properties
                AddProperty(properties, "DaprAppId", daprLog["app_id"]);
                AddProperty(properties, "DaprInstance", daprLog["instance"]);
                AddProperty(properties, "DaprScope", daprLog["scope"]);
                AddProperty(properties, "DaprTime", daprLog["time"]);
                AddProperty(properties, "DaprType", daprLog["type"]);
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

        private void AddProperty(Dictionary<string, object> values, string key, object value)
        {
            var text = Convert.ToString(value);
            if (!text.IsNullOrWhiteSpaceEx())
            {
                values.Add(key, text);
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
