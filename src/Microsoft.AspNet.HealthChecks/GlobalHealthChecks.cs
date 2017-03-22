using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.HealthChecks  // Put this in Extensions so you also have access to all the helper methods
{
    public class GlobalHealthChecks
    {
        static GlobalHealthChecks()
        {
            var logger = new LoggerFactory().CreateLogger<HealthCheckService>();

            Builder = new HealthCheckBuilder();
            Service = new HealthCheckService(Builder, logger);
        }

        public static HealthCheckBuilder Builder { get; }

        public static IHealthCheckService Service { get; }
    }
}
