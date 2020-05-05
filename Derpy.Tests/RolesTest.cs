using Discord;
using Moq;
using Norn.Test;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Derpy.Result;

namespace Derpy.Tests
{
    public class RolesTest
    {
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly Roles.Service _service;

        const int USER_ID = 42;
        const int UNICORN_ROLE_ID = 1;
        const int PEGASUS_ROLE_ID = 2;
        const int NSFW_ROLE_ID = 99;

        public Mock<IUser> _user = new Mock<IUser>();
        public Mock<IGuild> _guild = new Mock<IGuild>();
        public Mock<IGuildUser> _guildUser = new Mock<IGuildUser>();
        public Mock<IRole> _unicornRole = new Mock<IRole>();
        public Mock<IRole> _pegasusRole = new Mock<IRole>();
        public Mock<IRole> _nsfwRole = new Mock<IRole>();

        public RolesTest()
        {
            _service = new Roles.Service(_scheduler);

            _user.Setup(user => user.Id).Returns(USER_ID);
            _user.Setup(user => user.Username).Returns("Moq");
            _guild
                .Setup(guild => guild.GetUserAsync(USER_ID, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(_guildUser.Object);
            _guild.Setup(guild => guild.Roles).Returns(new IRole[] {
                _unicornRole.Object, _pegasusRole.Object, _nsfwRole.Object
            });
            _guild.Setup(guild => guild.GetRole(UNICORN_ROLE_ID)).Returns(_unicornRole.Object);
            _guild.Setup(guild => guild.GetRole(PEGASUS_ROLE_ID)).Returns(_pegasusRole.Object);
            _guildUser.Setup(user => user.Id).Returns(USER_ID);
            _guildUser.Setup(user => user.Nickname).Returns("Guest");
            _unicornRole.Setup(role => role.Name).Returns("Unicorns");
            _pegasusRole.Setup(role => role.Name).Returns("Pegasi");
            _nsfwRole.Setup(role => role.Id).Returns(NSFW_ROLE_ID);
            _nsfwRole.Setup(role => role.Name).Returns("nsfw opt-in");
        }

        #region Roles
        [Fact]
        public async void Test_AddVanityRole()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi");

            _guildUser.Verify(user => user.AddRoleAsync(_pegasusRole.Object, It.IsAny<RequestOptions>()));
            Assert.Equal("You are now part of the Pegasi, Guest!", result.Message);
        }

        [Fact]
        public async void Test_RoleAlreadyGiven()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { PEGASUS_ROLE_ID });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi");

            _guildUser.Verify(user => user.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Never);
            Assert.Equal("You already are a Pegasi!", result.Message);
        }

        [Fact]
        public async void Test_ChangeRole()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { UNICORN_ROLE_ID });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi");

            _guildUser.Verify(user => user.RemoveRoleAsync(_unicornRole.Object, It.IsAny<RequestOptions>()));
            _guildUser.Verify(user => user.AddRoleAsync(_pegasusRole.Object, It.IsAny<RequestOptions>()));
            Assert.Equal("You are now part of the Pegasi, Guest!", result.Message);
        }

        [Fact]
        public async void Test_ClearRoles()
        {
            var miscRole = new Mock<IRole>();
            _guild.Setup(guild => guild.GetRole(99)).Returns(miscRole.Object);
            miscRole.Setup(guild => guild.Name).Returns("Misc");
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { UNICORN_ROLE_ID, 99 });

            var result = await _service.ClearRoles(_guild.Object, _user.Object);

            _guildUser.Verify(user => user.RemoveRolesAsync(new IRole[] { _unicornRole.Object }, It.IsAny<RequestOptions>()));
            Assert.Equal("Here you go, just as new!", result.Message);
        }

        [Fact]
        public async void Test_ClearNoRoles()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });

            var result = await _service.ClearRoles(_guild.Object, _user.Object);

            _guildUser.Verify(user => user.RemoveRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>()), Times.Never);
            Assert.Equal("Mh, I found nothing to remove...", result.Message);
        }
        #endregion

        #region NSFW
        [Fact]
        public async void Test_NsfwExpiration()
        {
            var channel = new Mock<IMessageChannel>();

            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });
            var message = PrepareMessage(channel);

            var result = await _service.EnableNsfw(_guild.Object, channel.Object, _user.Object);
            Mock.Verify(channel);

            Assert.IsType<Success>(result);

            message.Setup(message => message.ModifyAsync(It.IsAny<Action<MessageProperties>>(), null))
                .Callback((Action<MessageProperties> callback, RequestOptions _) =>
                {
                    var mock = new Mock<MessageProperties>();
                    mock.SetupAllProperties();

                    callback.Invoke(mock.Object);

                    Assert.Equal("NSFW access request for Moq expired.", mock.Object.Content.Value);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            _scheduler.AdvanceToNextTimer();
            Mock.Verify(message);
        }

        [Fact]
        public async void Test_NsfwAccessAlreadyGiven()
        {
            var channel = new Mock<IMessageChannel>();
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { NSFW_ROLE_ID });

            var result = await _service.EnableNsfw(_guild.Object, channel.Object, _user.Object);

            Assert.Equal("You have already opted in the adult channels!", result.Message);
        }

        [Fact]
        public async void Test_NsfwGiveAccess()
        {
            var channel = new Mock<IMessageChannel>();
            var message = PrepareMessage(channel);

            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });

            var result = await _service.EnableNsfw(_guild.Object, channel.Object, _user.Object);
            Mock.Verify(message);

            result = await _service.EnableNsfw(_guild.Object, channel.Object, _user.Object);
            Assert.Equal("You have been allowed into the adult channels!", result.Message);

            _guildUser.Verify(user => user.AddRoleAsync(_nsfwRole.Object, null));
        }

        private static Mock<IUserMessage> PrepareMessage(Mock<IMessageChannel> channel)
        {
            var message = new Mock<IUserMessage>();

            channel.Setup(channel =>
                            channel.SendMessageAsync(
                                "Moq, you are trying to access 18+ channels. Repeat this command in the next 5 minutes to confirm.\n"
                                + "**Reminder:** Lying about your age may result in a server ban.",
                                false, null, null)
                        )
                            .ReturnsAsync(message.Object)
                            .Verifiable();

            return message;
        }
        #endregion
    }
}
