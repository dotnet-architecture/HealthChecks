using System;
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
    }
}
