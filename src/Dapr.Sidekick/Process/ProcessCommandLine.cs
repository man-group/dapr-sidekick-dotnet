using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapr.Sidekick.Process
{
    public class ProcessCommandLine : IProcessCommandLine
    {
        internal ProcessCommandLine(SystemProcess systemProcess)
            : this(
                  systemProcess,
                  Native.NativeProcess.GetCommandLine(systemProcess.Process, out var commandLine) == 0 ? commandLine : null,
                  Native.NativeProcess.CommandLineToArgs(commandLine))
        {
        }

        // For testing
        internal ProcessCommandLine(IProcess process, string commandLine, IEnumerable<string> arguments)
        {
            Process = process;
            CommandLine = commandLine;
            Arguments = arguments;
        }

        public IProcess Process { get; }

        public string CommandLine { get; }

        public IEnumerable<string> Arguments { get; }

        public IDictionary<string, string> GetArgumentsAsDictionary(char separator = '-')
        {
            var result = new Dictionary<string, string>();
            if (Arguments?.Any() != true)
            {
                return result;
            }

            string currentName = null;
            var index = 0;
            var args = Arguments.Where(x => !x.IsNullOrWhiteSpaceEx()).ToArray();
            while (index < args.Length)
            {
                var arg = args[index];
                if (arg[0] == separator)
                {
                    if (currentName != null)
                    {
                        // Name but no value
                        result[currentName] = null;
                    }

                    // Remove all occurrences of separator, including multiples
                    currentName = arg.TrimStart(separator);
                }
                else if (currentName != null)
                {
                    // Name with value
                    result[currentName] = arg;
                    currentName = null;
                }

                index++;
            }

            return result;
        }

        public override string ToString() => CommandLine ?? Process.Name;
    }
}
