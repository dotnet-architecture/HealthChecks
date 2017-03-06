namespace Microsoft.Extensions.HealthChecks
{
    public interface IHealthCheckResult
    {
        CheckStatus CheckStatus { get; }
        string Description { get; }
    }
}
