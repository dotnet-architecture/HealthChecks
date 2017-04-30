// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks;

namespace SampleHealthChecker
{
    public class CustomHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;

        public CustomHealthCheck(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            => new ValueTask<IHealthCheckResult>(HealthCheckResult.FromStatus(_serviceProvider == null ? CheckStatus.Unhealthy : CheckStatus.Healthy, "Testing DI support"));
    }
}
