// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks.Fakes;
using NSubstitute;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class CachedHealthCheckTest
    {
        private readonly FakeServiceProvider _serviceProvider = new FakeServiceProvider();

        [Fact]
        public void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>("name", () => new TestableCachedHealthCheck(name: null));
            Assert.Throws<ArgumentException>("name", () => new TestableCachedHealthCheck(name: ""));
            Assert.Throws<ArgumentException>("cacheDuration", () => new TestableCachedHealthCheck(cacheDuration: TimeSpan.MinValue));
        }

        public class ExceptionHandling : CachedHealthCheckTest
        {
            [Fact]
            public async void CheckDoesNotThrow_ReturnsCheckResult()
            {
                var checkResult = HealthCheckResult.Healthy("Healthy Check");
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromCheck(() => checkResult));

                var result = await cachedCheck.RunAsync(_serviceProvider);

                Assert.Same(checkResult, result);
            }

            [Fact]
            public async void CancellationRequested_ReturnsUnhealthyCheck()
            {
                var checkResult = HealthCheckResult.Healthy("Happy check");
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromCheck(token => { token.ThrowIfCancellationRequested(); return checkResult; }));
                var cts = new CancellationTokenSource();
                cts.Cancel();

                var result = await cachedCheck.RunAsync(_serviceProvider, cts.Token);

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal("The health check operation timed out", result.Description);
                Assert.Empty(result.Data);
            }

            [Fact]
            public async void CheckThrows_ReturnsUnhealthyCheck()
            {
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromCheck(() => throw new DivideByZeroException()));

                var result = await cachedCheck.RunAsync(_serviceProvider);

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal($"Exception during check: {typeof(DivideByZeroException).FullName}", result.Description);
                Assert.Empty(result.Data);
            }
        }

        public class Caching : CachedHealthCheckTest
        {
            [Fact]
            public async void FirstCallReadsCheck()
            {
                var checkResult = Substitute.For<IHealthCheckResult>();
                var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
                check(default(CancellationToken)).ReturnsForAnyArgs(new ValueTask<IHealthCheckResult>(checkResult));
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromValueTaskCheck(check));

                var result = await cachedCheck.RunAsync(_serviceProvider);

                Assert.Same(checkResult, result);
            }

            [Fact]
            public async void SecondCallUsesCachedValue()
            {
                var checkResult1 = Substitute.For<IHealthCheckResult>();
                var checkResult2 = Substitute.For<IHealthCheckResult>();
                var check = Substitute.For<Func<CancellationToken, ValueTask<IHealthCheckResult>>>();
                check(default(CancellationToken)).ReturnsForAnyArgs(new ValueTask<IHealthCheckResult>(checkResult1), new ValueTask<IHealthCheckResult>(checkResult2));
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromValueTaskCheck(check), cacheDuration: TimeSpan.FromSeconds(1));

                var result1 = await cachedCheck.RunAsync(_serviceProvider);
                var result2 = await cachedCheck.RunAsync(_serviceProvider);

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
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromValueTaskCheck(check), cacheDuration: TimeSpan.FromSeconds(1));
                var now = DateTimeOffset.UtcNow;

                cachedCheck.SetUtcNow(now);
                var result1 = await cachedCheck.RunAsync(_serviceProvider);
                cachedCheck.SetUtcNow(now + TimeSpan.FromSeconds(1));
                var result2 = await cachedCheck.RunAsync(_serviceProvider);

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
                var cachedCheck = new TestableCachedHealthCheck(check: HealthCheck.FromValueTaskCheck(check), cacheDuration: TimeSpan.FromSeconds(1));

                var task1 = cachedCheck.RunAsync(_serviceProvider);
                var task2 = cachedCheck.RunAsync(_serviceProvider);
                waiter.SetResult(0);
                var result1 = await task1;
                var result2 = await task2;

                Assert.Same(checkResult1, result1);
                Assert.Same(checkResult1, result2);
            }
        }

        class TestableCachedHealthCheck : CachedHealthCheck
        {
            private readonly IHealthCheck _check;
            private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

            public TestableCachedHealthCheck(string name = "The default check name", TimeSpan cacheDuration = default(TimeSpan), IHealthCheck check = null)
                    : base(name, cacheDuration)
                => _check = check ?? HealthCheck.FromCheck(() => HealthCheckResult.Healthy("Healthy Check"));

            protected override DateTimeOffset UtcNow => _utcNow;

            protected override IHealthCheck Resolve(IServiceProvider serviceProvider) => _check;

            public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow;
        }
    }
}
