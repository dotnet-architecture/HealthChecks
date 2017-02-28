using System.Collections.Generic;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckResults
    {
        public IList<HealthCheckResult> CheckResults { get; } = new List<HealthCheckResult>();
    }
}
