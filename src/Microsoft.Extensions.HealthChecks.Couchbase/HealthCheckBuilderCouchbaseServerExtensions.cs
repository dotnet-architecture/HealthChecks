// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Couchbase;
using Couchbase.Management;
using System;
using System.Linq;
using System.Net.Http;

namespace Microsoft.Extensions.HealthChecks
{
    // REVIEW: What are the appropriate guards for these functions?

    public static class HealthCheckBuilderCouchbaseServerExtensions
    {
        public static HealthCheckBuilder AddCouchbaseCheck(this HealthCheckBuilder builder, string username, string password)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddCouchbaseCheck(builder, username, password, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddCouchbaseCheck(this HealthCheckBuilder builder, string username, string password, TimeSpan cacheDuration)
        {
            builder.AddCheck($"CouchbaseCheck({username})", async () =>
            {
                var result = HealthCheckResult.Healthy($"CouchbaseCheck({username}): Healthy");

                if (!ClusterHelper.Initialized)
                {
                    return HealthCheckResult.Unhealthy($"CouchbaseCheck({username}): Unhealthy");
                }
                    try
                    {
                        var cluster = ClusterHelper.Get();
                        if (cluster == null)
                        {

                            return HealthCheckResult.Unhealthy($"CouchbaseCheck({username}): Unhealthy");
                        }

                        var manager = cluster.CreateManager(username, password);
                        if (manager == null)
                        {
                            return HealthCheckResult.Unhealthy($"CouchbaseCheck({username}): Unhealthy");
                        }

                        var buckets = await manager.ListBucketsAsync();
                        if (!buckets.Success)
                        {
                            return HealthCheckResult.Unhealthy($"CouchbaseCheck({username}): {buckets.Message}");

                        }

                        foreach (var bucket in buckets.Value)
                        {
                            foreach (var node in bucket.Nodes)
                            {
                                if (node.Status != "healthy")
                                {
                                    return HealthCheckResult.Unhealthy($"CouchbaseCheck({username}): Healthy");
                                }
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                    result = HealthCheckResult.Unhealthy(ex.Message);
                    }
                

                return result;

            }, cacheDuration);

            return builder;
        }
    }
}
