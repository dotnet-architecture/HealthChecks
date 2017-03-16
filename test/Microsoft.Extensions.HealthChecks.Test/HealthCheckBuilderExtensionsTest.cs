using System;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckBuilderExtensionsTest
    {
        HealthCheckBuilder builder = new HealthCheckBuilder();

        public class AddMinValueCheck : HealthCheckBuilderExtensionsTest
        {
            [Fact]
            public void GuardClauses()
            {
                Assert.Throws<ArgumentNullException>("builder", () => HealthCheckBuilderExtensions.AddMinValueCheck(null, "name", 42, () => 2112));
                Assert.Throws<ArgumentNullException>("name", () => HealthCheckBuilderExtensions.AddMinValueCheck(builder, null, 42, () => 2112));
                Assert.Throws<ArgumentException>("name", () => HealthCheckBuilderExtensions.AddMinValueCheck(builder, " ", 42, () => 2112));
                Assert.Throws<ArgumentNullException>("currentValueFunc", () => HealthCheckBuilderExtensions.AddMinValueCheck(builder, "name", 42, null));
            }

            [Theory]
            [InlineData(-1, CheckStatus.Unhealthy)]
            [InlineData(1, CheckStatus.Healthy)]
            public async void RegistersCheck(int monitoredValue, CheckStatus expectedStatus)
            {
                builder.AddMinValueCheck("CheckName", 0, () => monitoredValue);

                var check = builder.GetCheck("CheckName");
                Assert.NotNull(check);

                var result = await check();
                Assert.Equal(expectedStatus, result.CheckStatus);
                Assert.Equal($"CheckName: min=0, current={monitoredValue}", result.Description);
            }
        }

        public class AddMaxValueCheck : HealthCheckBuilderExtensionsTest
        {
            [Fact]
            public void GuardClauses()
            {
                Assert.Throws<ArgumentNullException>("builder", () => HealthCheckBuilderExtensions.AddMaxValueCheck(null, "name", 42, () => 2112));
                Assert.Throws<ArgumentNullException>("name", () => HealthCheckBuilderExtensions.AddMaxValueCheck(builder, null, 42, () => 2112));
                Assert.Throws<ArgumentException>("name", () => HealthCheckBuilderExtensions.AddMaxValueCheck(builder, " ", 42, () => 2112));
                Assert.Throws<ArgumentNullException>("currentValueFunc", () => HealthCheckBuilderExtensions.AddMaxValueCheck(builder, "name", 42, null));
            }

            [Theory]
            [InlineData(1, CheckStatus.Unhealthy)]
            [InlineData(-1, CheckStatus.Healthy)]
            public async void RegistersCheck(int monitoredValue, CheckStatus expectedStatus)
            {
                builder.AddMaxValueCheck("CheckName", 0, () => monitoredValue);

                var check = builder.GetCheck("CheckName");
                Assert.NotNull(check);

                var result = await check();
                Assert.Equal(expectedStatus, result.CheckStatus);
                Assert.Equal($"CheckName: max=0, current={monitoredValue}", result.Description);
            }
        }
    }
}
