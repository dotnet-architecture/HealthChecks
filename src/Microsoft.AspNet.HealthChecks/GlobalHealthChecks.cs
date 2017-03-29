using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.HealthChecks  // Put this in Extensions so you also have access to all the helper methods
{
    public class GlobalHealthChecks
    {
        static GlobalHealthChecks()
        {
            var logger = new LoggerFactory().CreateLogger<HealthCheckService>();

            Builder = new HealthCheckBuilder();
            HandlerCheckTimeout = TimeSpan.FromSeconds(10);
            Service = new HealthCheckService(Builder, logger);
        }

        public static HealthCheckBuilder Builder { get; }

        public static TimeSpan HandlerCheckTimeout { get; private set; }

        public static IHealthCheckService Service { get; }

        public void SetHandlerCheckTimeout(TimeSpan timeout)
        {
            Guard.ArgumentValid(timeout > TimeSpan.Zero, nameof(timeout), "Health check timeout must be a positive time span");

            HandlerCheckTimeout = timeout;
        }
    }
}
