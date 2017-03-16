using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckResultTest
    {
        [Fact]
        public void ReturnsEmptyDataWhenPassedNull()
        {
            var checkResult = HealthCheckResult.Healthy("Hello world", null);

            var data = checkResult.Data;

            Assert.NotNull(data);
            Assert.Empty(data);
        }
    }
}
