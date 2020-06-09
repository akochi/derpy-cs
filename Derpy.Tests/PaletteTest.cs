using System.Threading.Tasks;
using Derpy.Utils.Tumblr;
using Moq;
using Xunit;

namespace Derpy.Tests
{
    public class PaletteTest
    {
        private readonly Palette.Service _palette;
        private readonly Mock<ITumblrClient> _client = new Mock<ITumblrClient>();

        public PaletteTest()
        {
            _palette = new Palette.Service(_client.Object);
        }

        [Fact]
        public async void Test_GetRandomColourPaletteUrl()
        {
            _client.Setup(client => client.GetAllPostUrlsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new []
                {
                    "https://www.example.com/new-url"
                });

            var url = await _palette.GetRandomColourPaletteUrl();
            Assert.Equal("https://www.example.com/new-url", url);
        }

        [Fact]
        public async void Test_GetRandomColourPaletteUrlBad()
        {
            _client.Setup(client => client.GetAllPostUrlsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<string[]>(null));

            var url = await _palette.GetRandomColourPaletteUrl();
            Assert.Null(url);
        }
    }
}
