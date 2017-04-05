// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.HealthChecks  // Put this in Extensions so you also have access to all the helper methods
{
    public class GlobalHealthChecks
    {
        static GlobalHealthChecks()
        {
            Builder = new HealthCheckBuilder();
            HandlerCheckTimeout = TimeSpan.FromSeconds(10);
            Service = new HealthCheckService(Builder);
        }

        public static HealthCheckBuilder Builder { get; }

        public static TimeSpan HandlerCheckTimeout { get; private set; }

        public static IHealthCheckService Service { get; }

        public static void SetHandlerCheckTimeout(TimeSpan timeout)
        {
            Guard.ArgumentValid(timeout > TimeSpan.Zero, nameof(timeout), "Health check timeout must be a positive time span");

            HandlerCheckTimeout = timeout;
        }
    }
}
