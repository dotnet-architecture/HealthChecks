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

        [Fact]
        public void CollectsStatusesToMakeThemAvailable()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Healthy);
            var warningData = new Dictionary<string, object> { { "Hello", "world" } };
            var unhealthyData = new Dictionary<string, object> { { "The answer", 42 } };
            classUnderTest.Add(CheckStatus.Warning, "Warning", warningData);
            classUnderTest.Add(CheckStatus.Unhealthy, "Unhealthy", unhealthyData);

            var results = classUnderTest.Results;

            Assert.Collection(results,
                result =>
                {
                    Assert.Equal(CheckStatus.Warning, result.CheckStatus);
                    Assert.Equal("Warning", result.Description);
                    var kvp = Assert.Single(result.Data);
                    Assert.Equal("Hello", kvp.Key);
                    Assert.Equal("world", kvp.Value);
                },
                result =>
                {
                    Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                    Assert.Equal("Unhealthy", result.Description);
                    var kvp = Assert.Single(result.Data);
                    Assert.Equal("The answer", kvp.Key);
                    Assert.Equal(42, kvp.Value);
                }
            );
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

        // REVIEW: Better way to represent this data? Or should it always be empty?
        [Fact]
        public void DataIsCompositeDictionary()
        {
            var classUnderTest = new CompositeHealthCheckResult();
            classUnderTest.Add(CheckStatus.Healthy, "With data", new Dictionary<string, object> { { "Hello", "world" } });
            classUnderTest.Add(CheckStatus.Healthy, "With data", new Dictionary<string, object> { { "The answer", 42 } });

            var returnedData = classUnderTest.Data;

            Assert.Collection(returnedData,
                kvp =>
                {
                    Assert.Equal("0", kvp.Key);
                    var dict = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(kvp.Value);
                    var elem = Assert.Single(dict);
                    Assert.Equal("Hello", elem.Key);
                    Assert.Equal("world", elem.Value);
                },
                kvp =>
                {
                    Assert.Equal("1", kvp.Key);
                    var dict = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(kvp.Value);
                    var elem = Assert.Single(dict);
                    Assert.Equal("The answer", elem.Key);
                    Assert.Equal(42, elem.Value);
                }
            );
        }
    }
}
