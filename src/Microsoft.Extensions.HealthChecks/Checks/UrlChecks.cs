// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.HealthChecks.Internal;

namespace Microsoft.Extensions.HealthChecks
{
    public static partial class HealthCheckBuilderExtensions
    {
        // URL checks

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url)
            => AddUrlCheck(builder, url, response => DefaultUrlCheck(response));

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, IHealthCheckResult> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlCheck(builder, url, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, Task<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlCheck(builder, url, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlCheck(this HealthCheckBuilder builder, string url,
                                                     Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrWhitespace(nameof(url), url);
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            var urlCheck = new UrlChecker(checkFunc, url);
            builder.AddCheck($"UrlCheck({url})", () => urlCheck.CheckAsync());
            return builder;
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName)
            => AddUrlChecks(builder, urlItems, groupName, CheckStatus.Warning, response => DefaultUrlCheck(response));

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      Func<HttpResponseMessage, IHealthCheckResult> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlChecks(builder, urlItems, groupName, CheckStatus.Warning, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      Func<HttpResponseMessage, Task<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlChecks(builder, urlItems, groupName, CheckStatus.Warning, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlChecks(builder, urlItems, groupName, CheckStatus.Warning, response => checkFunc(response));
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      CheckStatus partialSuccessStatus)
            => AddUrlChecks(builder, urlItems, groupName, partialSuccessStatus, response => DefaultUrlCheck(response));

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      CheckStatus partialSuccessStatus, Func<HttpResponseMessage, IHealthCheckResult> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlChecks(builder, urlItems, groupName, partialSuccessStatus, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      CheckStatus partialSuccessStatus, Func<HttpResponseMessage, Task<IHealthCheckResult>> checkFunc)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);

            return AddUrlChecks(builder, urlItems, groupName, partialSuccessStatus, response => new ValueTask<IHealthCheckResult>(checkFunc(response)));
        }

        public static HealthCheckBuilder AddUrlChecks(this HealthCheckBuilder builder, IEnumerable<string> urlItems, string groupName,
                                                      CheckStatus partialSuccessStatus, Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc)
        {
            var urls = urlItems?.ToArray();

            Guard.ArgumentNotNull(nameof(builder), builder);
            Guard.ArgumentNotNullOrEmpty(nameof(urlItems), urls);
            Guard.ArgumentNotNullOrWhitespace(nameof(groupName), groupName);

            var urlChecker = new UrlChecker(checkFunc, urls) { PartiallyHealthyStatus = partialSuccessStatus };
            builder.AddCheck($"UrlChecks({groupName})", () => urlChecker.CheckAsync());
            return builder;
        }

        // Helpers

        private static async ValueTask<IHealthCheckResult> DefaultUrlCheck(HttpResponseMessage response)
        {
            // REVIEW: Should this be an explicit 200 check, or just an "is success" check?
            var status = response.StatusCode == HttpStatusCode.OK ? CheckStatus.Healthy : CheckStatus.Unhealthy;
            var data = new Dictionary<string, object>
            {
                { "url", response.RequestMessage.RequestUri.ToString() },
                { "status", (int)response.StatusCode },
                { "reason", response.ReasonPhrase },
                { "body", await response.Content?.ReadAsStringAsync() }
            };
            return HealthCheckResult.FromStatus(status, $"UrlCheck({response.RequestMessage.RequestUri}): status code {response.StatusCode} ({(int)response.StatusCode})", data);
        }
    }
}
