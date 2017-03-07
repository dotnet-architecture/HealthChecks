using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckBuilder
    {
        public Dictionary<string, Func<ValueTask<IHealthCheckResult>>> Checks { get; private set; }

        public HealthCheckBuilder()
        {
            Checks = new Dictionary<string, Func<ValueTask<IHealthCheckResult>>>();
        }

        public HealthCheckBuilder AddCheck(string name, Func<Task<IHealthCheckResult>> check)
        {
            Checks.Add(name, () => new ValueTask<IHealthCheckResult>(check()));
            return this;
        }

        public HealthCheckBuilder AddCheck(string name, Func<IHealthCheckResult> check)
        {
            Checks.Add(name, () => new ValueTask<IHealthCheckResult>(check()));
            return this;
        }

        // REVIEW: This is clearly not the right API, but it'll suffice for now for the purposes of testing
        public Func<ValueTask<IHealthCheckResult>> GetCheck(string name)
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);

            return Checks.TryGetValue(name, out Func<ValueTask<IHealthCheckResult>> result) ? result : null;
        }
    }
}