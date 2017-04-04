// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.HealthChecks
{
    public static partial class HealthCheckBuilderExtensions
    {
        public static HealthCheckBuilder AddHealthCheckGroup(this HealthCheckBuilder builder, string groupName,
                                                             Action<HealthCheckBuilder> innerChecks)
            => AddHealthCheckGroup(builder, groupName, innerChecks, CheckStatus.Warning);

        public static HealthCheckBuilder AddHealthCheckGroup(this HealthCheckBuilder builder, string groupName,
                                                             Action<HealthCheckBuilder> innerChecks,
                                                             CheckStatus partiallyHealthyStatus)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrWhitespace(nameof(groupName), groupName);
            Guard.ArgumentNotNull(nameof(innerChecks), innerChecks);

            var innerBuilder = new HealthCheckBuilder(builder.ServiceCollection);
            innerChecks(innerBuilder);

            builder.AddCheck($"Group({groupName})", cancellationToken => CheckExecutor.RunChecksAsync(innerBuilder.Checks, partiallyHealthyStatus, cancellationToken));

            return builder;
        }
    }
}
