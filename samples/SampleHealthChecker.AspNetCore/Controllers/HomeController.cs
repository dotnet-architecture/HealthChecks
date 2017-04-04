// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.HealthChecks;

namespace SampleHealthChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHealthCheckService _healthCheck;

        public HomeController(IHealthCheckService healthCheck)
        {
            _healthCheck = healthCheck;
        }

        public async Task<IActionResult> Index()
        {
            var timedTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var stopwatch = Stopwatch.StartNew();
            var checkResult = await _healthCheck.CheckHealthAsync(timedTokenSource.Token);
            ViewBag.ExecutionTime = stopwatch.Elapsed;
            return View(checkResult);
        }
    }
}
