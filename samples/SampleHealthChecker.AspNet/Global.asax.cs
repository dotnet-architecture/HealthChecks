// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.HealthChecks;

namespace SampleHealthChecker.AspNet
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            GlobalHealthChecks.Builder
                .WithDefaultCacheDuration(TimeSpan.FromMinutes(1))
                .AddUrlCheck("https://github.com")
                .AddPrivateMemorySizeCheck(1)
                .AddVirtualMemorySizeCheck(2)
                .AddWorkingSetCheck(1)
                .AddUrlChecks(new List<string> { "https://github.com", "https://google.com", "https://twitddter.com" }, "servers");
        }
    }
}
