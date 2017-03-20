// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.HealthChecks;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.HealthChecks
{
    public class HealthCheckMiddleware
    {
        RequestDelegate _next;
        int _healthCheckPort;
        IHealthCheckService _checkupService;

        public HealthCheckMiddleware(RequestDelegate next, IHealthCheckService checkupService, int port)
        {
            _healthCheckPort = port;
            _checkupService = checkupService;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var connInfo = context.Features.Get<IHttpConnectionFeature>();
            if (connInfo.LocalPort == _healthCheckPort)
            {
                var result = await _checkupService.CheckHealthAsync();
                var status = result.CheckStatus;

                if (status != CheckStatus.Healthy)
                    context.Response.StatusCode = 503;

                context.Response.Headers.Add("content-type", "application/json");
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new { status = status.ToString() }));
                return;
            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}