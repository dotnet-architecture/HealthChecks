// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class CachedHealthCheckTest
    {
        [Fact]
        public void GuardClauses()
        {
            Func<CancellationToken, ValueTask<IHealthCheckResult>> check = _ => new ValueTask<IHealthCheckResult>(default(IHealthCheckResult));

            Assert.Throws<ArgumentException>("cacheDuration", () => new TestableCachedHealthCheck(check, TimeSpan.FromMinutes(-1)));
        }

        [Fact]
        public async void FirstCallReadsCheck()
        {
            var checkResult = Substitute.For<IHealthCheckResult>();
            var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
            check(default(CancellationToken)).ReturnsForAnyArgs(new ValueTask<IHealthCheckResult>(checkResult));
            var healthCheck = new TestableCachedHealthCheck(check);

            var result = await healthCheck.CheckAsync();

            Assert.Same(checkResult, result);
        }

        [Fact]
        public async void SecondCallUsesCachedValue()
        {
            var checkResult1 = Substitute.For<IHealthCheckResult>();
            var checkResult2 = Substitute.For<IHealthCheckResult>();
            var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
            check(default(CancellationToken)).ReturnsForAnyArgs(new ValueTask<IHealthCheckResult>(checkResult1), new ValueTask<IHealthCheckResult>(checkResult2));
            var healthCheck = new TestableCachedHealthCheck(check, TimeSpan.FromSeconds(1));

            var result1 = await healthCheck.CheckAsync();
            var result2 = await healthCheck.CheckAsync();

            Assert.Same(checkResult1, result1);
            Assert.Same(checkResult1, result2);
        }

        [Fact]
        public async void CachedValueRefreshedAfterTimeout()
        {
            var checkResult1 = Substitute.For<IHealthCheckResult>();
            var checkResult2 = Substitute.For<IHealthCheckResult>();
            var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
            check(default(CancellationToken)).ReturnsForAnyArgs(new ValueTask<IHealthCheckResult>(checkResult1), new ValueTask<IHealthCheckResult>(checkResult2));
            var healthCheck = new TestableCachedHealthCheck(check, TimeSpan.FromSeconds(1));
            var now = DateTimeOffset.UtcNow;

            healthCheck.SetUtcNow(now);
            var result1 = await healthCheck.CheckAsync();
            healthCheck.SetUtcNow(now + TimeSpan.FromSeconds(1));
            var result2 = await healthCheck.CheckAsync();

            Assert.Same(checkResult1, result1);
            Assert.Same(checkResult2, result2);
        }

        [Fact]
        public async void MultipleCallersDuringRefreshPeriodOnlyResultInASingleValue()
        {
            var checkResult1 = Substitute.For<IHealthCheckResult>();
            var checkResult2 = Substitute.For<IHealthCheckResult>();
            var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
            var waiter = new TaskCompletionSource<int>();
            var firstTask = new ValueTask<IHealthCheckResult>(((Func<Task<IHealthCheckResult>>)(async () =>
            {
                await waiter.Task;
                return checkResult1;
            }))());
            var secondTask = new ValueTask<IHealthCheckResult>(checkResult2);
            check(default(CancellationToken)).ReturnsForAnyArgs(firstTask, secondTask);
            var healthCheck = new TestableCachedHealthCheck(check, TimeSpan.FromSeconds(1));

            var task1 = healthCheck.CheckAsync();
            var task2 = healthCheck.CheckAsync();
            waiter.SetResult(0);
            var result1 = await task1;
            var result2 = await task2;

            Assert.Same(checkResult1, result1);
            Assert.Same(checkResult1, result2);
        }

        class TestableCachedHealthCheck : CachedHealthCheck
        {
            private readonly Func<CancellationToken, ValueTask<IHealthCheckResult>> _check;
            private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

            public TestableCachedHealthCheck(
                Func<CancellationToken, ValueTask<IHealthCheckResult>> check,
                TimeSpan cacheDuration = default(TimeSpan)) : base(cacheDuration)
            {
                _check = check;
            }

            protected override DateTimeOffset UtcNow => _utcNow;

            public void SetUtcNow(DateTimeOffset utcNow)
                => _utcNow = utcNow;

            protected override ValueTask<IHealthCheckResult> ExecuteCheckAsync(CancellationToken cancellationToken)
                => _check(cancellationToken);
        }
    }
}
