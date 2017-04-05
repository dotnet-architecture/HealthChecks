// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class CheckExecutorTests
    {
        public class RunCheckAsync
        {
            [Fact]
            public async void GuardClause()
            {
                await Assert.ThrowsAsync<ArgumentNullException>("healthCheck", () => CheckExecutor.RunCheckAsync(null, default(CancellationToken)).AsTask());
            }

            [Fact]
            public async void CheckDoesNotThrow_ReturnsCheckResult()
            {
                var checkResult = HealthCheckResult.Healthy("Happy check");

                var result = await CheckExecutor.RunCheckAsync(HealthCheck.FromCheck(() => checkResult, TimeSpan.Zero), default(CancellationToken));

                Assert.Same(result, checkResult);
            }

            [Fact]
            public async void CheckThrows_ReturnsUnhealthyCheck()
            {
                var result = await CheckExecutor.RunCheckAsync(HealthCheck.FromCheck(() => throw new DivideByZeroException(), TimeSpan.Zero), default(CancellationToken));

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal($"Exception during check: {typeof(DivideByZeroException).FullName}", result.Description);
                Assert.Empty(result.Data);
            }

            [Fact]
            public async void CancellationRequested_ReturnsUnhealthyCheck()
            {
                var checkResult = HealthCheckResult.Healthy("Happy check");
                var check = HealthCheck.FromCheck(token => { token.ThrowIfCancellationRequested(); return checkResult; }, TimeSpan.Zero);
                var cts = new CancellationTokenSource();
                cts.Cancel();

                var result = await CheckExecutor.RunCheckAsync(check, cts.Token);

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal<object>("The health check operation timed out", result.Description);
                Assert.Empty(result.Data);
            }
        }

        public class RunChecksAsync
        {
            [Fact]
            public async void GuardClause()
            {
                await Assert.ThrowsAsync<ArgumentNullException>("healthChecks", () => CheckExecutor.RunChecksAsync(null, CheckStatus.Unhealthy, default(CancellationToken)));
            }

            [Fact]
            public async void RunsAllChecks()
            {
                var check1 = HealthCheck.FromCheck(() => HealthCheckResult.Healthy("Healthy check"), TimeSpan.Zero);
                var check2 = HealthCheck.FromCheck(() => HealthCheckResult.Unhealthy("Unhealthy check"), TimeSpan.Zero);
                var check3 = HealthCheck.FromCheck(() => throw new DivideByZeroException(), TimeSpan.Zero);
                var check4 = HealthCheck.FromCheck(token => { token.ThrowIfCancellationRequested(); return HealthCheckResult.Healthy("Happy check"); }, TimeSpan.Zero);
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var checks = new Dictionary<string, IHealthCheck> { { "c1", check1 }, { "c2", check2 }, { "c3", check3 }, { "c4", check4 } };

                var result = await CheckExecutor.RunChecksAsync(checks, CheckStatus.Warning, cts.Token);

                Assert.Equal(CheckStatus.Warning, result.CheckStatus);
                Assert.Collection(result.Results.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value.CheckStatus} ({kvp.Value.Description})'"),
                    item => Assert.Equal("'c1' = 'Healthy (Healthy check)'", item),
                    item => Assert.Equal("'c2' = 'Unhealthy (Unhealthy check)'", item),
                    item => Assert.Equal($"'c3' = 'Unhealthy (Exception during check: {typeof(DivideByZeroException).FullName})'", item),
                    item => Assert.Equal("'c4' = 'Unhealthy (The health check operation timed out)'", item)
                );
            }
        }
    }
}
