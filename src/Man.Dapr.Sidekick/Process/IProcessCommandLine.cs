using System.Collections.Generic;

namespace Man.Dapr.Sidekick.Process
{
    /// <summary>
    /// Represents the command line used to start a process.
    /// </summary>
    public interface IProcessCommandLine
    {
        /// <summary>
        /// Gets the process that was started with this command line.
        /// </summary>
        IProcess Process { get; }

        /// <summary>
        /// Gets the full command-line text.
        /// </summary>
        string CommandLine { get; }

        /// <summary>
        /// Gets all command-line arguments as a dictionary of name-value pairs separated by the specified <paramref name="separator"/>.
        /// </summary>
        /// <param name="separator">The argument separator or prefix.</param>
        /// <returns>A dictionary of command-line arguments.</returns>
        IDictionary<string, string> GetArgumentsAsDictionary(char separator);
    }
}
