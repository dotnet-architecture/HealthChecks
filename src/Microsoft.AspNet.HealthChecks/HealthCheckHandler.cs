// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;

namespace Microsoft.AspNet.HealthChecks
{
    public class HealthCheckHandler : HttpTaskAsyncHandler
    {
        private static TimeSpan _timeout = TimeSpan.FromSeconds(10);

        public override bool IsReusable => true;

        public static TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                Guard.ArgumentValid(value > TimeSpan.Zero, nameof(Timeout), "Health check timeout must be a positive time span.");

                _timeout = value;
            }
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var timeoutTokenSource = new CancellationTokenSource(Timeout);
            var result = await GlobalHealthChecks.Service.CheckHealthAsync(timeoutTokenSource.Token);
            var status = result.CheckStatus;

            if (status != CheckStatus.Healthy)
            {
                context.Response.StatusCode = 503;
            }

            context.Response.Headers.Add("content-type", "application/json");
            context.Response.Write(JsonConvert.SerializeObject(new { status = status.ToString() }));
        }
    }
}
