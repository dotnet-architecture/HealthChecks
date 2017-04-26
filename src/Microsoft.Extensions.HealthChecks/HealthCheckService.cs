// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckService : IHealthCheckService
    {
        public IReadOnlyDictionary<string, IHealthCheck> _checks;

        public HealthCheckService(HealthCheckBuilder builder)
            => _checks = builder.Checks;

        public Task<CompositeHealthCheckResult> CheckHealthAsync(CheckStatus partiallyHealthyStatus, CancellationToken cancellationToken)
            => CheckExecutor.RunChecksAsync(_checks, partiallyHealthyStatus, cancellationToken);
    }
}
