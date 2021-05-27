using System;
using Man.Dapr.Sidekick.Http;
using Man.Dapr.Sidekick.Logging;
using Man.Dapr.Sidekick.Threading;

namespace Man.Dapr.Sidekick.Process
{
    public abstract partial class DaprProcessHost<TOptions> : IDaprProcessHost
        where TOptions : Options.DaprProcessOptions
    {
        private readonly object _processLock = new object();
        private readonly Func<IDaprProcess<TOptions>> _createDaprProcess;

        protected DaprProcessHost(
            Func<IDaprProcess<TOptions>> createDaprProcess,
            IDaprProcessHttpClientFactory daprHttpClientFactory,
            IDaprLogger logger)
        {
            _createDaprProcess = createDaprProcess ?? throw new ArgumentNullException(nameof(createDaprProcess));
            DaprHttpClientFactory = daprHttpClientFactory ?? throw new ArgumentNullException(nameof(daprHttpClientFactory));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Internal for testing
        protected internal IDaprProcessHttpClientFactory DaprHttpClientFactory { get; }

        public DaprProcessInfo GetProcessInfo() => Process?.GetProcessInfo() ?? DaprProcessInfo.Unknown;

        public TOptions GetProcessOptions() => Process?.LastSuccessfulOptions;

        Options.DaprProcessOptions IDaprProcessHost.GetProcessOptions() => GetProcessOptions();

        public bool Start(Func<DaprOptions> optionsAccessor, DaprCancellationToken cancellationToken)
        {
            lock (_processLock)
            {
                // Stop process if already running
                Stop(cancellationToken);

                // Start the new process.
                Process = _createDaprProcess();
                Process.Starting += ProcessStarting;
                Process.Stopping += ProcessStopping;
                return Process.Start(optionsAccessor, Logger, cancellationToken);
            }
        }

        public void Stop(DaprCancellationToken cancellationToken)
        {
            lock (_processLock)
            {
                if (Process != null)
                {
                    // Stop the process
                    Process.Stop(cancellationToken);
                    Process.Starting -= ProcessStarting;
                    Process.Stopping -= ProcessStopping;

                    // Clear out the variables
                    Process = null;
                }
            }
        }

        // Internal for testing
        protected internal IDaprLogger Logger { get; }

        // Internal for testing
        protected internal IDaprProcess<TOptions> Process { get; private set; }

        protected virtual void OnProcessStarting(DaprProcessStartingEventArgs<TOptions> args)
        {
        }

        protected virtual void OnProcessStopping(DaprProcessStoppingEventArgs args)
        {
        }

        private void ProcessStarting(object sender, DaprProcessStartingEventArgs<TOptions> e) => OnProcessStarting(e);

        private void ProcessStopping(object sender, DaprProcessStoppingEventArgs e) => OnProcessStopping(e);
    }
}
