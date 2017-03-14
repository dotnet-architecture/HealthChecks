using System;
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

        public async Task<IHealthCheckResult> CheckAsync()
        {
            var composite = new CompositeHealthCheckResult(PartiallyHealthyStatus);
            var httpClient = GetHttpClient();
            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

            // REVIEW: Should these be done in parallel?
            foreach (var url in _urls)
            {
                var response = await httpClient.GetAsync(url);
                var result = await _checkFunc(response);
                composite.Add(result);
            }

            return composite;
        }

        protected virtual HttpClient GetHttpClient()
            => new HttpClient();
    }
}
