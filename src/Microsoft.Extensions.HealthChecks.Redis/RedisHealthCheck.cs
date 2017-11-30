using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        public const string Tag = "Redis";

        private string _message { get; set; }
        
        private readonly IConnectionMultiplexer _connection;

        private readonly ILogger _logger;
        
        public RedisHealthCheck(IConnectionMultiplexer connection, ILoggerFactory loggerFactory)
        {
            _connection = connection;

            if (_connection != null)
            {
                _connection.ConnectionFailed += _connection_ConnectionFailed;
                _connection.ConnectionRestored += _connection_ConnectionRestored;
            }

            _logger = loggerFactory?.CreateLogger<RedisHealthCheck>();
        }

        private void _connection_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            _message = $"{e.FailureType}";
            if (e.Exception != null)
            {
                _logger?.LogError(new EventId(), e.Exception, _message);
            }
        }

        private void _connection_ConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            if (e?.FailureType == ConnectionFailureType.None)
            {
                _message = null;
                _logger?.LogInformation(new EventId(), $"Connection restored on {e.EndPoint}");
            }
        }

        public ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
            => new ValueTask<IHealthCheckResult>(Check());
        

        private Task<IHealthCheckResult> Check()
        {
            IHealthCheckResult result = HealthCheckResult.FromStatus(CheckStatus.Unknown, "Unknown");
            if (_connection?.IsConnected != true)
            {
                result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, _message ?? "Not connected");
            }

            try
            {
                var status = _connection.GetStatus();
                if (!status.Contains("int: ConnectedEstablished"))
                {
                    result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, status);
                }
                else
                {
                    result = HealthCheckResult.FromStatus(CheckStatus.Healthy, "OK");
                }
            }
            catch (RedisException ex)
            {
                result = HealthCheckResult.FromStatus(CheckStatus.Unhealthy, ex.Message);
            }

            return Task.FromResult(result);
        }
    }
}
