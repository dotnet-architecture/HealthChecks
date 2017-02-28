using System;
using Microsoft.Extensions.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHealthChecks(this IServiceCollection services, Action<HealthCheckBuilder> checkupAction)
        {
            var checkupBuilder = new HealthCheckBuilder();

            checkupAction.Invoke(checkupBuilder);

            services.AddSingleton(checkupBuilder);
            services.AddSingleton<IHealthCheckService, HealthCheckService>();
            return services;
        }
    }
}
