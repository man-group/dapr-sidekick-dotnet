using System;
using System.IO;
using System.Linq;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Security;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    internal abstract class DaprProcess<TOptions> : IDaprProcess<TOptions>, IDaprProcessUpdater
        where TOptions : Options.DaprProcessOptions, new()
    {
        private readonly object _lock = new object();
        private readonly string _defaultProcessName;
        private Func<DaprOptions> _startOptionsAccessor; // Returns latest startup options
        private TOptions _pendingOptions; // Options used to start process, stored temporarily until startup complete.
        private DaprProcessLogger _daprLogger;
        private System.Timers.Timer _restartTimer;
        private IProcess _underlyingProcess;

#if NET35
        private System.Threading.Thread _startupThread;
#else
        private System.Threading.Tasks.Task _startupTask;
#endif

        protected DaprProcess(string defaultProcessName)
        {
            _defaultProcessName = defaultProcessName;
        }

        public TOptions LastSuccessfulOptions { get; private set; }

        // For testing
        internal IProcessFinder ProcessFinder { get; set; } = new ProcessFinder();

        public DaprProcessInfo GetProcessInfo() => new DaprProcessInfo(Name, Id, Version, Status, _underlyingProcess is AttachedProcess);

        public event EventHandler<DaprProcessStartingEventArgs<TOptions>> Starting;

        public event EventHandler<DaprProcessStoppingEventArgs> Stopping;

        public bool Start(Func<DaprOptions> optionsAccessor, IDaprLogger logger, DaprCancellationToken cancellationToken = default)
        {
            // Pre-initialization validation
            lock (_lock)
            {
                // Make sure not already running.
                if (IsRunning)
                {
                    // Already running
                    logger?.LogWarning("Process {DaprProcessName} PID:{DaprProcessId} is already running", Name, Id);
                    return false;
                }
                else if (Status != DaprProcessStatus.Stopped)
                {
                    // Must be in Stopped status
                    logger?.LogWarning("Unable to start Dapr process {DaprProcessName} as it is currently in the following state: {DaprProcessStatus}", Name, Status);
                    return false;
                }
            }

            // Store the input values for restart purposes
            _startOptionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Enter the initialization loop
            BeginInitialize(cancellationToken);
            return true;
        }

        public void Stop(DaprCancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (IsRunning || (Status != DaprProcessStatus.Stopped && Status != DaprProcessStatus.Disabled))
                {
                    Logger?.LogInformation("Stopping Process {DaprProcessName} PID:{DaprProcessId}", Name, Id);
                    UpdateStatus(DaprProcessStatus.Stopping);

                    // Allow inheritor to cleanly shut down the process first
                    OnStopping(cancellationToken);

                    // Stop the process
                    _underlyingProcess?.Stop(LastSuccessfulOptions?.WaitForShutdownSeconds, cancellationToken);

                    UpdateStatus(DaprProcessStatus.Stopped);
                }

                // Unassign stored variables
                _restartTimer?.Stop();
                _daprLogger = null;
                _pendingOptions = default;
                _underlyingProcess = null;
            }
        }

        public bool Restart(DaprCancellationToken cancellationToken)
        {
            var startOptions = _startOptionsAccessor?.Invoke();
            if (startOptions == null || Logger == null)
            {
                throw new InvalidOperationException("Unable to attempt a restart. Process has not been previously started successfully.");
            }

            // Stop the process
            Stop(cancellationToken);

            // Enter the initialization loop
            BeginInitialize(cancellationToken);
            return true;
        }

        protected IDaprLogger Logger { get; private set; }

        public void UpdateStatus(DaprProcessStatus status)
        {
            if (status != Status)
            {
                Logger?.LogInformation("Dapr Process Status Change: {DaprProcessStatusPrevious} -> {DaprProcessStatus}", Status, status);
                Status = status;

                if (status == DaprProcessStatus.Started)
                {
                    // Successfully started. Store the options to retain ports for any future restart.
                    LastSuccessfulOptions = _pendingOptions;
                    Logger?.LogInformation("Caching successful startup options for future restart attempts");
                }
            }
        }

        public void UpdateVersion(string version) => Version = version;

        protected abstract TOptions GetProcessOptions(DaprOptions daprOptions);

        protected abstract void AssignPorts(PortAssignmentBuilder<TOptions> builder);

        protected abstract void AssignLocations(TOptions options, string daprFolder);

        protected abstract void AddCommandLineArguments(TOptions source, CommandLineArgumentBuilder builder);

        protected abstract void AddEnvironmentVariables(TOptions source, EnvironmentVariableBuilder builder);

        protected abstract void ParseCommandLineArgument(TOptions target, string name, string value);

        /// <summary>
        /// Gets a value that determines the comparison of the proposed process options with those for an existing running process.
        /// For example it is not permitted to have more than one daprd.exe process for the same application id on the same machine,
        /// so an equivalence check could be to compare the app-id values. When two sets of options are considered
        /// equivalent a process for the proposed options will not be launched if a process for the existing options is running.
        /// </summary>
        /// <param name="proposedProcessOptions">The options representing the proposed process that is about to be launched.</param>
        /// <param name="existingProcessOptions">The options read from the existing running process.</param>
        /// <param name="existingProcess">Information about the existing process.</param>
        /// <returns>A <see cref="ProcessComparison"/> value representing the comparison result.</returns>
        protected abstract ProcessComparison CompareProcessOptions(TOptions proposedProcessOptions, TOptions existingProcessOptions, IProcess existingProcess);

        /// <summary>
        /// Writes default certificate files to the specified <paramref name="certsDirectory"/> if defined in the suppplied <paramref name="options"/>.
        /// </summary>
        /// <param name="certsDirectory">The directory in which the certificates file will be created.</param>
        /// <param name="options">The options defining the certificate file contents.</param>
        protected void WriteDefaultCertificates(string certsDirectory, TOptions options)
        {
            // Make sure each of the certificate files are present/defined
            void WriteCertFile(string name, string contents)
            {
                if (string.IsNullOrEmpty(contents))
                {
                    return;
                }

                if (!Directory.Exists(certsDirectory))
                {
                    Directory.CreateDirectory(certsDirectory);
                }

                File.WriteAllText(Path.Combine(certsDirectory, name), contents);
            }

            WriteCertFile(DaprConstants.TrustAnchorsCertificateFilename, options.TrustAnchorsCertificate);
            WriteCertFile(DaprConstants.IssuerCertificateFilename, options.IssuerCertificate);
            WriteCertFile(DaprConstants.IssuerKeyFilename, options.IssuerKey);
        }

        protected virtual void OnStarting(TOptions options) => Starting?.Invoke(this, new DaprProcessStartingEventArgs<TOptions>(options));

        protected virtual void OnStopping(DaprCancellationToken cancellationToken) => Stopping?.Invoke(this, new DaprProcessStoppingEventArgs(cancellationToken));

        protected virtual void SetEnvironmentVariable(string key, string value) => Environment.SetEnvironmentVariable(key, value);

#if NET35
        private void BeginInitialize(DaprCancellationToken cancellationToken)
        {
            _startupThread = new System.Threading.Thread(Initialize);
            _startupThread.Start(cancellationToken);
        }

        private void Initialize(object arg)
        {
            var cancellationToken = (DaprCancellationToken)arg;
#else
        private void BeginInitialize(DaprCancellationToken cancellationToken)
        {
            _startupTask = System.Threading.Tasks.Task.Run(() => Initialize(cancellationToken), cancellationToken.CancellationToken);
        }

        private void Initialize(DaprCancellationToken cancellationToken)
        {
#endif
            try
            {
                // Enter Initializing State
                UpdateStatus(DaprProcessStatus.Initializing);

                // Start with a new instance
                var proposedOptions = GetProcessOptions(_startOptionsAccessor?.Invoke() ?? new DaprOptions());
                proposedOptions.ProcessName ??= _defaultProcessName;

                // If not enabled then exit
                if (proposedOptions.Enabled == false)
                {
                    UpdateStatus(DaprProcessStatus.Disabled);
                    Logger.LogInformation("Dapr Sidekick is disabled for {DaprProcessName}, Dapr process will not be launched", proposedOptions.ProcessName);
                    return;
                }

                Logger.LogInformation("Dapr expected process name set to {DaprProcessName}", proposedOptions.ProcessName);

                // Check existing processes
                var attachableProcess = CheckExistingProcesses(proposedOptions);
                if (attachableProcess != null)
                {
                    // We can attach to an existing process
                    _underlyingProcess = AttachExistingProcess(attachableProcess);
                    return;
                }

                // Assign ports, retaining any from last used options if required.
                var portBuilder = new PortAssignmentBuilder<TOptions>();
                AssignPorts(portBuilder);
                portBuilder.Build(proposedOptions, LastSuccessfulOptions, Logger);

                // Initialize expected locations and directories
                InitializeDirectories(proposedOptions);

                // Check the process file exists and the Installer process was considered valid, otherwise this is a hard failure.
                if (!File.Exists(proposedOptions.ProcessFile))
                {
                    throw new FileNotFoundException("Dapr binary not found at expected runtime location.", proposedOptions.ProcessFile);
                }

                // Process file exists, assign all additional locations
                AssignLocations(proposedOptions, proposedOptions.RuntimeDirectory);

                // We want to make sure Daprd is restarted if it fails.
                // If a default value is not defined, set it to 5 seconds. If negative value defined no restart required.
                if (proposedOptions.RestartAfterMillseconds.HasValue && proposedOptions.RestartAfterMillseconds <= 0)
                {
                    Logger.LogWarning("Dapr process auto-restart disabled by RestartAfterMillseconds: {RestartAfterMillseconds}", proposedOptions.RestartAfterMillseconds);
                }
                else
                {
                    _restartTimer = new System.Timers.Timer
                    {
                        AutoReset = false,
                        Interval = proposedOptions.RestartAfterMillseconds ?? 5000
                    };
                    _restartTimer.Elapsed += (sender, args) => Restart(DaprCancellationToken.None);
                }

                // Set all starting options
                SetStarting(proposedOptions);

                // Store the options used to start the process, in a pending state until fully successful start.
                _pendingOptions = proposedOptions;

                // Create the logger handler for stdio events (we always use JSON logging)
                _daprLogger = new DaprProcessLogger(Logger, this);

                // Start the process
                _underlyingProcess = StartManagedProcess(proposedOptions, cancellationToken);
            }
            catch (Exception ex)
            {
                // Cleanup
                Stop(cancellationToken);

                // Restart if a timer is defined
                if (_restartTimer != null)
                {
                    Logger.LogError(ex, "Process {DaprdProcessName} failed to start. Restarting in {DaprdProcessRestartMilliseconds}ms...", Name, _restartTimer.Interval);
                    _restartTimer.Start();
                }
                else
                {
                    Logger.LogError(ex, "Process {DaprdProcessName} failed to start, a restart will not be attempted", Name);
                }
            }
        }

        private void SetStarting(TOptions options)
        {
            // Set the starting status
            UpdateStatus(DaprProcessStatus.Starting);

            // Allow inheritors to carry out any remaining actions and modify options
            OnStarting(options);

            // Apply environment variables
            var builder = new EnvironmentVariableBuilder();
            AddEnvironmentVariables(options, builder);

            // Add environment variable overrides
            if (options.EnvironmentVariables?.Any() == true)
            {
                foreach (var entry in options.EnvironmentVariables)
                {
                    builder.Add(entry.Key, entry.Value);
                }
            }

            // Apply the environment variables
            foreach (var entry in builder.ToDictionary())
            {
                // Set the variable
                SetEnvironmentVariable(entry.Key, Convert.ToString(entry.Value.SensitiveValue()));
                Logger.LogInformation("Environment variable {DaprEnvironmentVariableName} set to {DaprEnvironmentVariableValue}", entry.Key, entry.Value);
            }
        }

        private IProcess StartManagedProcess(TOptions proposedOptions, DaprCancellationToken cancellationToken)
        {
            // Get command-line arguments
            var builder = new CommandLineArgumentBuilder();
            AddCommandLineArguments(proposedOptions, builder);
            var arguments = builder.ToString();

            // Create the managed process wrapper
            var process = new ManagedProcess();
            process.OutputDataReceived += (sender, args) => _daprLogger?.LogData(args.Data);

            // Start the managed dapr process
            process.Start(proposedOptions.ProcessFile, arguments, Logger, cancellationToken: cancellationToken);

            // Handle unplanned exit
            process.UnplannedExit += (sender, args) =>
            {
                // Cleanup
                UpdateStatus(DaprProcessStatus.Stopped);
                Stop();

                // Restart if a timer is defined
                if (_restartTimer != null)
                {
                    Logger.LogError("Process {DaprProcessName} exited unexpectedly. Restarting in {DaprdProcessRestartMilliseconds}ms...", Name, _restartTimer.Interval);
                    _restartTimer.Start();
                }
                else
                {
                    Logger.LogError("Process {DaprProcessName} exited unexpectedly, a restart will not be attempted", Name);
                }
            };

            return process;
        }

        private IProcess AttachExistingProcess(AttachableProcess attachableProcess)
        {
            SetStarting(attachableProcess.Options);

            // Store the options used to start the process, in a pending state until fully successful start.
            _pendingOptions = attachableProcess.Options;

            // Attach to the process
            var process = new AttachedProcess(attachableProcess.Process);
            Logger.LogInformation("Attached to existing Dapr Process {DaprProcessName} PID:{DaprProcessId}", process.Name, process.Id);

            // Started!
            UpdateStatus(DaprProcessStatus.Started);

            return process;
        }

        private void InitializeDirectories(TOptions proposedOptions)
        {
            // Set the initial directory
            if (string.IsNullOrEmpty(proposedOptions.InitialDirectory))
            {
                // Not specified, set to the default "dapr init" location %USERPROFILE%/.dapr ($HOME/.dapr on Linux)
#if NET35
                var profilePath = Environment.GetEnvironmentVariable("USERPROFILE");
#else
                var profilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
#endif
                proposedOptions.InitialDirectory = Path.Combine(profilePath, ".dapr");
            }

            // Set the Runtime directory
            if (string.IsNullOrEmpty(proposedOptions.RuntimeDirectory))
            {
                proposedOptions.RuntimeDirectory = proposedOptions.InitialDirectory;
            }

            // Set the Bin Directory
            if (string.IsNullOrEmpty(proposedOptions.BinDirectory))
            {
                proposedOptions.BinDirectory = Path.Combine(proposedOptions.RuntimeDirectory, DaprConstants.DaprBinDirectory);
            }

            // If the path is rooted then return it else calculate the path relative to the working directory
            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string GetFullPath(string path) => Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(workingDirectory, path));

            // Normalize all paths
            proposedOptions.InitialDirectory = GetFullPath(proposedOptions.InitialDirectory);
            proposedOptions.RuntimeDirectory = GetFullPath(proposedOptions.RuntimeDirectory);
            proposedOptions.BinDirectory = GetFullPath(proposedOptions.BinDirectory);

            // Set the Process File name
            var exeName = DaprConstants.IsWindows ? proposedOptions.ProcessName + DaprConstants.ExeExtension : proposedOptions.ProcessName;
            var initialFile = Path.Combine(Path.Combine(proposedOptions.InitialDirectory, DaprConstants.DaprBinDirectory), exeName);
            if (string.IsNullOrEmpty(proposedOptions.ProcessFile))
            {
                proposedOptions.ProcessFile = Path.Combine(proposedOptions.BinDirectory, exeName);
            }

            // Set up everything
            try
            {
                if (!Directory.Exists(proposedOptions.RuntimeDirectory))
                {
                    // Create the target directory
                    Logger.LogInformation("Creating directory: {DaprRuntimeDirectory}", proposedOptions.RuntimeDirectory);
                    Directory.CreateDirectory(proposedOptions.RuntimeDirectory);
                }

                // Log out the expected locations
                Logger.LogInformation("Dapr initial directory: {DaprInitialDirectory}", proposedOptions.InitialDirectory);
                Logger.LogInformation("Dapr runtime directory: {DaprRuntimeDirectory}", proposedOptions.RuntimeDirectory);

                // Copy the process file if necessary
                var initialFileInfo = new FileInfo(initialFile);
                var runtimeFileInfo = new FileInfo(proposedOptions.ProcessFile);
                if (proposedOptions.CopyProcessFile == true && initialFileInfo.Exists)
                {
                    Logger.LogDebug("CopyProcessFile is set, process file will be copied from initial directory");

                    if (!Directory.Exists(proposedOptions.BinDirectory))
                    {
                        // Create the target directory
                        Logger.LogInformation("Creating directory: {DaprBinDirectory}", proposedOptions.BinDirectory);
                        Directory.CreateDirectory(proposedOptions.BinDirectory);
                    }

                    if (string.Equals(runtimeFileInfo.FullName, initialFileInfo.FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Path is the same as the runtime file, nothing to copy.
                        Logger.LogDebug("Not copying process file from {DaprProcessInitialFile} to {DaprProcessRuntimeFile} because files appear to be the same path", initialFile, proposedOptions.ProcessFile);
                    }
                    else if (
                        runtimeFileInfo.Exists &&
                        initialFileInfo.LastWriteTime == runtimeFileInfo.LastWriteTime &&
                        initialFileInfo.Length == runtimeFileInfo.Length)
                    {
                        // Initial file is considered same as runtime file, nothing to copy.
                        Logger.LogDebug("Not copying process file from {DaprProcessInitialFile} to {DaprProcessRuntimeFile} because files appear to be the same version", initialFile, proposedOptions.ProcessFile);
                    }
                    else
                    {
                        // Copy the file
                        Logger.LogInformation("Copying process file from {DaprProcessInitialFile} to {DaprProcessRuntimeFile}", initialFile, proposedOptions.ProcessFile);
                        File.Copy(initialFile, proposedOptions.ProcessFile, true);
                    }
                }

                // If the proposed file still does not exist, revert to initial file
                if (!File.Exists(proposedOptions.ProcessFile))
                {
                    Logger.LogDebug("Process file {DaprProcessRuntimeFile} does not exist at expected location. Reverting to initial location {DaprProcessInitialFile}", proposedOptions.ProcessFile, initialFile);
                    proposedOptions.ProcessFile = initialFile;
                }

                // Log out the expected location
                Logger.LogInformation("Dapr process binary: {DaprProcessFile}", proposedOptions.ProcessFile);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing directories and dapr binary location");
            }
        }

        /// <summary>
        /// Checks to see if there is an existing Dapr process runnning. If one is found and it is not considered attachable then an exception
        /// will be thrown as Dapr does not permit multiple instances of the same process.
        /// </summary>
        /// <param name="proposedOptions">The propopsed options for starting a new process instance.</param>
        /// <returns>An attachable process and its options if found, else null. If a duplicate is found an exception is thrown.</returns>
        private AttachableProcess CheckExistingProcesses(TOptions proposedOptions)
        {
            // Get a list of all existing running processes, if any
            // were started with equivalent options as this one it is a duplicate.
            var existingProcesses = ProcessFinder.FindExistingProcesses(proposedOptions.ProcessName);
            if (existingProcesses?.Any() == true)
            {
                foreach (var existingProcess in existingProcesses)
                {
                    // Create a new set of options from the command line
                    var existingOptions = new TOptions();
                    var args = existingProcess.GetCommandLine()?.GetArgumentsAsDictionary('-');
                    if (args?.Any() == true)
                    {
                        // Arguments are in sequential pairs, first entry is typically the full path to the EXE.
                        foreach (var arg in args)
                        {
                            ParseCommandLineArgument(existingOptions, arg.Key, arg.Value ?? string.Empty);
                        }
                    }

                    var comparison = CompareProcessOptions(proposedOptions, existingOptions, existingProcess);
                    if (comparison == ProcessComparison.Duplicate)
                    {
                        // Proposed options are equivalent to the existing options, cannot run a second instance
                        throw new InvalidOperationException(
                            $"Process {existingProcess.Name} PID:{existingProcess.Id} is already running with duplicate settings. Dapr does not permit duplicate equivalent instances on a single host.");
                    }
                    else if (comparison == ProcessComparison.Attachable)
                    {
                        // Found an attachable process. Assign any remaining default ports by forcing use of starting port.
                        // All other custom ports will come from the command-line args in the attached process.
                        var portBuilder = new PortAssignmentBuilder<TOptions>
                        {
                            AlwaysUseStartingPort = true
                        };

                        AssignPorts(portBuilder);
                        portBuilder.Build(existingOptions, proposedOptions, Logger);

                        // Return an attachable process
                        return new AttachableProcess(existingProcess, existingOptions);
                    }
                }
            }

            // No match
            return null;
        }

        // Return the name of the running process, or the friendly expected name if not currently running.
        private string Name => _underlyingProcess?.Name ?? _pendingOptions?.ProcessName ?? _defaultProcessName;

        private int? Id => _underlyingProcess?.Id;

        private string Version { get; set; }

        private DaprProcessStatus Status { get; set; }

        private bool IsRunning => _underlyingProcess?.IsRunning == true;

        private class AttachableProcess
        {
            public AttachableProcess(IProcess process, TOptions options)
            {
                Process = process;
                Options = options;
            }

            public IProcess Process { get; }

            public TOptions Options { get; }
        }
    }
}
