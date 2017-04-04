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
        private readonly Lazy<IReadOnlyDictionary<string, IHealthCheck>> _checks;
        private readonly ILogger<HealthCheckService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public HealthCheckService(
            IServiceProvider serviceProvider,
            HealthCheckBuilder builder,
            ILogger<HealthCheckService> logger)
        {
            Guard.ArgumentNotNull(nameof(serviceProvider), serviceProvider);
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNull(nameof(logger), logger);

            _serviceProvider = serviceProvider;
            _logger = logger;

            _checks = new Lazy<IReadOnlyDictionary<string, IHealthCheck>>(
                () => builder.Checks.ToDictionary(kvp => kvp.Key, kvp => CheckExecutor.ResolveCheck(kvp.Value, _serviceProvider))
            );
        }

        public async Task<CompositeHealthCheckResult> CheckHealthAsync(CheckStatus partiallyHealthyStatus, CancellationToken cancellationToken)
        {
            var results = await CheckExecutor.RunChecksAsync(_checks.Value, partiallyHealthyStatus, cancellationToken);
            var logMessage = new StringBuilder();

            // REVIEW: This only logs the top-level results. Should we dive into composites when logging?
            foreach (var result in results.Results)
                logMessage.AppendLine($"HealthCheck: {result.Key} : {result.Value.CheckStatus} : {result.Value.Description}");

            if (logMessage.Length == 0)
                logMessage.AppendLine("HealthCheck: No checks have been registered");

            _logger.Log((results.CheckStatus == CheckStatus.Healthy ? LogLevel.Information : LogLevel.Error), 0, logMessage.ToString(), null, MessageFormatter);
            return results;
        }

        // This entry point is for non-DI (we leave the single constructor in place for DI)
        public static HealthCheckService FromBuilder(HealthCheckBuilder builder, ILogger<HealthCheckService> logger)
            => new HealthCheckService(new SimpleSingletonServiceProvider(), builder, logger);

        private static string MessageFormatter(string state, Exception error) => state;

        class SimpleSingletonServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

            public object GetService(Type serviceType)
            {
                if (!_singletons.TryGetValue(serviceType, out var result))
                {
                    result = Activator.CreateInstance(serviceType);
                    _singletons[serviceType] = result;
                }

                return result;
            }
        }
    }
}
