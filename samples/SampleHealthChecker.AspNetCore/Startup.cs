// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.HealthChecks;
using Microsoft.Extensions.Logging;
using Couchbase;
using StackExchange.Redis;
using Nest;
using System.Linq;
using Elasticsearch.Net;
using Couchbase.Core;
using Couchbase.Management;

namespace SampleHealthChecker
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // When doing DI'd health checks, you must register them as services of their concrete type
            services.AddSingleton<CustomHealthCheck>();
            //services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(Configuration.GetValue<string>("REDIS_CLUSTER_SERVERS")));

            //RegisterCouchbase(services);
            //RegisterElastic(services);

            services.AddHealthChecks(checks =>
            {
                checks.AddUrlCheck("https://github.com")
                      .AddHealthCheckGroup(
                          "servers",
                          group => group.AddUrlCheck("https://google.com")
                                        .AddUrlCheck("https://twitddter.com")
                      )
                      .AddHealthCheckGroup(
                          "memory",
                          group => group.AddPrivateMemorySizeCheck(1)
                                        .AddVirtualMemorySizeCheck(2)
                                        .AddWorkingSetCheck(1),
                          CheckStatus.Unhealthy
                      )
                      .AddCheck("thrower", (Func<IHealthCheckResult>)(() => { throw new DivideByZeroException(); }))
                      .AddCheck("long-running", async cancellationToken => { await Task.Delay(10000, cancellationToken); return HealthCheckResult.Healthy("I ran too long"); })
                      .AddCheck<CustomHealthCheck>("custom");

               

                //checks.AddCouchbaseCheck(services,TimeSpan.FromSeconds(10));
                //checks.AddRedisCheck(services, TimeSpan.FromSeconds(10));
                //checks.AddElasticCheck(services, TimeSpan.FromSeconds(10));

                /*
                // add valid storage account credentials first
                checks.AddAzureBlobStorageCheck("accountName", "accountKey");
                checks.AddAzureBlobStorageCheck("accountName", "accountKey", "containerName");

                checks.AddAzureTableStorageCheck("accountName", "accountKey");
                checks.AddAzureTableStorageCheck("accountName", "accountKey", "tableName");

                checks.AddAzureFileStorageCheck("accountName", "accountKey");
                checks.AddAzureFileStorageCheck("accountName", "accountKey", "shareName");

                checks.AddAzureQueueStorageCheck("accountName", "accountKey");
                checks.AddAzureQueueStorageCheck("accountName", "accountKey", "queueName");
                */

            });

            services.AddMvc();
        }

        private void RegisterElastic(IServiceCollection services)
        {
            var nodes = new Uri[] { };

            if (Configuration != null)
            {
                var servers = Configuration.GetValue<string>("ELASTIC_CLUSTER_SERVERS");
                if (!string.IsNullOrEmpty(servers))
                {
                    nodes = (from u in servers.Split(',').ToList()
                              where Uri.IsWellFormedUriString(u, UriKind.RelativeOrAbsolute)
                              select new Uri(u)).ToArray();
                }
            }

            if (!nodes.Any())
            {
                return;
            }

            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);
            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }

        private void RegisterCouchbase(IServiceCollection services)
        {
            ClusterHelper.Initialize();

            var username = Configuration.GetValue<string>("COUCHBASE_CLUSTER_USER");
            var password = Configuration.GetValue<string>("COUCHBASE_CLUSTER_PASSWORD");

            services.AddSingleton<ICluster>(ClusterHelper.Get());
            services.AddSingleton<IClusterManager>(p => p.GetService<ICluster>().CreateManager(username, password));
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
