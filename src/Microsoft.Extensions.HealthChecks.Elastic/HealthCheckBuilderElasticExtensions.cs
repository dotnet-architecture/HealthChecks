// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Extensions.HealthChecks
{
    // REVIEW: What are the appropriate guards for these functions?

    public static class HealthCheckBuilderRedisExtensions
    {
        public static HealthCheckBuilder AddElasticCheck(this HealthCheckBuilder builder, IServiceCollection services, TimeSpan? cacheDuration = null)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            services.AddSingleton<ElasticHealthCheck>();
            builder.AddCheck<ElasticHealthCheck>(ElasticHealthCheck.Tag, cacheDuration ?? builder.DefaultCacheDuration);

            return builder;
        }
    }
}
