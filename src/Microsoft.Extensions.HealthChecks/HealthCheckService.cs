// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckService : IHealthCheckService
    {
        public IReadOnlyDictionary<string, IHealthCheck> _checks;

        private ILogger<HealthCheckService> _logger;

        public HealthCheckService(HealthCheckBuilder builder, ILogger<HealthCheckService> logger)
        {
            _checks = builder.Checks;
            _logger = logger;
        }

        public async Task<CompositeHealthCheckResult> CheckHealthAsync(CheckStatus partiallyHealthyStatus, CancellationToken cancellationToken)
        {
            var logMessage = new StringBuilder();
            var result = new CompositeHealthCheckResult(partiallyHealthyStatus);

            try
            {
                var healthCheckTasks = _checks.Select(check => new { Key = check.Key, Task = check.Value.CheckAsync(cancellationToken).AsTask() }).ToList();
                await Task.WhenAll(healthCheckTasks.Select(x => x.Task)).ConfigureAwait(false);

                foreach (var healthCheckTask in healthCheckTasks)
                {
                    try
                    {
                        var healthCheckResult = healthCheckTask.Task.Result;
                        logMessage.AppendLine($"HealthCheck: {healthCheckTask.Key} : {healthCheckResult.CheckStatus}");
                        result.Add(healthCheckTask.Key, healthCheckResult);
                    }
                    catch (Exception ex)
                    {
                        logMessage.AppendLine($"HealthCheck: {healthCheckTask.Key} : Exception {ex.GetType().FullName} thrown");
                        result.Add(healthCheckTask.Key, CheckStatus.Unhealthy, $"Exception during check: {ex.GetType().FullName}");
                    }
                }

                if (logMessage.Length == 0)
                    logMessage.AppendLine("HealthCheck: No checks have been registered");

                _logger.Log((result.CheckStatus == CheckStatus.Healthy ? LogLevel.Information : LogLevel.Error), 0, logMessage.ToString(), null, MessageFormatter);
                return result;
            }
            catch (TaskCanceledException)
            {
                result = new CompositeHealthCheckResult();
                result.Add("*", CheckStatus.Unhealthy, "The health check operation timed out");
                _logger.Log(LogLevel.Warning, 0, result.Description, null, MessageFormatter);
                return result;
            }
        }

        private static string MessageFormatter(string state, Exception error) => state;
    }
}
