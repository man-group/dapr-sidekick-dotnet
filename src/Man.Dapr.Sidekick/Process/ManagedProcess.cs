using System;
using System.Collections.Specialized;
using System.IO;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Options;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    /// <summary>
    /// Starts and manages a system process for a Dapr process binary.
    /// </summary>
    public class ManagedProcess : IProcess
    {
        private readonly object _processLock = new object();
        private SystemProcess _systemProcess;
        private IDaprLogger _logger;
        private bool _stopping;

        public ManagedProcess()
        {
            CreateSystemProcess = p => new SystemProcess(p, _logger);
        }

        public int? Id => _systemProcess?.Id;

        public string Name => _systemProcess?.Name;

        public bool IsRunning => _systemProcess?.IsRunning ?? false;

        /// <summary>
        /// Starts a system process for a Dapr process binary.
        /// </summary>
        /// <param name="filename">The full path to the system process.</param>
        /// <param name="managedProcessOptions">Options used by the process.</param>
        /// <param name="logger">An optional <see cref="IDaprLogger"/> instance for receiving stdout log messages from the process.</param>
        /// <param name="cancellationToken">A <see cref="DaprCancellationToken"/> for aborting the process startup operation.</param>
        public void Start(
            string filename,
            DaprManagedProcessOptions managedProcessOptions,
            IDaprLogger logger = null,
            DaprCancellationToken cancellationToken = default)
        {
            lock (_processLock)
            {
                if (IsRunning)
                {
                    return;
                }

                // If filename does not exist, cannot start
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    throw new InvalidOperationException($"Unable to start process, file '{filename}' does not exist");
                }

                if (managedProcessOptions != null
                    && !string.IsNullOrEmpty(managedProcessOptions.WorkingDirectory)
                    && !Directory.Exists(managedProcessOptions.WorkingDirectory))
                {
                    throw new InvalidOperationException(
                        $"Unable to start process, working directory '{managedProcessOptions.WorkingDirectory}' does not exist");
                }

                try
                {
                    // Initialize the process start info
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filename,
                        Arguments = managedProcessOptions?.Arguments,
                        UseShellExecute = false,
                        CreateNoWindow =
                            true, // Ensures CTRL-C and keystrokes in Console mode is intercepted only by hosting window
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        WorkingDirectory = managedProcessOptions?.WorkingDirectory ?? Path.GetDirectoryName(filename)
                    };

                    // Add environment variables
                    managedProcessOptions?.ConfigureEnvironmentVariables?.Invoke(processStartInfo.EnvironmentVariables);

                    // Last chance for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Initialize process
                    var process = new System.Diagnostics.Process
                    {
                        EnableRaisingEvents = true, StartInfo = processStartInfo
                    };
                    process.OutputDataReceived += (sender, args) => OnOutputDataReceived(args);
                    process.ErrorDataReceived += (sender, args) => OnOutputDataReceived(args);
                    process.Exited += (sender, args) =>
                    {
                        if (!_stopping && _systemProcess != null)
                        {
                            Stop(null, cancellationToken);
                            OnUnplannedExit();
                        }
                    };

                    // Store the logger and start the process.
                    _logger = logger;
                    _logger?.LogInformation("Starting Process {ProcessFilename} with arguments '{ProcessArguments}'", filename, managedProcessOptions?.Arguments);
                    _systemProcess = CreateSystemProcess(process);
                    _systemProcess.Start();

                    // If the process has an ID then it really has started so hook it up
                    if (_systemProcess.Id > 0)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    logger?.LogInformation("Process {ProcessName} PID:{ProcessId} started successfully", _systemProcess.Name, _systemProcess.Id);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error starting process {ProcessFilename}", filename);

                    // Do a full cleanup
                    Stop(null, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Starts a system process for a Dapr process binary.
        /// </summary>
        /// <param name="filename">The full path to the system process.</param>
        /// <param name="arguments">The optional command-line process arguments.</param>
        /// <param name="logger">An optional <see cref="IDaprLogger"/> instance for receiving stdout log messages from the process.</param>
        /// <param name="configureEnvironmentVariables">An optional action for configuring any environment variables for the process.</param>
        /// <param name="cancellationToken">A <see cref="DaprCancellationToken"/> for aborting the process startup operation.</param>
        public void Start(
            string filename,
            string arguments = null,
            IDaprLogger logger = null,
            Action<StringDictionary> configureEnvironmentVariables = null,
            DaprCancellationToken cancellationToken = default)
        {
            lock (_processLock)
            {
                if (IsRunning)
                {
                    return;
                }

                // If filename does not exist, cannot start
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    throw new InvalidOperationException($"Unable to start process, file '{filename}' does not exist");
                }

                try
                {
                    // Initialize the process start info
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = filename,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow =
                            true, // Ensures CTRL-C and keystrokes in Console mode is intercepted only by hosting window
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        WorkingDirectory = Path.GetDirectoryName(filename)
                    };

                    // Add environment variables
                    configureEnvironmentVariables?.Invoke(processStartInfo.EnvironmentVariables);

                    // Last chance for cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    // Initialize process
                    var process = new System.Diagnostics.Process
                    {
                        EnableRaisingEvents = true, StartInfo = processStartInfo
                    };
                    process.OutputDataReceived += (sender, args) => OnOutputDataReceived(args);
                    process.ErrorDataReceived += (sender, args) => OnOutputDataReceived(args);
                    process.Exited += (sender, args) =>
                    {
                        if (!_stopping && _systemProcess != null)
                        {
                            Stop(null, cancellationToken);
                            OnUnplannedExit();
                        }
                    };

                    // Store the logger and start the process.
                    _logger = logger;
                    _logger?.LogInformation("Starting Process {ProcessFilename} with arguments '{ProcessArguments}'", filename, arguments);
                    _systemProcess = CreateSystemProcess(process);
                    _systemProcess.Start();

                    // If the process has an ID then it really has started so hook it up
                    if (_systemProcess.Id > 0)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    logger?.LogInformation("Process {ProcessName} PID:{ProcessId} started successfully", _systemProcess.Name, _systemProcess.Id);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "Error starting process {ProcessFilename}", filename);

                    // Do a full cleanup
                    Stop(null, cancellationToken);
                }
            }
        }

        public void Stop(int? waitForShutdownSeconds = null, DaprCancellationToken cancellationToken = default)
        {
            lock (_processLock)
            {
                if (!_stopping)
                {
                    // Stop the process
                    _stopping = true;
                    _systemProcess?.Stop(waitForShutdownSeconds, cancellationToken);

                    // Clean up
                    _systemProcess = null;
                    _stopping = false;
                }
            }
        }

        public IProcessCommandLine GetCommandLine() => _systemProcess?.GetCommandLine();

        public event EventHandler<System.Diagnostics.DataReceivedEventArgs> OutputDataReceived;

        public event EventHandler UnplannedExit;

        protected virtual void OnOutputDataReceived(System.Diagnostics.DataReceivedEventArgs args) =>
            OutputDataReceived?.Invoke(this, args);

        protected virtual void OnUnplannedExit() => UnplannedExit?.Invoke(this, EventArgs.Empty);

        // For testing
        internal Func<System.Diagnostics.Process, SystemProcess> CreateSystemProcess { get; set; }
    }
}
