// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.HealthChecks.Fakes
{
    class FakeServiceProvider : IServiceProvider
    {
        private readonly FakeServiceScopeFactory _scopeFactory;

        public FakeServiceProvider() => _scopeFactory = new FakeServiceScopeFactory(this);

        public List<string> Operations => _scopeFactory.Operations;

        public IServiceScopeFactory ScopeFactory => _scopeFactory;

        public object GetService(Type serviceType)
        {
            Operations.Add($"Resolved type '{serviceType.FullName}'");
            return Activator.CreateInstance(serviceType);
        }

        class FakeServiceScopeFactory : IServiceScopeFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public FakeServiceScopeFactory(IServiceProvider serviceProvider)
                => _serviceProvider = serviceProvider;

            public List<string> Operations = new List<string>();

            public IServiceScope CreateScope() => new FakeServiceScope(_serviceProvider, Operations);
        }

        class FakeServiceScope : IServiceScope
        {
            private readonly List<string> _operations;
            private readonly IServiceProvider _serviceProvider;

            public FakeServiceScope(IServiceProvider serviceProvider, List<string> operations)
            {
                _serviceProvider = serviceProvider;
                _operations = operations;

                operations.Add("Scope created");
            }

            public IServiceProvider ServiceProvider => _serviceProvider;

            public void Dispose() => _operations.Add("Scope disposed");
        }
    }
}
