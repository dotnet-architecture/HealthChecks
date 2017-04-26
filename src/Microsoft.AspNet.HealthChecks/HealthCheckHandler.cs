// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;

namespace Microsoft.AspNet.HealthChecks
{
    public class HealthCheckHandler : HttpTaskAsyncHandler
    {
        public override bool IsReusable => true;

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var timeoutTokenSource = new CancellationTokenSource(GlobalHealthChecks.HandlerCheckTimeout);
            var result = await GlobalHealthChecks.Service.CheckHealthAsync(timeoutTokenSource.Token);
            var status = result.CheckStatus;

            if (status != CheckStatus.Healthy)
                context.Response.StatusCode = 503;

            context.Response.Headers.Add("content-type", "application/json");
            context.Response.Write(JsonConvert.SerializeObject(new { status = status.ToString() }));
        }
    }
}
