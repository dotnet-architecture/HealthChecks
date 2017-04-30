// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks.Fakes;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckBuilderTest
    {
        private readonly HealthCheckBuilder _builder = new HealthCheckBuilder();
        private readonly FakeServiceProvider _serviceProvider = new FakeServiceProvider();

        [Fact]
        public void Defaults()
        {
            Assert.Equal(TimeSpan.FromMinutes(5), _builder.DefaultCacheDuration);
            Assert.Empty(_builder.ChecksByName);
            Assert.Collection(_builder.Groups,
                kvp =>
                {
                    // Root group has string.Empty as its name
                    Assert.Empty(kvp.Key);
                    Assert.Empty(kvp.Value.GroupName);
                    Assert.Empty(kvp.Value.Checks);
                    Assert.Equal(CheckStatus.Unhealthy, kvp.Value.PartiallyHealthyStatus);
                }
            );
        }

        public class AddCheck_Type : HealthCheckBuilderTest
        {
            [Fact]
            public void GuardClauses()
            {
                Assert.Throws<ArgumentNullException>("checkName", () => _builder.AddCheck<MyCheck>(null, TimeSpan.Zero));
                Assert.Throws<ArgumentException>("checkName", () => _builder.AddCheck<MyCheck>("", TimeSpan.Zero));
                Assert.Throws<ArgumentException>("cacheDuration", () => _builder.AddCheck<MyCheck>("myCheck", TimeSpan.FromMilliseconds(-1)));

                // Reused name
                _builder.AddCheck<MyCheck>("myCheck", TimeSpan.FromMinutes(1));
                Assert.Throws<ArgumentException>("checkName", () => _builder.AddCheck<MyCheck>("myCheck", TimeSpan.FromMinutes(1)));
            }

            [Fact]
            public void RegistersCheck()
            {
                _builder.AddCheck<MyCheck>("myCheck", TimeSpan.FromMinutes(1));

                // Registered by name
                var cachedCheck = _builder.ChecksByName["myCheck"];
                Assert.Null(cachedCheck.CachedResult);  // Hasn't been run yet
                Assert.Equal(default(DateTimeOffset), cachedCheck.CacheExpiration);
                Assert.Equal(TimeSpan.FromMinutes(1), cachedCheck.CacheDuration);
                Assert.Equal("myCheck", cachedCheck.Name);

                // Registered in the root group
                var rootGroup = Assert.Single(_builder.Groups);
                var groupedCheck = Assert.Single(rootGroup.Value.Checks);
                Assert.Same(cachedCheck, groupedCheck);
            }

            [Fact]
            public async void CheckIsResolvedViaServiceProvider()
            {
                _builder.AddCheck<MyCheck>("myCheck", TimeSpan.FromMinutes(1));
                var cachedCheck = _builder.ChecksByName["myCheck"];

                var result = await cachedCheck.RunAsync(_serviceProvider);

                Assert.Equal(CheckStatus.Healthy, result.CheckStatus);
                Assert.Equal("Happy status", result.Description);
                Assert.Collection(_serviceProvider.Operations,
                    op => Assert.Equal("Resolved type 'Microsoft.Extensions.HealthChecks.HealthCheckBuilderTest+MyCheck'", op)
                );
            }
        }

        public class AddCheck_Check : HealthCheckBuilderTest
        {
            private readonly MyCheck _myCheck = new MyCheck();

            [Fact]
            public void GuardClauses()
            {
                Assert.Throws<ArgumentNullException>("checkName", () => _builder.AddCheck(null, _myCheck, TimeSpan.Zero));
                Assert.Throws<ArgumentException>("checkName", () => _builder.AddCheck("", _myCheck, TimeSpan.Zero));
                Assert.Throws<ArgumentException>("cacheDuration", () => _builder.AddCheck("myCheck", _myCheck, TimeSpan.FromMilliseconds(-1)));
                Assert.Throws<ArgumentNullException>("check", () => _builder.AddCheck("myCheck", null, TimeSpan.Zero));

                // Reused name
                _builder.AddCheck("myCheck", _myCheck, TimeSpan.FromMinutes(1));
                var aex = Assert.Throws<ArgumentException>("checkName", () => _builder.AddCheck("myCheck", _myCheck, TimeSpan.FromMinutes(1)));
                Assert.StartsWith("A check with name 'myCheck' has already been registered.", aex.Message);
            }

            [Fact]
            public void RegistersCheck()
            {
                _builder.AddCheck("myCheck", _myCheck, TimeSpan.FromMinutes(1));

                // Registered by name
                var cachedCheck = _builder.ChecksByName["myCheck"];
                Assert.Null(cachedCheck.CachedResult);  // Hasn't been run yet
                Assert.Equal(default(DateTimeOffset), cachedCheck.CacheExpiration);
                Assert.Equal(TimeSpan.FromMinutes(1), cachedCheck.CacheDuration);
                Assert.Equal("myCheck", cachedCheck.Name);

                // Registered in the root group
                var rootGroup = Assert.Single(_builder.Groups);
                var groupedCheck = Assert.Single(rootGroup.Value.Checks);
                Assert.Same(cachedCheck, groupedCheck);
            }

            [Fact]
            public async void CheckIsUsedDirectlyWithoutServiceProviderResolution()
            {
                _builder.AddCheck("myCheck", _myCheck, TimeSpan.FromMinutes(1));
                var cachedCheck = _builder.ChecksByName["myCheck"];

                var result = await cachedCheck.RunAsync(_serviceProvider);

                Assert.Equal(CheckStatus.Healthy, result.CheckStatus);
                Assert.Equal("Happy status", result.Description);
                Assert.Empty(_serviceProvider.Operations);
            }
        }

        public class AddHealthCheckGroup : HealthCheckBuilderTest
        {
            [Fact]
            public void GuardClauses()
            {
                Assert.Throws<ArgumentNullException>("groupName", () => _builder.AddHealthCheckGroup(null, _ => { }, CheckStatus.Warning));
                Assert.Throws<ArgumentException>("groupName", () => _builder.AddHealthCheckGroup("", _ => { }, CheckStatus.Warning));
                Assert.Throws<ArgumentNullException>("groupChecks", () => _builder.AddHealthCheckGroup("myGroup", null, CheckStatus.Warning));

                var aex = Assert.Throws<ArgumentException>("partialSuccessStatus", () => _builder.AddHealthCheckGroup("groupName", _ => { }, CheckStatus.Unknown));
                Assert.StartsWith("Check status 'Unknown' is not valid for partial success.", aex.Message);

                _builder.AddHealthCheckGroup("doubleGroup", _ => { });
                aex = Assert.Throws<ArgumentException>("groupName", () => _builder.AddHealthCheckGroup("doubleGroup", _ => { }));
                Assert.StartsWith("A group with name 'doubleGroup' has already been registered.", aex.Message);

                var ioex = Assert.Throws<InvalidOperationException>(() => _builder.AddHealthCheckGroup("g1", g => g.AddHealthCheckGroup("g2", _ => { })));
                Assert.Equal("Nested groups are not supported by HealthCheckBuilder.", ioex.Message);
            }

            [Fact]
            public void RegistersChecksInGroup()
            {
                var _myCheck = new MyCheck();
                _builder.AddHealthCheckGroup("myGroup", group => group.AddCheck("myCheck", _myCheck, TimeSpan.FromMinutes(1)));

                // Registered by name
                var cachedCheck = _builder.ChecksByName["myCheck"];
                Assert.Null(cachedCheck.CachedResult);  // Hasn't been run yet
                Assert.Equal(default(DateTimeOffset), cachedCheck.CacheExpiration);
                Assert.Equal(TimeSpan.FromMinutes(1), cachedCheck.CacheDuration);
                Assert.Equal("myCheck", cachedCheck.Name);

                // Registered in the new group
                var myGroup = _builder.Groups["myGroup"];
                var groupedCheck = Assert.Single(myGroup.Checks);
                Assert.Same(cachedCheck, groupedCheck);

                // Root group remains empty
                Assert.Empty(_builder.Groups[string.Empty].Checks);
            }

            [Fact]
            public void CheckNamesMustBeGloballyUnique()
            {
                var _myCheck = new MyCheck();
                _builder.AddCheck("myCheck", _myCheck);

                Assert.Throws<ArgumentException>("checkName", () => _builder.AddHealthCheckGroup("myGroup", group => group.AddCheck("myCheck", _myCheck)));
            }
        }

        public class WithDefaultCacheDuration : HealthCheckBuilderTest
        {
            [Fact]
            public void GuardClause()
            {
                var aex = Assert.Throws<ArgumentException>("duration", () => _builder.WithDefaultCacheDuration(TimeSpan.FromMilliseconds(-1)));
                Assert.StartsWith("Duration must be zero (disabled) or a positive duration.", aex.Message);
            }

            [Fact]
            public void SetsDefaultDuration()
            {
                var duration = TimeSpan.FromSeconds(42);

                _builder.WithDefaultCacheDuration(duration);

                Assert.Equal(duration, _builder.DefaultCacheDuration);
            }
        }

        public class WithPartialSuccessStatus : HealthCheckBuilderTest
        {
            [Fact]
            public void GuardClause()
            {
                var aex = Assert.Throws<ArgumentException>(() => _builder.WithPartialSuccessStatus(CheckStatus.Unknown));
                Assert.StartsWith("Check status 'Unknown' is not valid for partial success.", aex.Message);
            }

            [Theory]
            [MemberData(nameof(CompositeHealthCheckResultTest.AllLegalStatusesExceptUnknown), MemberType = typeof(CompositeHealthCheckResultTest))]
            public void SetsPartialSuccessForRootGroup(CheckStatus status)
            {
                _builder.WithPartialSuccessStatus(status);

                Assert.Equal(status, _builder.Groups[string.Empty].PartiallyHealthyStatus);
            }
        }

        class MyCheck : IHealthCheck
        {
            public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
                => new ValueTask<IHealthCheckResult>(HealthCheckResult.Healthy("Happy status"));
        }
    }
}
