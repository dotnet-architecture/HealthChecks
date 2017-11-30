using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class ElasticHealthCheck : IHealthCheck
    {
        public const string Tag = "Elastic";

        private string _message { get; set; }

        private readonly IElasticClient _connection;

        private readonly ILogger _logger;

        public ElasticHealthCheck(IElasticClient connection, ILoggerFactory loggerFactory)
        {
            _connection = connection;
            _logger = loggerFactory?.CreateLogger<ElasticHealthCheck>();
        }


        public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            => new ValueTask<IHealthCheckResult>(Check(cancellationToken));


        private async Task<IHealthCheckResult> Check(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = HealthCheckResult.FromStatus(CheckStatus.Unknown, "Unknown");

            try
            {
                var clusterHealth = await _connection.ClusterHealthAsync(new ClusterHealthRequest(), cancellationToken);
                switch (clusterHealth?.Status)
                {
                    case "green":
                        return HealthCheckResult.FromStatus(CheckStatus.Healthy, "Healthy");
                    case "yellow":
                        return HealthCheckResult.FromStatus(CheckStatus.Warning, "Warning");
                    case "red":
                        return HealthCheckResult.FromStatus(CheckStatus.Unhealthy, "Unhealthy");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(new EventId(), ex, ex.Message);
                result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, ex.Message);
            }

            return result;
        }
    }
}
