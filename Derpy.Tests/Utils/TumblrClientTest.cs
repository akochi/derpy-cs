using System;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Derpy.Utils;
using Derpy.Utils.Tumblr;
using Moq;
using Moq.Protected;
using Xunit;

namespace Derpy.Tests.Utils
{
    public class TumblrClientTest
    {
        private readonly TumblrClient _client;
        private readonly Mock<HttpMessageHandler> _handler = new Mock<HttpMessageHandler>();

        public TumblrClientTest()
        {
            var keyProvider = new Mock<IKeyProvider>();
            keyProvider.Setup(key => key.TumblrApiKey).Returns("test-api-key");
            _client = new TumblrClient(_handler.Object, keyProvider.Object);
        }

        [Fact]
        public async void Test_FullResponse()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    TumblrRequestMessageFor("test-blog"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("full"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog");

            Assert.Equal(3, urls.Length);
            Assert.Equal(@"https://test-blog.tumblr.com/post/3507845453", urls[0]);
            Assert.Equal(@"https://test-blog.tumblr.com/post/4534708483", urls[1]);
            Assert.Equal(@"https://test-blog.tumblr.com/post/8943561832", urls[2]);
        }

        [Fact]
        public async void Test_FullResponseWithTag()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    TumblrRequestMessageFor("test-blog", "test-tag"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("full"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog", "test-tag");

            Assert.Equal(3, urls.Length);
            Assert.Equal(@"https://test-blog.tumblr.com/post/3507845453", urls[0]);
            Assert.Equal(@"https://test-blog.tumblr.com/post/4534708483", urls[1]);
            Assert.Equal(@"https://test-blog.tumblr.com/post/8943561832", urls[2]);
        }

        [Fact]
        public async void Test_BadResponse()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    TumblrRequestMessageFor("test-blog"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("bad"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog");

            Assert.Null(urls);
        }

        [Fact]
        public async void Test_MalformedResponse()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    TumblrRequestMessageFor("test-blog"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("malformed"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog");

            Assert.Null(urls);
        }

        [Fact]
        public async void Test_BadUrlResponse()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    TumblrRequestMessageFor("test-blog"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog");

            Assert.Null(urls);
        }

        private static Stream LoadJsonResponse(string responseName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream($"Derpy.Tests.Utils.TestResponses.{responseName}.json");
        }

        private static Expression TumblrRequestMessageFor(string blogIdentifier, string tag = null)
        {
            // Create request message that checks if correct URL is created by GetAllPostUrlsAsync
            return ItExpr.Is<HttpRequestMessage>(req
                => req.RequestUri.Query.Contains("api_key=test-api-key")
                   && req.RequestUri.AbsolutePath.Split('/', StringSplitOptions.None)[3] == blogIdentifier
                   && (tag == null || req.RequestUri.Query.Contains($"tag={tag}")));
        }
    }
}
