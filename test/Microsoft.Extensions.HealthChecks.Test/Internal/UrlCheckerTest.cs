// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.HealthChecks.Internal
{
    public class UrlCheckerTest
    {
        [Fact]
        public void ConstructorGuardClauses()
        {
            Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc = response => default(ValueTask<IHealthCheckResult>);

            Assert.Throws<ArgumentNullException>("checkFunc", () => new UrlChecker(null, "https://url"));
            Assert.Throws<ArgumentNullException>("urls", () => new UrlChecker(checkFunc, null));
            Assert.Throws<ArgumentException>("urls", () => new UrlChecker(checkFunc, new string[0]));
        }

        public class CheckAsync
        {
            [Fact]
            public async void SingleUrl_CallsHttpClientWithNoCache_ReturnsCheckFunctionResult()
            {
                var response = new HttpResponseMessage();
                var checkResult = HealthCheckResult.Healthy("This is a healthy response");
                var checkFunc = Substitute.For<Func<HttpResponseMessage, ValueTask<IHealthCheckResult>>>();
                checkFunc(response).Returns(new ValueTask<IHealthCheckResult>(checkResult));
                var checker = new TestableUrlChecker(checkFunc, "http://url1/", response);

                var result = await checker.CheckAsync();

                Assert.Equal(CheckStatus.Healthy, checkResult.CheckStatus);
                Assert.Equal("This is a healthy response", checkResult.Description);
                Assert.Equal(HttpMethod.Get, response.RequestMessage.Method);
                Assert.Equal("http://url1/", response.RequestMessage.RequestUri.ToString());
                Assert.Equal(true, response.RequestMessage.Headers.CacheControl.NoCache);
            }

            [Theory]
            [InlineData(HttpStatusCode.OK, HttpStatusCode.OK, CheckStatus.Healthy)]
            [InlineData(HttpStatusCode.OK, HttpStatusCode.InternalServerError, CheckStatus.Warning)]
            [InlineData(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError, CheckStatus.Unhealthy)]
            public async void MultipleUrls_ReturnsCompositeResult(HttpStatusCode code1, HttpStatusCode code2, CheckStatus expectedStatus)
            {
                var response1 = new HttpResponseMessage { StatusCode = code1 };
                var response2 = new HttpResponseMessage { StatusCode = code2 };
                Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc =
                    response => new ValueTask<IHealthCheckResult>(HealthCheckResult.FromStatus(response.StatusCode == HttpStatusCode.OK ? CheckStatus.Healthy : CheckStatus.Unhealthy, $"{response.RequestMessage.RequestUri}: {response.StatusCode}"));
                var checker = new TestableUrlChecker(checkFunc, "http://url1/", response1, "http://url2/", response2);

                var result = await checker.CheckAsync();

                Assert.Equal(expectedStatus, result.CheckStatus);
                Assert.Equal($"http://url1/: {code1}{Environment.NewLine}http://url2/: {code2}", result.Description);
            }
        }

        public class DefaultUrlCheck
        {
            HttpResponseMessage response = new HttpResponseMessage
            {
                RequestMessage = new HttpRequestMessage { RequestUri = new Uri("http://uri/") },
                ReasonPhrase = "HTTP reason phrase",
                Content = new StringContent("This is the body content")
            };

            [Fact]
            public async void StatusCode200_ReturnsHealthy()
            {
                response.StatusCode = HttpStatusCode.OK;

                var result = await UrlChecker.DefaultUrlCheck(response);

                Assert.Equal(CheckStatus.Healthy, result.CheckStatus);
                Assert.Equal("UrlCheck(http://uri/): status code OK (200)", result.Description);
                Assert.Collection(result.Data.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value}'"),
                    value => Assert.Equal("'body' = 'This is the body content'", value),
                    value => Assert.Equal("'reason' = 'HTTP reason phrase'", value),
                    value => Assert.Equal("'status' = '200'", value),
                    value => Assert.Equal("'url' = 'http://uri/'", value)
                );
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]            // 1xx
            [InlineData(HttpStatusCode.NoContent)]           // 2xx
            [InlineData(HttpStatusCode.Moved)]               // 3xx
            [InlineData(HttpStatusCode.NotFound)]            // 4xx
            [InlineData(HttpStatusCode.ServiceUnavailable)]  // 5xx
            public async void StatusCodeNon200_ReturnsUnhealthy(HttpStatusCode statusCode)
            {
                response.StatusCode = statusCode;

                var result = await UrlChecker.DefaultUrlCheck(response);

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal($"UrlCheck(http://uri/): status code {statusCode} ({(int)statusCode})", result.Description);
                Assert.Collection(result.Data.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value}'"),
                    value => Assert.Equal("'body' = 'This is the body content'", value),
                    value => Assert.Equal("'reason' = 'HTTP reason phrase'", value),
                    value => Assert.Equal($"'status' = '{(int)statusCode}'", value),
                    value => Assert.Equal("'url' = 'http://uri/'", value)
                );
            }
        }

        class TestableUrlChecker : UrlChecker
        {
            private readonly Dictionary<string, HttpResponseMessage> _responses = new Dictionary<string, HttpResponseMessage>(StringComparer.OrdinalIgnoreCase);

            // Single URL
            public TestableUrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc,
                                      string url, HttpResponseMessage response)
                : base(checkFunc, new[] { url })
            {
                _responses.Add(url, response);
            }

            // Two URLs
            public TestableUrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc,
                                      string url1, HttpResponseMessage response1,
                                      string url2, HttpResponseMessage response2)
                : base(checkFunc, new[] { url1, url2 })
            {
                _responses.Add(url1, response1);
                _responses.Add(url2, response2);
            }

            protected override HttpClient GetHttpClient()
                => new HttpClient(new Handler(_responses));

            class Handler : HttpMessageHandler
            {
                private static readonly HttpResponseMessage _defaultResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
                private readonly Dictionary<string, HttpResponseMessage> _responses;

                public Handler(Dictionary<string, HttpResponseMessage> responses)
                {
                    _responses = responses;
                }

                public void Add(string uri, HttpResponseMessage response)
                    => _responses.Add(uri, response);

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    if (!_responses.TryGetValue(request.RequestUri.ToString(), out var response))
                        response = _defaultResponse;

                    response.RequestMessage = request;
                    return Task.FromResult(response);
                }
            }
        }
    }
}
