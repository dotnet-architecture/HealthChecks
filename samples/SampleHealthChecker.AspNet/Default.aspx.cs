// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;
using Microsoft.Extensions.HealthChecks;

namespace SampleHealthChecker.AspNet
{
    public partial class Default : Page
    {
        public CompositeHealthCheckResult CheckResult;

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterAsyncTask(new PageAsyncTask(GetChecksAsync));
        }

        private async Task GetChecksAsync(CancellationToken cancellationToken)
        {
            var timedTokenSource = new CancellationTokenSource(GlobalHealthChecks.HandlerCheckTimeout);
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timedTokenSource.Token);

            CheckResult = await GlobalHealthChecks.Service.CheckHealthAsync(linkedTokenSource.Token);
        }
    }
}
