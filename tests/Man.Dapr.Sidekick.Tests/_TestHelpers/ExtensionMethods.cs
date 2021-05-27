using System.Linq;
using Man.Dapr.Sidekick.Logging;
using NSubstitute;

namespace Man.Dapr.Sidekick
{
    public static class ExtensionMethods
    {
        public static LoggerCall[] ReceivedLoggerCalls(this IDaprLogger logger)
        {
            return logger.ReceivedCalls().Select(x => new LoggerCall(x)).ToArray();
        }
    }
}
