﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace Dapr.Sidekick.Process
{
    public class PortAvailabilityChecker : IPortAvailabilityChecker
    {
        public int GetAvailablePort(int startingPort, IEnumerable<int> reservedPorts = null)
        {
            if (startingPort > ushort.MaxValue)
            {
                throw new ArgumentException($"Starting Port cannot be greater than {ushort.MaxValue}", nameof(startingPort));
            }

            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

            var connectionsEndpoints = ipGlobalProperties.GetActiveTcpConnections().Select(c => c.LocalEndPoint);
            var tcpListenersEndpoints = ipGlobalProperties.GetActiveTcpListeners();
            var udpListenersEndpoints = ipGlobalProperties.GetActiveUdpListeners();
            var portsInUse = connectionsEndpoints.Concat(tcpListenersEndpoints)
                                                 .Concat(udpListenersEndpoints)
                                                 .Select(e => e.Port)
                                                 .ToList();
            // Add any additional reserved ports
            if (reservedPorts != null)
            {
                portsInUse.AddRange(reservedPorts);
            }

            return Enumerable.Range(startingPort, ushort.MaxValue - startingPort + 1).Except(portsInUse).FirstOrDefault();
        }
    }
}
