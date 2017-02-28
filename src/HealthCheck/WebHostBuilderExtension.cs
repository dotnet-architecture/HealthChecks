using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder UseHealthChecks(this IWebHostBuilder builder, int port)
        {
            builder.ConfigureServices(services =>
            {
                var existingUrl = builder.GetSetting(WebHostDefaults.ServerUrlsKey);
                builder.UseSetting(WebHostDefaults.ServerUrlsKey, $"{existingUrl};http://localhost:{port}");

                services.AddSingleton<IStartupFilter>(new HealthCheckStartupFilter(port));
            });
            return builder;
        }
    }
}
