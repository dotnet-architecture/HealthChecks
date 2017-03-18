// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public static class HealthCheckServiceExtensions
    {
        /// <summary>
        /// Gets the current health status. Returns <see cref="CheckStatus.Unhealthy"/> if any
        /// health check does not return <see cref="CheckStatus.Healthy"/>.
        /// </summary>
        public static Task<CompositeHealthCheckResult> CheckHealthAsync(this IHealthCheckService service)
        {
            Guard.ArgumentNotNull(nameof(service), service);

            return service.CheckHealthAsync(CheckStatus.Unhealthy);
        }
    }
}
