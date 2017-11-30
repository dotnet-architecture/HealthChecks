using Couchbase;
using Couchbase.Configuration.Server.Serialization;
using Couchbase.Core;
using Couchbase.Management;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class CouchbaseHealthCheck : IHealthCheck
    {
        public const string Tag = "Couchbase Server";

        private string _message { get; set; }

        private readonly IClusterManager _connection;

        private readonly ILogger _logger;

        private readonly string _username;

        private readonly string _password;

        public CouchbaseHealthCheck(IClusterManager connection, ILoggerFactory loggerFactory)
        {
            _connection = connection;
            _logger = loggerFactory?.CreateLogger<CouchbaseHealthCheck>();
        }


        public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            => new ValueTask<IHealthCheckResult>(Check(cancellationToken));


        private async Task<IHealthCheckResult> Check(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = HealthCheckResult.FromStatus(CheckStatus.Unknown, "Unknown");
            
         
            if (!ClusterHelper.Initialized || _connection == null)
            {
                return HealthCheckResult.FromStatus(CheckStatus.Unknown, $"Cluster not initialized");
            }

            try
            {
                var buckets = await _connection.ListBucketsAsync();
                if (!buckets.Success)
                {
                    return HealthCheckResult.FromStatus(CheckStatus.Unknown, "Could not list any buckets");

                }

                var unhealthy = from n in buckets?.Value?.SelectMany(x => x.Nodes) ?? Enumerable.Empty<Node>()
                                where n != null && n.Status != "healthy"
                                select n;

                if (unhealthy.Any())
                {
                    result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, string.Join("\n", unhealthy.Select(x => $"{x.Hostname} : {x.Status}")));
                }
                else
                {
                    result = HealthCheckResult.FromStatus(CheckStatus.Healthy, "OK");
                }
            }
            catch (HttpRequestException ex)
            {
                result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, ex.Message);
                _logger?.LogError(new EventId(), ex, ex.Message);
            }


            return result;
        }
    }
}
