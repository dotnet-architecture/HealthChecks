Health checks for building services  [![Build status](https://ci.appveyor.com/api/projects/status/nyvfn5yb8g623rt3?svg=true)](https://ci.appveyor.com/project/seven1986/healthchecks) 
===

This project is part of ASP.NET Core. You can find samples, documentation and getting started instructions for ASP.NET Core at the [Home](https://github.com/aspnet/home) repo.

Project | NuGet| Used For 
--------------- | --------------- 
Microsoft.AspNet.HealthChecks|[![NuGet downloads Microsoft.AspNet.HealthChecks](https://img.shields.io/nuget/dt/Microsoft.AspNet.HealthChecks.svg)](https://www.nuget.org/packages/Microsoft.AspNet.HealthChecks) | AspNet
Microsoft.AspNetCore.HealthChecks|[![NuGet downloads Microsoft.AspNetCore.HealthChecks](https://img.shields.io/nuget/dt/Microsoft.AspNetCore.HealthChecks.svg)](https://www.nuget.org/packages/Microsoft.AspNetCore.HealthChecks) | AspNetCore
Microsoft.Extensions.HealthChecks|[![NuGet downloads Microsoft.Extensions.HealthChecks](https://img.shields.io/nuget/dt/Microsoft.Extensions.HealthChecks.svg)](https://www.nuget.org/packages/Microsoft.Extensions.HealthChecks) | AspNetCore
Microsoft.Extensions.HealthChecks.AzureStorage|[![NuGet downloads Microsoft.Extensions.HealthChecks.AzureStorage](https://img.shields.io/nuget/dt/Microsoft.Extensions.HealthChecks.AzureStorage.svg)](https://www.nuget.org/packages/Microsoft.Extensions.HealthChecks.AzureStorage) | AspNetCore
Microsoft.Extensions.HealthChecks.SqlServer|[![NuGet downloads Microsoft.Extensions.HealthChecks.SqlServer](https://img.shields.io/nuget/dt/Microsoft.Extensions.HealthChecks.SqlServer.svg)](https://www.nuget.org/packages/Microsoft.Extensions.HealthChecks.SqlServer) | AspNetCore

#### for your AspNet Project
```
Install-Package Microsoft.AspNet.HealthChecks
```

```csharp
//Global.cs
public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            HealthCheckHandler.Timeout = TimeSpan.FromSeconds(3);

            GlobalHealthChecks.Build(builder =>
                builder.WithDefaultCacheDuration(TimeSpan.FromMinutes(1))
                       .AddUrlCheck("https://github.com")
                       .AddHealthCheckGroup(
                           "servers",
                           group => group.AddUrlCheck("https://google.com")
                                         .AddUrlCheck("https://twitddter.com")
                       )
                       .AddHealthCheckGroup(
                           "memory",
                           group => group.AddPrivateMemorySizeCheck(1)
                                         .AddVirtualMemorySizeCheck(2)
                                         .AddWorkingSetCheck(1)
                       )
                       .AddCheck("thrower", (Func<IHealthCheckResult>)(() => { throw new DivideByZeroException(); }))
                       .AddCheck("long-running", async cancellationToken => { await Task.Delay(10000, cancellationToken); return HealthCheckResult.Healthy("I ran too long"); })
            );
        }
    }
```



### for your AspNetCore Project
```
Install-Package Microsoft.AspNetCore.HealthChecks
```

```csharp
//Program.cs
 public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseHealthChecks("/health", TimeSpan.FromSeconds(3))     // Or to host on a separate port: .UseHealthChecks(port)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
```

###  Group¡¢Custom health check
```
Install-Package Microsoft.Extensions.HealthChecks
```

```csharp
// Startup.cs
 public void ConfigureServices(IServiceCollection services)
        {
            // When doing DI'd health checks, you must register them as services of their concrete type
            services.AddSingleton<CustomHealthCheck>();

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
            
                // Install-Package Microsoft.Extensions.HealthChecks.AzureStorage
                // Install-Package Microsoft.Extensions.HealthChecks.SqlServer
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

```