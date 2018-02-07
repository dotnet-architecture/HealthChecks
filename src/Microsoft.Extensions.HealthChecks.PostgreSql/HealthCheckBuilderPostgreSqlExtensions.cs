using System;
using System.Data;
using Npgsql;

namespace Microsoft.Extensions.HealthChecks.PostgreSql
{
    public static class HealthCheckBuilderPostgreSqlExtensions
    {
        public static HealthCheckBuilder AddPostgreSqlCheck(this HealthCheckBuilder builder, string name, string connectionString)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return AddPostgreSqlCheck(builder, name, connectionString, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddPostgreSqlCheck(this HealthCheckBuilder builder, string name, string connectionString, TimeSpan cacheDuration)
        {
            builder.AddCheck($"PostgreSqlCheck({name})", async () =>
            {
                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT 1";
                            var result = (int)await command.ExecuteScalarAsync().ConfigureAwait(false);
                            if (result == 1)
                            {
                                return HealthCheckResult.Healthy($"PostgreSqlCheck({name}): Healthy");
                            }

                            return HealthCheckResult.Unhealthy($"PostgreSqlCheck({name}): Unhealthy");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"PostgreSqlCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
