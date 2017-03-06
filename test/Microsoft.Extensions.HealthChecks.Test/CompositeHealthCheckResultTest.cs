using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class CompositeHealthCheckResultTest
    {
        public static List<object[]> AllLegalStatuses = Enum.GetValues(typeof(CheckStatus)).Cast<object>().Select(x => new[] { x }).ToList();
        public static List<object[]> AllLegalStatusesForAdd = Enum.GetValues(typeof(CheckStatus)).Cast<CheckStatus>().Where(x => x != CheckStatus.Unknown).Select(x => new object[] { x }).ToList();

        [Fact]
        public void GuardClauses()
        {
            var classUnderTest = new CompositeHealthCheckResult();

            // Add(CheckStatus, string)
            Assert.Throws<ArgumentException>("status", () => classUnderTest.Add(CheckStatus.Unknown, "?"));
            Assert.Throws<ArgumentNullException>("description", () => classUnderTest.Add(CheckStatus.Healthy, null));
            Assert.Throws<ArgumentException>("description", () => classUnderTest.Add(CheckStatus.Unhealthy, " "));

            // Add(IHealthCheckResult)
            Assert.Throws<ArgumentNullException>("checkResult", () => classUnderTest.Add(null));
        }

        [Theory]
        [MemberData(nameof(AllLegalStatuses))]
        public void EmptyComposite_CheckStatusIsInitialStatus_DescriptionIsEmpty(CheckStatus initialStatus)
        {
            var classUnderTest = new CompositeHealthCheckResult(initialStatus: initialStatus);

            Assert.Equal(initialStatus, classUnderTest.CheckStatus);
            Assert.Empty(classUnderTest.Description);
        }

        [Theory]
        [MemberData(nameof(AllLegalStatusesForAdd))]
        public void SingleValue_CheckStatusIsSetStatus_DescriptionIsDescription(CheckStatus status)
        {
            var classUnderTest = new CompositeHealthCheckResult();
            var description = $"Description for {status}";

            classUnderTest.Add(status, description);

            Assert.Equal(status, classUnderTest.CheckStatus);
            Assert.Equal(description, classUnderTest.Description);
        }

        [Theory]
        [MemberData(nameof(AllLegalStatusesForAdd))]
        public void MultipleValuesOfSameStatus_CheckStatusIsStatus_DescriptionIsComposite(CheckStatus status)
        {
            var classUnderTest = new CompositeHealthCheckResult();

            classUnderTest.Add(status, "Description 1");
            classUnderTest.Add(status, "Description 2");

            Assert.Equal(status, classUnderTest.CheckStatus);
            Assert.Equal($"Description 1{Environment.NewLine}Description 2", classUnderTest.Description);
        }

        [Fact]
        public void MultipleDifferentValuesWithHealthy_CheckStatusIsPartiallyHealthyStatus()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Warning);

            classUnderTest.Add(CheckStatus.Healthy, "Healthy");
            classUnderTest.Add(CheckStatus.Unhealthy, "Unhealthy");

            Assert.Equal(CheckStatus.Warning, classUnderTest.CheckStatus);
        }

        [Fact]
        public void MultipleDifferentValuesWithoutHealthy_CheckStatusIsUnhealthy()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Healthy);

            classUnderTest.Add(CheckStatus.Warning, "Warning");
            classUnderTest.Add(CheckStatus.Unhealthy, "Unhealthy");

            Assert.Equal(CheckStatus.Unhealthy, classUnderTest.CheckStatus);
        }
    }
}
