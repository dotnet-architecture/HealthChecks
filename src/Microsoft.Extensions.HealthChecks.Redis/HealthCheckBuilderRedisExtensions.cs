// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.HealthChecks
{
    // REVIEW: What are the appropriate guards for these functions?

    public static class HealthCheckBuilderRedisExtensions
    {
        public static HealthCheckBuilder AddRedisCheck(this HealthCheckBuilder builder, IServiceCollection services, TimeSpan? cacheDuration = null)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            services.AddSingleton<RedisHealthCheck>();
            builder.AddCheck<RedisHealthCheck>(RedisHealthCheck.Tag, cacheDuration ?? builder.DefaultCacheDuration);

            return builder;
        }
    }
}
