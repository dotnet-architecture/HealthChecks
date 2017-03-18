// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class HealthCheckBuilder
    {
        public Dictionary<string, Func<ValueTask<IHealthCheckResult>>> Checks { get; private set; }

        public HealthCheckBuilder()
        {
            Checks = new Dictionary<string, Func<ValueTask<IHealthCheckResult>>>();
        }

        public HealthCheckBuilder AddCheck(string name, Func<IHealthCheckResult> check)
        {
            Guard.ArgumentNotNull(nameof(check), check);

            return AddValueTaskCheck(name, () => new ValueTask<IHealthCheckResult>(check()));
        }

        public HealthCheckBuilder AddCheck(string name, Func<Task<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNull(nameof(check), check);

            return AddValueTaskCheck(name, () => new ValueTask<IHealthCheckResult>(check()));
        }

        public HealthCheckBuilder AddValueTaskCheck(string name, Func<ValueTask<IHealthCheckResult>> check)
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);
            Guard.ArgumentNotNull(nameof(check), check);

            Checks.Add(name, check);
            return this;
        }

        // REVIEW: This is clearly not the right API, but it'll suffice for now for the purposes of testing
        public Func<ValueTask<IHealthCheckResult>> GetCheck(string name)
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);

            return Checks.TryGetValue(name, out Func<ValueTask<IHealthCheckResult>> result) ? result : null;
        }
    }
}