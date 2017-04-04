// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Extensions.HealthChecks.Checks
{
    public class GroupCheckTest
    {
        HealthCheckBuilder builder = new HealthCheckBuilder();

        [Fact]
        public void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>("builder", () => HealthCheckBuilderExtensions.AddHealthCheckGroup(null, null, null));
            Assert.Throws<ArgumentNullException>("groupName", () => HealthCheckBuilderExtensions.AddHealthCheckGroup(builder, null, null));
            Assert.Throws<ArgumentException>("groupName", () => HealthCheckBuilderExtensions.AddHealthCheckGroup(builder, String.Empty, null));
            Assert.Throws<ArgumentNullException>("innerChecks", () => HealthCheckBuilderExtensions.AddHealthCheckGroup(builder, "groupName", null));
        }

        [Fact]
        public async void AddNoCheckToGroup()
        {
            builder.AddHealthCheckGroup("NoChecks", group => { });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Unknown);
                Assert.Equal(result.Results.Count, 0);
            }
        }

        [Fact]
        public async void AddSingleUnhealthyCheckToGroupShouldReturnUnhealthy()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Unhealthy", () => HealthCheckResult.Unhealthy("Unhealthy"));
            });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Unhealthy);
                Assert.Equal(result.Results.Count, 1);
            }
        }

        [Fact]
        public async void AddSingleHealthyCheckToGroupShouldReturnHealthy()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Healthy", () => HealthCheckResult.Healthy("Healthy"));
            });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Healthy);
                Assert.Equal(result.Results.Count, 1);
            }
        }

        [Fact]
        public async void AddTwoUnhealthyCheckToGroupShouldReturnUnhealthy()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Unhealthy_1", () => HealthCheckResult.Unhealthy("Unhealthy"));
                group.AddCheck("Unhealthy_2", () => HealthCheckResult.Unhealthy("Unhealthy"));
            });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Unhealthy);
                Assert.Equal(result.Results.Count, 2);
            }
        }

        [Fact]
        public async void AddTwoHealthyCheckToGroupShouldReturnHealthy()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Healthy_1", () => HealthCheckResult.Healthy("Healthy"));
                group.AddCheck("Healthy_2", () => HealthCheckResult.Healthy("Healthy"));
            });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Healthy);
                Assert.Equal(result.Results.Count, 2);
            }
        }

        [Fact]
        public async void AddOneHealthyAndOneUnhealthyCheckToGroupShouldReturnWarning()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Healthy", () => HealthCheckResult.Healthy("Healthy"));
                group.AddCheck("Unhealthy", () => HealthCheckResult.Unhealthy("Unhealthy"));
            });

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Warning);
                Assert.Equal(result.Results.Count, 2);
            }
        }

        [Fact]
        public async void AddOneHealthyAndOneUnhealthyCheckToGroupShouldReturnUnhealthy()
        {
            builder.AddHealthCheckGroup("NoChecks", group =>
            {
                group.AddCheck("Healthy", () => HealthCheckResult.Healthy("Healthy"));
                group.AddCheck("Unhealthy", () => HealthCheckResult.Unhealthy("Unhealthy"));
            }, CheckStatus.Unhealthy);

            var checks = builder.Checks;
            foreach (var check in checks)
            {
                var result = await check.Value.CheckAsync() as CompositeHealthCheckResult;
                Assert.NotNull(result);
                Assert.Equal(result.CheckStatus, CheckStatus.Unhealthy);
                Assert.Equal(result.Results.Count, 2);
            }
        }
    }
}
