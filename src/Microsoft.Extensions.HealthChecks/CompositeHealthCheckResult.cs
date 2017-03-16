using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.HealthChecks
{
    // REVIEW: Does this need to be thread safe?
    /// <summary>
    /// Represents a composite health check result built from several results.
    /// </summary>
    public class CompositeHealthCheckResult : IHealthCheckResult
    {
        private static readonly IReadOnlyDictionary<string, object> _emptyData = new Dictionary<string, object>();
        private readonly CheckStatus _initialStatus;
        private readonly CheckStatus _partiallyHealthyStatus;
        private readonly List<IHealthCheckResult> _results = new List<IHealthCheckResult>();

        public CompositeHealthCheckResult(CheckStatus partiallyHealthyStatus = CheckStatus.Warning,
                                          CheckStatus initialStatus = CheckStatus.Unknown)
        {
            _partiallyHealthyStatus = partiallyHealthyStatus;
            _initialStatus = initialStatus;
        }

        public CheckStatus CheckStatus
        {
            get
            {
                var checkStatuses = new HashSet<CheckStatus>(_results.Select(x => x.CheckStatus));
                if (checkStatuses.Count == 0)
                    return _initialStatus;
                if (checkStatuses.Count == 1)
                    return _results.First().CheckStatus;
                if (checkStatuses.Contains(CheckStatus.Healthy))
                    return _partiallyHealthyStatus;

                return CheckStatus.Unhealthy;
            }
        }

        public string Description => string.Join(Environment.NewLine, _results.Select(r => r.Description));

        public IReadOnlyDictionary<string, object> Data
        {
            get
            {
                var result = new Dictionary<string, object>();
                var idx = 0;

                foreach (var dictionary in _results.Select(r => r.Data))
                    result.Add((idx++).ToString(), dictionary);

                return result;
            }
        }

        public IReadOnlyList<IHealthCheckResult> Results => _results;

        // REVIEW: Should description be required? Seems redundant for success checks.

        public void Add(CheckStatus status, string description)
            => Add(status, description, null);

        public void Add(CheckStatus status, string description, Dictionary<string, object> data)
        {
            Guard.ArgumentValid(status != CheckStatus.Unknown, nameof(status), "Cannot add unknown status to composite health check result");
            Guard.ArgumentNotNullOrWhitespace(nameof(description), description);

            _results.Add(HealthCheckResult.FromStatus(status, description, data));
        }

        public void Add(IHealthCheckResult checkResult)
        {
            Guard.ArgumentNotNull(nameof(checkResult), checkResult);

            _results.Add(checkResult);
        }
    }
}
