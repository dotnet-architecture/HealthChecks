using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Extensions.HealthChecks
{
    public interface IHealthCheckService
    {
        Task<bool> CheckHealthAsync();

        HealthCheckResults CheckResults { get; set; }
    }
}