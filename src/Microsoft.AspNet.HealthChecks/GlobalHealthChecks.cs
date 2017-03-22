using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNet.HealthChecks
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
