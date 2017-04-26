// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    /// <summary>
    /// This class is responsible for executing checks, including ensuring that checks which throw automatically
    /// return unhealth results.
    /// </summary>
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

        public static async Task<CompositeHealthCheckResult> RunChecksAsync(
            IEnumerable<KeyValuePair<string, IHealthCheck>> healthChecks,
            CheckStatus partiallyHealthyStatus,
            CancellationToken cancellationToken)
        {
            Guard.ArgumentNotNull(nameof(healthChecks), healthChecks);

            var result = new CompositeHealthCheckResult(partiallyHealthyStatus);
            var healthCheckTasks = healthChecks.Select(check => new { Key = check.Key, Task = RunCheckAsync(check.Value, cancellationToken).AsTask() }).ToList();
            await Task.WhenAll(healthCheckTasks.Select(x => x.Task)).ConfigureAwait(false);

            foreach (var healthCheckTask in healthCheckTasks)
                result.Add(healthCheckTask.Key, healthCheckTask.Task.Result);

            return result;
        }
    }
}
