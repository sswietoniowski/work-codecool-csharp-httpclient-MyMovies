using Moq;
using Moq.Protected;
using Movies.Client;
using NUnit.Framework;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Movies.Api.Tests
{
    [TestFixture]
    public class TestableClassWithApiAccessUnitTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException()
        {
            var httpClient = new HttpClient(new Return401UnauthorizedResponseHandler())
            {
                BaseAddress = new System.Uri("http://localhost:33333")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            Assert.ThrowsAsync<UnauthorizedApiAccessException>(async () => await testableClass.GetMovie(CancellationToken.None));
        }

        [Test]
        public void GetMovie_On401Response_MustThrowUnauthorizedApiAccessException_WithMoq()
        {
            var unauthorizedResponseHttpMessageHandlerMock = new Mock<HttpMessageHandler>();
            unauthorizedResponseHttpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });

            var httpClient = new HttpClient(unauthorizedResponseHttpMessageHandlerMock.Object)
            {
                BaseAddress = new System.Uri("http://localhost:33333")
            };

            var testableClass = new TestableClassWithApiAccess(httpClient);

            Assert.ThrowsAsync<UnauthorizedApiAccessException>(async () => await testableClass.GetMovie(CancellationToken.None));
        }
    }
}