// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks.Fakes;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckServiceTest
    {
        private HealthCheckBuilder _builder;
        private HealthCheckService _classUnderTest;
        private FakeLogger<HealthCheckService> _logger;

        public HealthCheckServiceTest()
        {
            _builder = new HealthCheckBuilder(new ServiceCollection());
            _logger = new FakeLogger<HealthCheckService>();
            _classUnderTest = HealthCheckService.FromBuilder(_builder, _logger);
        }

        [Fact]
        public async void NoChecks_ReturnsUnknownStatus_LogsError()
        {
            var result = await _classUnderTest.CheckHealthAsync();

            Assert.Equal(CheckStatus.Unknown, result.CheckStatus);
            Assert.Empty(result.Results);
            var operation = Assert.Single(_logger.Operations);
            Assert.Equal($"Log: level=Error, id=0, exception=(null), message='HealthCheck: No checks have been registered{Environment.NewLine}'", operation);
        }

        [Fact]
        public async void Checks_ReturnsCompositeStatus_LogsCheckInfo()
        {
            _builder.AddCheck("c1", HealthCheck.FromCheck(() => HealthCheckResult.Healthy("Healthy check"), TimeSpan.Zero));
            _builder.AddCheck("c2", HealthCheck.FromCheck(() => HealthCheckResult.Unhealthy("Unhealthy check"), TimeSpan.Zero));

            var result = await _classUnderTest.CheckHealthAsync(CheckStatus.Warning);

            Assert.Equal(CheckStatus.Warning, result.CheckStatus);
            Assert.Collection(result.Results.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value.CheckStatus} ({kvp.Value.Description})'"),
                item => Assert.Equal("'c1' = 'Healthy (Healthy check)'", item),
                item => Assert.Equal("'c2' = 'Unhealthy (Unhealthy check)'", item)
            );
            var operation = Assert.Single(_logger.Operations);
            Assert.StartsWith("Log: level=Error, id=0, exception=(null), message='", operation);
            Assert.Contains("HealthCheck: c2 : Unhealthy : Unhealthy check", operation);
            Assert.Contains("HealthCheck: c1 : Healthy : Healthy check", operation);
        }
    }
}
