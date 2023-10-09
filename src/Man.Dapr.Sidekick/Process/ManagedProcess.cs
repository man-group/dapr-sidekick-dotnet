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
        /// <param name="options">Options used by the process.</param>
        public void Start(DaprManagedProcessOptions options)
        {
            lock (_processLock)
            {
                if (IsRunning)
                {
                    return;
                }

                // If filename does not exist, cannot start
                if (string.IsNullOrEmpty(options.Filename) || !File.Exists(options.Filename))
                {
                    throw new InvalidOperationException($"Unable to start process, file '{options.Filename}' does not exist");
                }

                if (!string.IsNullOrEmpty(options.WorkingDirectory) && !Directory.Exists(options.WorkingDirectory))
                {
                    throw new InvalidOperationException(
                        $"Unable to start process, working directory '{options.WorkingDirectory}' does not exist");
                }

                try
                {
                    // Initialize the process start info
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = options.Filename,
                        Arguments = options?.Arguments,
                        UseShellExecute = false,
                        CreateNoWindow =
                            true, // Ensures CTRL-C and keystrokes in Console mode is intercepted only by hosting window
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        WorkingDirectory = options.WorkingDirectory ?? Path.GetDirectoryName(options.Filename)
                    };

                    // Add environment variables
                    options?.ConfigureEnvironmentVariables?.Invoke(processStartInfo.EnvironmentVariables);

                    // Last chance for cancellation
                    if (options.CancellationToken.IsCancellationRequested)
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
                            Stop(null, options.CancellationToken);
                            OnUnplannedExit();
                        }
                    };

                    // Store the logger and start the process.
                    _logger = options.Logger;
                    _logger?.LogInformation("Starting Process {ProcessFilename} with arguments '{ProcessArguments}'", options.Filename, options?.Arguments);
                    _systemProcess = CreateSystemProcess(process);
                    _systemProcess.Start();

                    // If the process has an ID then it really has started so hook it up
                    if (_systemProcess.Id > 0)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    options.Logger?.LogInformation("Process {ProcessName} PID:{ProcessId} started successfully", _systemProcess.Name, _systemProcess.Id);
                }
                catch (Exception ex)
                {
                    options.Logger?.LogError(ex, "Error starting process {ProcessFilename}", options.Filename);

                    // Do a full cleanup
                    Stop(null, options.CancellationToken);
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
            DaprCancellationToken cancellationToken = default) => Start(new DaprManagedProcessOptions
        {
            Filename = filename,
            Arguments = arguments,
            Logger = logger,
            ConfigureEnvironmentVariables = configureEnvironmentVariables,
            CancellationToken = cancellationToken
        });

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
