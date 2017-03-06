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
        private readonly HashSet<CheckStatus> checkStatuses = new HashSet<CheckStatus>();
        private readonly List<string> descriptions = new List<string>();
        private readonly CheckStatus initialStatus;
        private readonly CheckStatus partiallyHealthyStatus;

        public CompositeHealthCheckResult(CheckStatus partiallyHealthyStatus = CheckStatus.Warning,
                                          CheckStatus initialStatus = CheckStatus.Unknown)
        {
            this.partiallyHealthyStatus = partiallyHealthyStatus;
            this.initialStatus = initialStatus;
        }

        public CheckStatus CheckStatus
        {
            get
            {
                if (checkStatuses.Count == 0)
                    return initialStatus;
                if (checkStatuses.Count == 1)
                    return checkStatuses.First();
                if (checkStatuses.Contains(CheckStatus.Healthy))
                    return partiallyHealthyStatus;

                return CheckStatus.Unhealthy;
            }
        }

        public string Description => string.Join(Environment.NewLine, descriptions);

        // REVIEW: Should description be required? Seems redundant for success checks.
        public void Add(CheckStatus status, string description)
        {
            Guard.ArgumentValid(status != CheckStatus.Unknown, nameof(status), "Cannot add unknown status to composite health check result");
            Guard.ArgumentNotNullOrWhitespace(nameof(description), description);

            checkStatuses.Add(status);
            descriptions.Add(description);
        }

        public void Add(IHealthCheckResult checkResult)
        {
            Guard.ArgumentNotNull(nameof(checkResult), checkResult);

            checkStatuses.Add(checkResult.CheckStatus);
            descriptions.Add(checkResult.Description);
        }
    }
}
