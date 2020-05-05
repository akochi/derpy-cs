using Discord;
using Moq;
using Norn.Test;
using Xunit;

namespace Derpy.Tests
{
    public class DrawalongTest
    {
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly Drawalong.Service _drawalong;

        public DrawalongTest() => _drawalong = new Drawalong.Service(_scheduler);

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

            Assert.Equal("A drawalong is already running!", result.Message);
        }

        [Fact]
        public void Test_CreateDrawalongInDM()
        {
            var channel = new Mock<IDMChannel>();
            var user = new Mock<IGuildUser>();

            var result = _drawalong.Create(channel.Object, user.Object, "Test");

            Assert.Equal("You can't run a drawalong here!", result.Message);
            Assert.False(_drawalong.Running);
        }

        [Fact]
        public void Test_ComparesUsersWithCacheMiss()
        {
            var channel = new Mock<ITextChannel>();
            var user1 = new Mock<IGuildUser>();
            var user2 = new Mock<IGuildUser>();

            // Those are two different instances referring to the same user, in the case we have a cache miss and load
            // a new object.
            user1.Setup(user => user.Id).Returns(42);
            user2.Setup(user => user.Id).Returns(42);
            user2.Setup(user => user.Nickname).Returns("User");

            _drawalong.Create(channel.Object, user1.Object, "Test");

            var result = _drawalong.Join(user2.Object);

            Assert.Equal("You are already in this drawalong, User!", result.Message);
        }
    }
}
