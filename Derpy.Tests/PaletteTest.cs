using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;

namespace Derpy.Tests
{
    public class PaletteTest
    {
        private readonly Palette.Service _palette;
        private readonly Mock<HttpMessageHandler> _handler = new Mock<HttpMessageHandler>();

        public PaletteTest()
        {
            _palette = new Palette.Service(_handler.Object);
        }

        [Fact]
        public async void Test_GetRandomColourPaletteUrl()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    RequestMessage = new HttpRequestMessage
                    {
                        RequestUri = new Uri("https://www.example.com/new-url")
                    }
                });

            var url = await _palette.GetRandomColourPaletteUrl();
            Assert.Equal("https://www.example.com/new-url", url);
        }

        [Fact]
        public async void Test_GetRandomColourPaletteUrlBad()
        {
            _handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var url = await _palette.GetRandomColourPaletteUrl();
            Assert.Null(url);
        }
    }
}
