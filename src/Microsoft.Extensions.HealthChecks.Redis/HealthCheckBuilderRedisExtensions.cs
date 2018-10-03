using System;
using ServiceStack.Redis;
using System.Linq;
namespace Microsoft.Extensions.HealthChecks
{
    public static class HealthCheckBuilderRedisExtensions
    {
        public static HealthCheckBuilder AddRedisCheck(this HealthCheckBuilder builder, string name, string host, int port, string password = null)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            return AddRedisCheck(builder, name, builder.DefaultCacheDuration, host, port, password);
        }


        public static HealthCheckBuilder AddRedisCheck(this HealthCheckBuilder builder, string name, TimeSpan cacheDuration, string host, int port, string password = null)
        {
            builder.AddCheck($"RedisCheck({name})", () =>
            {
                try
                {
                    using (var client = new RedisClient(host, port, password))
                    {
                        var response = client.Info;

                        if (response != null && response.Any())
                        {
                            return HealthCheckResult.Healthy($"RedisCheck({name}): Healthy");
                        }
                        return HealthCheckResult.Unhealthy($"RedisCheck({name}): Unhealthy");

                    }
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"RedisCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
