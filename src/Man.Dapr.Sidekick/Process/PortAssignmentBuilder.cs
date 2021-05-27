using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Man.Dapr.Sidekick.Logging;

namespace Man.Dapr.Sidekick.Process
{
    internal class PortAssignmentBuilder<TOptions>
        where TOptions : Options.DaprProcessOptions
    {
        private class PortInfo
        {
            public PortInfo(Expression<Func<TOptions, int?>> property, int startingPort)
            {
                Property = property;
                StartingPort = startingPort;
            }

            public Expression<Func<TOptions, int?>> Property { get; }

            public int StartingPort { get; }
        }

        private readonly List<PortInfo> _ports = new List<PortInfo>();

        public PortAssignmentBuilder(IPortAvailabilityChecker portAvailabilityChecker)
        {
            PortAvailabilityChecker = portAvailabilityChecker;
        }

        public PortAssignmentBuilder()
            : this(new PortAvailabilityChecker())
        {
        }

        // For testing
        internal IPortAvailabilityChecker PortAvailabilityChecker { get; }

        public PortAssignmentBuilder<TOptions> Add(Expression<Func<TOptions, int?>> property, int startingPort)
        {
            _ports.Add(new PortInfo(property, startingPort));
            return this;
        }

        public void Build(TOptions proposedOptions, TOptions lastSuccessfulOptions, IDaprLogger logger)
        {
            // Determine if previously assigned ports shoulld be retained
            var retainPreviousPorts = (proposedOptions.RetainPortsOnRestart ?? true) && lastSuccessfulOptions != null;

            // Iterate through each port
            var reservedPorts = new List<int>();
            foreach (var port in _ports)
            {
                // Get the existing values
                var propertyInfo = (port.Property.Body as MemberExpression)?.Member as PropertyInfo;
                var propertyName = propertyInfo.Name;
                var lastSuccessfulValue = lastSuccessfulOptions != null ? (int?)propertyInfo.GetValue(lastSuccessfulOptions, null) : default;
                var proposedValue = (int?)propertyInfo.GetValue(proposedOptions, null);

                // If we need to retain previous ports and port has not been explicitly specified then copy the value over and add to the reserved list.
                if (retainPreviousPorts && lastSuccessfulValue.HasValue && !proposedValue.HasValue)
                {
                    logger.LogDebug("Assigning previously reserved port {DaprPortNumber} for option {DaprPortName}", lastSuccessfulValue, propertyName);
                    propertyInfo.SetValue(proposedOptions, lastSuccessfulValue, null);
                    reservedPorts.Add(lastSuccessfulValue.Value);
                    continue;
                }

                // If port is already defined, then use it
                if (proposedValue.HasValue)
                {
                    logger.LogDebug("Assigning preferred port {DaprPortNumber} for option {DaprPortName}", proposedValue, propertyName);
                    reservedPorts.Add(proposedValue.Value);
                    continue;
                }

                // Assign a new port
                var newPort = PortAvailabilityChecker.GetAvailablePort(port.StartingPort, reservedPorts);
                propertyInfo.SetValue(proposedOptions, newPort, null);
                reservedPorts.Add(newPort);
                logger.LogDebug("Reserving new port {DaprPortNumber} for option {DaprPortName}", newPort, propertyName);
            }
        }
    }
}
