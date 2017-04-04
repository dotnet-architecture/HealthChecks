// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

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

            var innerBuilder = new HealthCheckBuilder();
            innerChecks(innerBuilder);

            builder.AddCheck($"Group({groupName})", async cancellationToken =>
            {
                var result = new CompositeHealthCheckResult(partiallyHealthyStatus);
                var checkResults = innerBuilder.Checks.Select(check => new { Name = check.Key, Operation = check.Value.CheckAsync(cancellationToken).AsTask() }).ToList();
                await Task.WhenAll(checkResults.Select(x => x.Operation)).ConfigureAwait(false);

                foreach (var checkResult in checkResults)
                {
                    result.Add(checkResult.Name, checkResult.Operation.Result);
                }

                return result;
            });

            return builder;
        }
    }
}
