// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.HealthChecks
{
    /// <summary>
    /// Represents a composite health check result built from several results.
    /// </summary>
    public class CompositeHealthCheckResult : IHealthCheckResult
    {
        private static readonly IReadOnlyDictionary<string, object> _emptyData = new Dictionary<string, object>();
        private readonly CheckStatus _initialStatus;
        private readonly CheckStatus _partiallyHealthyStatus;
        private readonly ConcurrentDictionary<string, IHealthCheckResult> _results = new ConcurrentDictionary<string, IHealthCheckResult>(StringComparer.OrdinalIgnoreCase);

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
                var checkStatuses = new HashSet<CheckStatus>(_results.Select(x => x.Value.CheckStatus));
                if (checkStatuses.Count == 0)
                    return _initialStatus;
                if (checkStatuses.Count == 1)
                    return checkStatuses.First();
                if (checkStatuses.Contains(CheckStatus.Healthy))
                    return _partiallyHealthyStatus;

                return CheckStatus.Unhealthy;
            }
        }

        public string Description => string.Join(Environment.NewLine, _results.Select(r => $"{r.Key}: {r.Value.Description}"));

        public IReadOnlyDictionary<string, object> Data
        {
            get
            {
                var result = new Dictionary<string, object>();

                foreach (var kvp in _results)
                    result.Add(kvp.Key, kvp.Value.Data);

                return result;
            }
        }

        public IReadOnlyDictionary<string, IHealthCheckResult> Results => _results;

        public void Add(string name, CheckStatus status, string description)
            => Add(name, status, description, null);

        public void Add(string name, CheckStatus status, string description, Dictionary<string, object> data)
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);
            Guard.ArgumentValid(status != CheckStatus.Unknown, nameof(status), "Cannot add unknown status to composite health check result");
            Guard.ArgumentNotNullOrWhitespace(nameof(description), description);

            if (!_results.TryAdd(name, HealthCheckResult.FromStatus(status, description, data)))
                throw new ArgumentException($"Check name {name} must be unique", nameof(name));
        }

        public void Add(string name, IHealthCheckResult checkResult)
        {
            Guard.ArgumentNotNullOrWhitespace(nameof(name), name);
            Guard.ArgumentNotNull(nameof(checkResult), checkResult);

            if (!_results.TryAdd(name, checkResult))
                throw new ArgumentException($"Check name {name} must be unique", nameof(name));
        }
    }
}
