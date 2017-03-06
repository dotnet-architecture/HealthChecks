using System.Collections.Generic;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckResults
    {
        public IList<IHealthCheckResult> CheckResults { get; } = new List<IHealthCheckResult>();
    }
}
