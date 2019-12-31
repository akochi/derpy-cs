using Discord;
using Moq;
using Norn.Test;
using Xunit;

namespace Derpy.Tests
{
    public class DrawalongTest
    {
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly Drawalong _drawalong;

        public DrawalongTest() => _drawalong = new Drawalong(_scheduler);

        [Fact]
        public void Test_NotActive()
        {
            Assert.False(_drawalong.Active);
            Assert.False(_drawalong.Running);
        }

        [Fact]
        public void Test_CreateDrawalong()
        {
            var channel = new Mock<ITextChannel>();
            var user = new Mock<IGuildUser>();

            var result = _drawalong.Create(channel.Object, user.Object, "Test");

            Assert.True(result.IsSuccess);
            Assert.Equal("Drawalong created! Topic is \"Test\".", result.Message);
            Assert.True(_drawalong.Active);
            Assert.False(_drawalong.Running);
        }

        [Fact]
        public void Test_CreateDrawalongTwice()
        {
            var channel = new Mock<ITextChannel>();
            var user = new Mock<IGuildUser>();

            var _ = _drawalong.Create(channel.Object, user.Object, "Test");
            var result = _drawalong.Create(channel.Object, user.Object, "Copycat");

            Assert.False(result.IsSuccess);
            Assert.Equal("A drawalong is already running!", result.Message);
        }

        [Fact]
        public void Test_CreateDrawalongInDM()
        {
            var channel = new Mock<IDMChannel>();
            var user = new Mock<IGuildUser>();

            var result = _drawalong.Create(channel.Object, user.Object, "Test");

            Assert.False(result.IsSuccess);
            Assert.Equal("You can't run a drawalong here!", result.Message);
            Assert.False(_drawalong.Running);
        }
    }
}
