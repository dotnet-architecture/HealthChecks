// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckBuilder
    {
        // Contains either a Type (to be resolved later) or an object (already provided)
        private readonly Dictionary<string, object> _checks;

        public HealthCheckBuilder(IServiceCollection serviceCollection)
        {
            _checks = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            ServiceCollection = serviceCollection;
            DefaultCacheDuration = TimeSpan.FromMinutes(5);
        }

        public IReadOnlyDictionary<string, object> Checks => _checks;

        public TimeSpan DefaultCacheDuration { get; private set; }

        public IServiceCollection ServiceCollection { get; }

        public HealthCheckBuilder AddCheck<TCheck>(string name, TCheck check) where TCheck : class, IHealthCheck
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);
            Guard.ArgumentNotNull(nameof(check), check);

            _checks.Add(name, check);
            return this;
        }

        public HealthCheckBuilder AddCheck<TCheck>(string name) where TCheck : class, IHealthCheck
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);

            ServiceCollection?.AddSingleton<TCheck>();
            _checks.Add(name, typeof(TCheck));
            return this;
        }

        public HealthCheckBuilder WithDefaultCacheDuration(TimeSpan duration)
        {
            Guard.ArgumentValid(duration >= TimeSpan.Zero, nameof(duration), "Duration must be zero (disabled) or a positive duration");

            DefaultCacheDuration = duration;
            return this;
        }
    }
}
