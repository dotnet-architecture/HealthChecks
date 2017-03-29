// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheck : CachedHealthCheck
    {
        protected HealthCheck(
            Func<CancellationToken, ValueTask<IHealthCheckResult>> check,
            TimeSpan cacheDuration) : base(cacheDuration)
        {
            Guard.ArgumentNotNull(nameof(check), check);

            Check = check;
        }

        protected Func<CancellationToken, ValueTask<IHealthCheckResult>> Check { get; }

        protected override ValueTask<IHealthCheckResult> ExecuteCheckAsync(CancellationToken cancellationToken)
            => Check(cancellationToken);

        public static HealthCheck FromCheck(Func<IHealthCheckResult> check, TimeSpan cacheDuration)
            => new HealthCheck(token => new ValueTask<IHealthCheckResult>(check()), cacheDuration);

        public static HealthCheck FromCheck(Func<CancellationToken, IHealthCheckResult> check, TimeSpan cacheDuration)
            => new HealthCheck(token => new ValueTask<IHealthCheckResult>(check(token)), cacheDuration);

        public static HealthCheck FromTaskCheck(Func<Task<IHealthCheckResult>> check, TimeSpan cacheDuration)
            => new HealthCheck(token => new ValueTask<IHealthCheckResult>(check()), cacheDuration);

        public static HealthCheck FromTaskCheck(Func<CancellationToken, Task<IHealthCheckResult>> check, TimeSpan cacheDuration)
            => new HealthCheck(token => new ValueTask<IHealthCheckResult>(check(token)), cacheDuration);

        public static HealthCheck FromValueTaskCheck(Func<ValueTask<IHealthCheckResult>> check, TimeSpan cacheDuration)
            => new HealthCheck(token => check(), cacheDuration);

        public static HealthCheck FromValueTaskCheck(Func<CancellationToken, ValueTask<IHealthCheckResult>> check, TimeSpan cacheDuration)
            => new HealthCheck(check, cacheDuration);
    }
}
