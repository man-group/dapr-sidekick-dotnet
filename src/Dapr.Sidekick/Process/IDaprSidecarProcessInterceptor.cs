﻿namespace Dapr.Sidekick.Process
{
    public interface IDaprSidecarProcessInterceptor
    {
        void OnStarting(DaprSidecarOptions options);
    }
}
