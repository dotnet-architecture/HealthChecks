using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckServiceTest
    {
        private HealthCheckBuilder _builder;
        private HealthCheckService _classUnderTest;
        private ILogger<HealthCheckService> _logger;

        public HealthCheckServiceTest()
        {
            _builder = new HealthCheckBuilder();
            _logger = Substitute.For<ILogger<HealthCheckService>>();
            _classUnderTest = new HealthCheckService(_builder, _logger);
        }

        [Fact]
        public async void NoChecks_ReturnsUnknownStatus_LogsError()
        {
            var result = await _classUnderTest.CheckHealthAsync();

            Assert.Equal(CheckStatus.Unknown, result.CheckStatus);
            Assert.Empty(result.Results);
            _logger.Received(1).Log(LogLevel.Error, 0, $"HealthCheck: No checks have been registered{Environment.NewLine}", null, Arg.Any<Func<string, Exception, string>>());
        }
    }
}
