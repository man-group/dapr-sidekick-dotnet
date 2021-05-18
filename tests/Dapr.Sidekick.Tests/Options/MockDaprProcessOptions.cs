using System;

namespace Dapr.Sidekick.Options
{
    public class MockDaprProcessOptions : DaprProcessOptions
    {
        public int? HealthPort { get; set; }

        public int? MetricsPort { get; set; }

        protected override bool AddHealthUri(UriBuilder builder)
        {
            if (HealthPort.HasValue)
            {
                builder.Port = HealthPort.Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override bool AddMetricsUri(UriBuilder builder)
        {
            if (MetricsPort.HasValue)
            {
                builder.Port = MetricsPort.Value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class MockDaprProcessOptionsNoOverrides : DaprProcessOptions
    {
    }
}
