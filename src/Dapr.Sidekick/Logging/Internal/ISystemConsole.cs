using System;

namespace Dapr.Sidekick.Logging.Internal
{
    public interface ISystemConsole
    {
        ConsoleColor ForegroundColor { get; set; }

        void WriteLine(string value);
    }
}
