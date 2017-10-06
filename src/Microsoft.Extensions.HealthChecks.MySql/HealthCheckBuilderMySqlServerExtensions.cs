using Microsoft.Extensions.HealthChecks;
using Pomelo.Data.MySql;
using System;

namespace Microsoft.Extensions.HealthChecks
{
    public static class HealthCheckBuilderMySqlServerExtensions
    {
        public static HealthCheckBuilder AddMySqlCheck(this HealthCheckBuilder builder, string name, string connectionString)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddMySqlCheck(builder, name, connectionString, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddMySqlCheck(this HealthCheckBuilder builder, string name, string connectionString, TimeSpan cacheDuration)
        {
            builder.AddCheck($"MySqlCheck({name})", async () =>
            {
                try
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var cmd = new MySqlCommand("SHOW STATUS", connection))
                        {
                            var result = await cmd.ExecuteScalarAsync();
                            if(result != null)
                            {
                                return HealthCheckResult.Healthy($"MySqlCheck({name}): Healthy");
                            }
                            return HealthCheckResult.Unhealthy($"MySqlCheck({name}): Unhealthy");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"MySqlCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
