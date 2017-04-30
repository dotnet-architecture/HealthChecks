// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.HealthChecks;
using Microsoft.Extensions.HealthChecks;

namespace SampleHealthChecker.AspNet
{
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
}
