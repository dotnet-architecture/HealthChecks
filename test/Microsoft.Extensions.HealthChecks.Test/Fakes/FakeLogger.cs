// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.HealthChecks.Fakes
{
    public class FakeLogger<TCategoryName> : ILogger<TCategoryName>
    {
        public List<string> Operations = new List<string>();

        public IDisposable BeginScope<TState>(TState state)
        {
            Operations.Add($"BeginScope: state={state?.ToString() ?? "(null)"}");
            return new Disposer(Operations);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Operations.Add($"Log: level={logLevel}, id={eventId.Id}, exception={exception?.ToString() ?? "(null)"}, message='{formatter(state, exception)}'");

        class Disposer : IDisposable
        {
            private readonly List<string> _operations;

            public Disposer(List<string> operations)
                => _operations = operations;

            public void Dispose()
                => _operations.Add("DisposeScope");
        }
    }
}
