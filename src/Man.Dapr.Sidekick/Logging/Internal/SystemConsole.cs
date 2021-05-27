using System;

namespace Man.Dapr.Sidekick.Logging.Internal
{
    internal class SystemConsole : ISystemConsole
    {
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }

        public void WriteLine(string value) => Console.WriteLine(value);
    }
}
