using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public abstract class CachedHealthCheck : IHealthCheck
    {
        private DateTimeOffset _cacheExpiration;
        private volatile int _writerCount;

        protected CachedHealthCheck(TimeSpan cacheDuration)
        {
            Guard.ArgumentValid(cacheDuration >= TimeSpan.Zero, nameof(cacheDuration), "Cache duration must either be zero (disabled) or a positive value");

            CacheDuration = cacheDuration;
        }

        protected IHealthCheckResult CachedResult { get; private set; }

        public virtual TimeSpan CacheDuration { get; }

        protected virtual DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public async ValueTask<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken)
        {
            while (_cacheExpiration <= UtcNow)
            {
                // Can't use a standard lock here because of async, so we'll use this flag to determine when we should write a value,
                // and the waiters who aren't allowed to write will just spin wait for the new value.
                if (Interlocked.Exchange(ref _writerCount, 1) != 0)
                {
                    await Task.Delay(5, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                CachedResult = await CheckExecutor.RunCheckAsync(ExecuteCheckAsync, cancellationToken);
                _cacheExpiration = UtcNow + CacheDuration;
                _writerCount = 0;
                break;
            }

            return CachedResult;
        }

        /// <summary>
        /// Override to provide the health check implementation. The results will
        /// automatically be cached based on <see cref="CacheDuration"/>, and if
        /// needed, the previously cached value is available via <see cref="CachedResult"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected abstract ValueTask<IHealthCheckResult> ExecuteCheckAsync(CancellationToken cancellationToken);
    }
}
