// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Extensions.HealthChecks
{
    public class CompositeHealthCheckResultTest
    {
        public static List<object[]> AllLegalStatuses = Enum.GetValues(typeof(CheckStatus)).Cast<object>().Select(x => new[] { x }).ToList();
        public static List<object[]> AllLegalStatusesExceptUnknown = Enum.GetValues(typeof(CheckStatus)).Cast<CheckStatus>().Where(x => x != CheckStatus.Unknown).Select(x => new object[] { x }).ToList();

        [Fact]
        public void GuardClauses()
        {
            var classUnderTest = new CompositeHealthCheckResult();

            // Add(string, CheckStatus, string)
            Assert.Throws<ArgumentNullException>("name", () => classUnderTest.Add(null, CheckStatus.Healthy, "?"));
            Assert.Throws<ArgumentException>("name", () => classUnderTest.Add("", CheckStatus.Healthy, "?"));
            Assert.Throws<ArgumentException>("status", () => classUnderTest.Add("name", CheckStatus.Unknown, "?"));
            Assert.Throws<ArgumentNullException>("description", () => classUnderTest.Add("name", CheckStatus.Healthy, null));
            Assert.Throws<ArgumentException>("description", () => classUnderTest.Add("name", CheckStatus.Unhealthy, ""));

            // Add(string, IHealthCheckResult)
            var checkResult = HealthCheckResult.Healthy("Hello");
            Assert.Throws<ArgumentNullException>("name", () => classUnderTest.Add(null, checkResult));
            Assert.Throws<ArgumentException>("name", () => classUnderTest.Add("", checkResult));
            Assert.Throws<ArgumentNullException>("checkResult", () => classUnderTest.Add("name", null));
        }

        [Fact]
        public void NamesMustBeUnique()
        {
            var classUnderTest = new CompositeHealthCheckResult();
            classUnderTest.Add("name", HealthCheckResult.Healthy("healthy"));

            Assert.Throws<ArgumentException>(() => classUnderTest.Add("name", HealthCheckResult.Healthy("healthy")));
        }

        [Fact]
        public void CollectsStatusesToMakeThemAvailable()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Healthy);
            var warningData = new Dictionary<string, object> { { "Hello", "world" } };
            var unhealthyData = new Dictionary<string, object> { { "The answer", 42 } };
            classUnderTest.Add("0", CheckStatus.Warning, "Warning", warningData);
            classUnderTest.Add("1", CheckStatus.Unhealthy, "Unhealthy", unhealthyData);

            var results = classUnderTest.Results;

            Assert.Collection(results.OrderBy(kvp => kvp.Key),
                kvp =>
                {
                    Assert.Equal("0", kvp.Key);
                    Assert.Equal(CheckStatus.Warning, kvp.Value.CheckStatus);
                    Assert.Equal("Warning", kvp.Value.Description);
                    var innerKvp = Assert.Single(kvp.Value.Data);
                    Assert.Equal("Hello", innerKvp.Key);
                    Assert.Equal("world", innerKvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("1", kvp.Key);
                    Assert.Equal(CheckStatus.Unhealthy, kvp.Value.CheckStatus);
                    Assert.Equal("Unhealthy", kvp.Value.Description);
                    var innerKvp = Assert.Single(kvp.Value.Data);
                    Assert.Equal("The answer", innerKvp.Key);
                    Assert.Equal(42, innerKvp.Value);
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
        [MemberData(nameof(AllLegalStatusesExceptUnknown))]
        public void SingleValue_CheckStatusIsSetStatus_DescriptionIsDescription(CheckStatus status)
        {
            var classUnderTest = new CompositeHealthCheckResult();
            var description = $"Description for {status}";

            classUnderTest.Add("name", status, description);

            Assert.Equal(status, classUnderTest.CheckStatus);
            Assert.Equal($"name: {description}", classUnderTest.Description);
        }

        [Theory]
        [MemberData(nameof(AllLegalStatusesExceptUnknown))]
        public void MultipleValuesOfSameStatus_CheckStatusIsStatus_DescriptionIsComposite(CheckStatus status)
        {
            var classUnderTest = new CompositeHealthCheckResult();

            classUnderTest.Add("name1", status, "Description 1");
            classUnderTest.Add("name2", status, "Description 2");

            Assert.Equal(status, classUnderTest.CheckStatus);
            Assert.Contains($"name1: Description 1", classUnderTest.Description);
            Assert.Contains($"name2: Description 2", classUnderTest.Description);
        }

        [Fact]
        public void MultipleDifferentValuesWithHealthy_CheckStatusIsPartiallyHealthyStatus()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Warning);

            classUnderTest.Add("name1", CheckStatus.Healthy, "Healthy");
            classUnderTest.Add("name2", CheckStatus.Unhealthy, "Unhealthy");

            Assert.Equal(CheckStatus.Warning, classUnderTest.CheckStatus);
        }

        [Fact]
        public void MultipleDifferentValuesWithoutHealthy_CheckStatusIsUnhealthy()
        {
            var classUnderTest = new CompositeHealthCheckResult(partiallyHealthyStatus: CheckStatus.Healthy);

            classUnderTest.Add("name1", CheckStatus.Warning, "Warning");
            classUnderTest.Add("name2", CheckStatus.Unhealthy, "Unhealthy");

            Assert.Equal(CheckStatus.Unhealthy, classUnderTest.CheckStatus);
        }

        // REVIEW: Better way to represent this data? Or should it always be empty?
        [Fact]
        public void DataIsCompositeDictionary()
        {
            var classUnderTest = new CompositeHealthCheckResult();
            classUnderTest.Add("name1", CheckStatus.Healthy, "With data", new Dictionary<string, object> { { "Hello", "world" } });
            classUnderTest.Add("name2", CheckStatus.Healthy, "With data", new Dictionary<string, object> { { "The answer", 42 } });

            var returnedData = classUnderTest.Data;

            Assert.Collection(returnedData.OrderBy(x => x.Key),
                kvp =>
                {
                    Assert.Equal("name1", kvp.Key);
                    var dict = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(kvp.Value);
                    var elem = Assert.Single(dict);
                    Assert.Equal("Hello", elem.Key);
                    Assert.Equal("world", elem.Value);
                },
                kvp =>
                {
                    Assert.Equal("name2", kvp.Key);
                    var dict = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object>>(kvp.Value);
                    var elem = Assert.Single(dict);
                    Assert.Equal("The answer", elem.Key);
                    Assert.Equal(42, elem.Value);
                }
            );
        }
    }
}
