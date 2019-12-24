using Derpy.Services;
using Discord;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace Derpy.Tests.Services
{
    public class RolesTest
    {
        private readonly Roles _service = new Roles();

        const int UNICORN_ROLE_ID = 1;
        const int PEGASUS_ROLE_ID = 2;

        public Mock<IUser> _user = new Mock<IUser>();
        public Mock<IGuild> _guild = new Mock<IGuild>();
        public Mock<IGuildUser> _guildUser = new Mock<IGuildUser>();
        public Mock<IRole> _unicornRole = new Mock<IRole>();
        public Mock<IRole> _pegasusRole = new Mock<IRole>();

        public RolesTest()
        {
            _user.Setup(user => user.Id).Returns(42);
            _guild
                .Setup(guild => guild.GetUserAsync(42, It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                .ReturnsAsync(_guildUser.Object);
            _guild.Setup(guild => guild.Roles).Returns(new IRole[] { _unicornRole.Object, _pegasusRole.Object });
            _guild.Setup(guild => guild.GetRole(UNICORN_ROLE_ID)).Returns(_unicornRole.Object);
            _guild.Setup(guild => guild.GetRole(PEGASUS_ROLE_ID)).Returns(_pegasusRole.Object);
            _guildUser.Setup(user => user.Nickname).Returns("Guest");
            _unicornRole.Setup(role => role.Name).Returns("Unicorns");
            _pegasusRole.Setup(role => role.Name).Returns("Pegasi");
        }

        [Fact]
        public async void Test_AddVanityRole()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi") as CommandResult;

            _guildUser.Verify(user => user.AddRoleAsync(_pegasusRole.Object, It.IsAny<RequestOptions>()));
            Assert.True(result.IsSuccess);
            Assert.Equal("You are now part of the Pegasi, Guest!", result.Message);
        }

        [Fact]
        public async void Test_RoleAlreadyGiven()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { PEGASUS_ROLE_ID });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi") as CommandResult;

            _guildUser.Verify(user => user.AddRoleAsync(It.IsAny<IRole>(), It.IsAny<RequestOptions>()), Times.Never);
            Assert.False(result.IsSuccess);
            Assert.Equal("You already are a Pegasi!", result.Message);
        }

        [Fact]
        public async void Test_ChangeRole()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { UNICORN_ROLE_ID });

            var result = await _service.SetRole(_guild.Object, _user.Object, "Pegasi") as CommandResult;

            _guildUser.Verify(user => user.RemoveRoleAsync(_unicornRole.Object, It.IsAny<RequestOptions>()));
            _guildUser.Verify(user => user.AddRoleAsync(_pegasusRole.Object, It.IsAny<RequestOptions>()));
            Assert.True(result.IsSuccess);
            Assert.Equal("You are now part of the Pegasi, Guest!", result.Message);
        }

        [Fact]
        public async void Test_ClearRoles()
        {
            var miscRole = new Mock<IRole>();
            _guild.Setup(guild => guild.GetRole(99)).Returns(miscRole.Object);
            miscRole.Setup(guild => guild.Name).Returns("Misc");
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { UNICORN_ROLE_ID, 99 });

            var result = await _service.ClearRoles(_guild.Object, _user.Object) as CommandResult;

            _guildUser.Verify(user => user.RemoveRolesAsync(new IRole[] { _unicornRole.Object }, It.IsAny<RequestOptions>()));
            Assert.True(result.IsSuccess);
            Assert.Equal("Here you go, just as new!", result.Message);
        }

        [Fact]
        public async void Test_ClearNoRoles()
        {
            _guildUser.Setup(user => user.RoleIds).Returns(new ulong[] { });

            var result = await _service.ClearRoles(_guild.Object, _user.Object) as CommandResult;

            _guildUser.Verify(user => user.RemoveRolesAsync(It.IsAny<IEnumerable<IRole>>(), It.IsAny<RequestOptions>()), Times.Never);
            Assert.False(result.IsSuccess);
            Assert.Equal("Mh, I found nothing to remove...", result.Message);
        }
    }
}
