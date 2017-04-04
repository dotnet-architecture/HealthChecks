// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var results = await CheckExecutor.RunChecksAsync(_checks, partiallyHealthyStatus, cancellationToken);
            var logMessage = new StringBuilder();

            // REVIEW: This only logs the top-level results. Should we dive into composites when logging?
            foreach (var result in results.Results)
                logMessage.AppendLine($"HealthCheck: {result.Key} : {result.Value.CheckStatus} : {result.Value.Description}");

            if (logMessage.Length == 0)
                logMessage.AppendLine("HealthCheck: No checks have been registered");

            _logger.Log((results.CheckStatus == CheckStatus.Healthy ? LogLevel.Information : LogLevel.Error), 0, logMessage.ToString(), null, MessageFormatter);
            return results;
        }

        private static string MessageFormatter(string state, Exception error) => state;
    }
}
