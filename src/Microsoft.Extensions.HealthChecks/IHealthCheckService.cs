// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public interface IHealthCheckService
    {
        /// <summary>
        /// Gets the current health status. Returns <paramref name="partiallyHealthStatus"/> if there
        /// is a mix of health and non-healthy checks; returns <see cref="CheckStatus.Unhealthy"/> if
        /// there are no healthy checks.
        /// </summary>
        Task<CompositeHealthCheckResult> CheckHealthAsync(CheckStatus partiallyHealthStatus);
    }
}
