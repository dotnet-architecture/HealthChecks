using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class CheckExecutor
    {
        public static ValueTask<IHealthCheckResult> RunCheckAsync(IHealthCheck healthCheck, CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(nameof(healthCheck), healthCheck);

            return RunCheckAsync(token => healthCheck.CheckAsync(token), cancellationToken);
        }

        internal static async ValueTask<IHealthCheckResult> RunCheckAsync(Func<CancellationToken, ValueTask<IHealthCheckResult>> thunk, CancellationToken cancellationToken)
        {
            try
            {
                return await thunk(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return HealthCheckResult.Unhealthy("The health check operation timed out");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}");
            }
        }
    }
}
