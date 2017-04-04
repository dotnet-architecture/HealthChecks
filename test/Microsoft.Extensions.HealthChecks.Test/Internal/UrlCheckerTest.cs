// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
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
            Assert.Throws<ArgumentNullException>("url", () => new UrlChecker(checkFunc, null));
            Assert.Throws<ArgumentException>("url", () => new UrlChecker(checkFunc, " "));
        }

        public class CheckAsync
        {
            [Fact]
            public async void CallsHttpClientWithNoCache_ReturnsCheckFunctionResult()
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

            [Fact]
            public async void ReturnedDataIncludesUrlWhenExceptionIsThrown()
            {
                var exception = new DivideByZeroException();
                var checker = new TestableUrlChecker(exception, "http://uri/");

                var result = await checker.CheckAsync();

                Assert.Collection(result.Data.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value}'"),
                    value => Assert.Equal("'url' = 'http://uri/'", value)
                );
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

            [Theory]
            [InlineData(HttpStatusCode.OK)]         // 200
            [InlineData(HttpStatusCode.NoContent)]  // 204
            public async void StatusCode2xx_ReturnsHealthy(HttpStatusCode statusCode)
            {
                response.StatusCode = statusCode;

                var result = await UrlChecker.DefaultUrlCheck(response);

                Assert.Equal(CheckStatus.Healthy, result.CheckStatus);
                Assert.Equal($"status code {statusCode} ({(int)statusCode})", result.Description);
                Assert.Collection(result.Data.OrderBy(kvp => kvp.Key).Select(kvp => $"'{kvp.Key}' = '{kvp.Value}'"),
                    value => Assert.Equal("'body' = 'This is the body content'", value),
                    value => Assert.Equal("'reason' = 'HTTP reason phrase'", value),
                    value => Assert.Equal($"'status' = '{(int)statusCode}'", value),
                    value => Assert.Equal("'url' = 'http://uri/'", value)
                );
            }

            [Theory]
            [InlineData(HttpStatusCode.Continue)]            // 1xx
            [InlineData(HttpStatusCode.Moved)]               // 3xx
            [InlineData(HttpStatusCode.NotFound)]            // 4xx
            [InlineData(HttpStatusCode.ServiceUnavailable)]  // 5xx
            public async void StatusCodeNon2xx_ReturnsUnhealthy(HttpStatusCode statusCode)
            {
                response.StatusCode = statusCode;

                var result = await UrlChecker.DefaultUrlCheck(response);

                Assert.Equal(CheckStatus.Unhealthy, result.CheckStatus);
                Assert.Equal($"status code {statusCode} ({(int)statusCode})", result.Description);
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
            private readonly HttpMessageHandler _handler;

            // URL and response
            public TestableUrlChecker(Func<HttpResponseMessage, ValueTask<IHealthCheckResult>> checkFunc,
                                      string url, HttpResponseMessage response)
                : base(checkFunc, url)
            {
                _handler = new ResponseHandler(url, response);
            }

            // Exception
            public TestableUrlChecker(Exception exceptionToThrow, string url)
                : base(response => { throw new DivideByZeroException(); }, url)
            {
                _handler = new ExceptionHandler(exceptionToThrow);
            }

            protected override HttpClient GetHttpClient()
                => new HttpClient(_handler);

            class ExceptionHandler : HttpMessageHandler
            {
                private readonly Exception _exceptionToThrow;

                public ExceptionHandler(Exception exceptionToThrow)
                    => _exceptionToThrow = exceptionToThrow;

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                    => throw _exceptionToThrow;
            }

            class ResponseHandler : HttpMessageHandler
            {
                private static readonly HttpResponseMessage _defaultResponse = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
                private readonly HttpResponseMessage _response;
                private readonly string _url;

                public ResponseHandler(string url, HttpResponseMessage response)
                {
                    _url = url;
                    _response = response;
                }

                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                {
                    var response = _url == request.RequestUri.ToString() ? _response : _defaultResponse;
                    response.RequestMessage = request;
                    return Task.FromResult(response);
                }
            }
        }
    }
}
