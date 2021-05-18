﻿namespace Dapr.Sidekick.Process
{
    public interface IDaprProcessFactory
    {
        IDaprSidecarProcess CreateDaprSidecarProcess();

        IDaprPlacementProcess CreateDaprPlacementProcess();

        IDaprSentryProcess CreateDaprSentryProcess();
    }
}
