using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks.Internal
{
    public class UrlChecker
    {
        private readonly Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> _checkFunc;
        private readonly string[] _urls;

        // REVIEW: Cache timeout here?
        public UrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc, params string[] urls)
        {
            Guard.ArgumentNotNull(nameof(checkFunc), checkFunc);
            Guard.ArgumentNotNullOrEmpty(nameof(urls), urls);

            _checkFunc = checkFunc;
            _urls = urls;
        }

        public CheckStatus PartiallyHealthyStatus { get; set; } = CheckStatus.Warning;

        public Task<IHealthCheckResult> CheckAsync()
            => _urls.Length == 1 ? CheckSingleAsync() : CheckMultiAsync();

        public async Task<IHealthCheckResult> CheckSingleAsync()
        {
            var httpClient = CreateHttpClient();
            var result = default(IHealthCheckResult);
            await CheckUrlAsync(httpClient, _urls[0], (_, checkResult) => result = checkResult);
            return result;
        }

        public async Task<IHealthCheckResult> CheckMultiAsync()
        {
            var composite = new CompositeHealthCheckResult(PartiallyHealthyStatus);
            var httpClient = CreateHttpClient();

            // REVIEW: Should these be done in parallel?
            foreach (var url in _urls)
                await CheckUrlAsync(httpClient, url, (name, checkResult) => composite.Add(name, checkResult));

            return composite;
        }

        private async Task CheckUrlAsync(HttpClient httpClient, string url, Action<string, IHealthCheckResult> adder)
        {
            var name = $"UrlCheck({url})";
            try
            {
                var response = await httpClient.GetAsync(url);
                var result = await _checkFunc(response);
                adder(name, result);
            }
            catch (Exception ex)
            {
                adder(name, HealthCheckResult.Unhealthy($"Exception during check: {ex.GetType().FullName}"));
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            return httpClient;
        }

        protected virtual HttpClient GetHttpClient()
            => new HttpClient();
    }
}
