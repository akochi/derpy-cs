using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Derpy.Utils;
using Derpy.Utils.Tumblr;
using Moq;
using Xunit;

namespace Derpy.Tests.Utils
{
    public class TumblrClientTest
    {
        private readonly TumblrClient _client;
        private readonly Mock<IWebClient> _webClient = new Mock<IWebClient>();

        public TumblrClientTest()
        {
            var keyProvider = new Mock<IKeyProvider>();
            keyProvider.Setup(key => key.TumblrApiKey).Returns("test-api-key");
            _client = new TumblrClient(_webClient.Object, keyProvider.Object);
        }

        [Fact]
        public async void Test_FullResponse()
        {
            _webClient.Setup(webClient => webClient.GetAsync(UriFor("test-blog", null)))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("full"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog");

            Assert.Equal(3, urls.Length);
            Assert.Equal("https://test-blog.tumblr.com/post/3507845453", urls[0]);
            Assert.Equal("https://test-blog.tumblr.com/post/4534708483", urls[1]);
            Assert.Equal("https://test-blog.tumblr.com/post/8943561832", urls[2]);
        }

        [Fact]
        public async void Test_FullResponseWithTag()
        {
            _webClient.Setup(webClient => webClient.GetAsync(UriFor("test-blog", "test-tag")))
                .ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StreamContent(LoadJsonResponse("full"))
                });

            var urls = await _client.GetAllPostUrlsAsync("test-blog", "test-tag");

            Assert.Equal(3, urls.Length);
            Assert.Equal("https://test-blog.tumblr.com/post/3507845453", urls[0]);
            Assert.Equal("https://test-blog.tumblr.com/post/4534708483", urls[1]);
            Assert.Equal("https://test-blog.tumblr.com/post/8943561832", urls[2]);
        }

        [Fact]
        public async void Test_BadResponse()
        {
            _webClient.Setup(webClient => webClient.GetAsync(UriFor("test-blog", null)))
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
            _webClient.Setup(webClient => webClient.GetAsync(UriFor("test-blog", null)))
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
            _webClient.Setup(webClient => webClient.GetAsync(UriFor("test-blog", null)))
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

        private static Uri UriFor(string blogIdentifier, string tag)
        {
            // Create URI match that ensures correct URL is created by GetAllPostUrlsAsync
            return It.Is<Uri>(uri
                => uri.Query.Contains("api_key=test-api-key")
                   && uri.AbsolutePath.Split('/', StringSplitOptions.None)[3] == blogIdentifier
                   && (string.IsNullOrEmpty(tag) || uri.Query.Contains($"tag={tag}")));
        }
    }
}
