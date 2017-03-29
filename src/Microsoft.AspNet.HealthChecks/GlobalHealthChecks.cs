using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.HealthChecks  // Put this in Extensions so you also have access to all the helper methods
{
    public class GlobalHealthChecks
    {
        static GlobalHealthChecks()
        {
            // REVIEW: Should we add a way to override the service collection, or just assume no DI here?
            var logger = new LoggerFactory().CreateLogger<HealthCheckService>();

            Builder = new HealthCheckBuilder(null);
            HandlerCheckTimeout = TimeSpan.FromSeconds(10);
            Service = HealthCheckService.FromBuilder(Builder, logger);
        }

        public static HealthCheckBuilder Builder { get; }

        public static TimeSpan HandlerCheckTimeout { get; private set; }

        public static IHealthCheckService Service { get; }

        public static void SetHandlerCheckTimeout(TimeSpan timeout)
        {
            Guard.ArgumentValid(timeout > TimeSpan.Zero, nameof(timeout), "Health check timeout must be a positive time span");

            HandlerCheckTimeout = timeout;
        }
    }
}
