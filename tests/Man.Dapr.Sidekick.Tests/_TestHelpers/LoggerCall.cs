using System;
using Man.Dapr.Sidekick.Logging;
using NSubstitute.Core;

namespace Man.Dapr.Sidekick
{
    /// <summary>
    /// Supporting class for capturing details about calls made to the Microsoft Logging framework.
    /// </summary>
    public class LoggerCall
    {
        public LoggerCall(ICall call)
        {
            var args = call.GetArguments();
            if (args.Length == 1)
            {
                // Scope message
                IsScope = true;
                Message = Convert.ToString(args[0]);
            }
            else
            {
                // Regular message
                LogLevel = (DaprLogLevel)args[0];
                Message = Convert.ToString(args[2]);
                Exception = args.Length > 3 ? (Exception)args[3] : null;
            }
        }

        public DaprLogLevel? LogLevel { get; }

        public string Message { get; }

        public bool IsScope { get; }

        public Exception Exception { get; }
    }
}
