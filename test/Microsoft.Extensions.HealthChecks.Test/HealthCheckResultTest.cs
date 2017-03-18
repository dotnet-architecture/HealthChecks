// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
