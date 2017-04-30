// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.HealthChecks  // Put this in Extensions so you also have access to all the helper methods
{
    public static class GlobalHealthChecks
    {
        private static IHealthCheckService _service;

        public static IHealthCheckService Service
        {
            get
            {
                Guard.OperationValid(_service != null, "You must call Build before retrieving the service.");

                return _service;
            }
        }

        public static void Build(Action<HealthCheckBuilder> buildout)
            => Build(buildout, null, null);

        public static void Build(Action<HealthCheckBuilder> buildout, IServiceProvider serviceProvider)
            => Build(buildout, serviceProvider, null);

        public static void Build(Action<HealthCheckBuilder> buildout, IServiceProvider serviceProvider, IServiceScopeFactory serviceScopeFactory)
        {
            Guard.ArgumentNotNull(nameof(buildout), buildout);
            Guard.OperationValid(_service == null, "You may only call Build once.");

            var builder = new HealthCheckBuilder();
            buildout(builder);

            _service = new HealthCheckService(builder, serviceProvider ?? new NoOpServiceProvider(), serviceScopeFactory);
        }

        class NoOpServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }
    }
}
