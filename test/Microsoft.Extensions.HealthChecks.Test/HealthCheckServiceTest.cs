// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckServiceTest
    {
        private HealthCheckBuilder _builder;
        private HealthCheckService _classUnderTest;

        public HealthCheckServiceTest()
        {
            _builder = new HealthCheckBuilder();
            _classUnderTest = new HealthCheckService(_builder);
        }

        [Fact]
        public async void NoChecks_ReturnsUnknownStatus()
        {
            var result = await _classUnderTest.CheckHealthAsync();

            Assert.Equal(CheckStatus.Unknown, result.CheckStatus);
            Assert.Empty(result.Results);
        }

        [Fact]
        public async void Checks_ReturnsCompositeStatus()
        {
            _builder.AddCheck("c1", HealthCheck.FromCheck(() => HealthCheckResult.Healthy("Healthy check"), TimeSpan.Zero));
            _builder.AddCheck("c2", HealthCheck.FromCheck(() => HealthCheckResult.Unhealthy("Unhealthy check"), TimeSpan.Zero));

            var result = await _classUnderTest.CheckHealthAsync(CheckStatus.Warning);

            Assert.Equal(CheckStatus.Warning, result.CheckStatus);
            Assert.Collection(result.Results.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value.CheckStatus} ({kvp.Value.Description})'"),
                item => Assert.Equal("'c1' = 'Healthy (Healthy check)'", item),
                item => Assert.Equal("'c2' = 'Unhealthy (Unhealthy check)'", item)
            );
        }
    }
}
