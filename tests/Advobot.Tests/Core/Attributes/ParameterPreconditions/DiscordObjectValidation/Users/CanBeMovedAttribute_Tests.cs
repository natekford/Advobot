using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	[TestClass]
	public sealed class CanBeMovedAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<CanBeMovedAttribute>
	{
		private static readonly GuildPermissions _Admin = new GuildPermissions(administrator: true);

		private static readonly OverwritePermissions _Allowed = new OverwritePermissions(
			viewChannel: PermValue.Allow,
			moveMembers: PermValue.Allow
		);

		private static readonly OverwritePermissions _Denied = new OverwritePermissions(
			viewChannel: PermValue.Allow,
			moveMembers: PermValue.Deny
		);

		private static readonly GuildPermissions _MoveMembers = new GuildPermissions(moveMembers: true);
		private readonly FakeVoiceChannel _Channel;
		private readonly FakeGuildUser _User;

		public CanBeMovedAttribute_Tests()
		{
			_Channel = new FakeVoiceChannel(Context.Guild);
			_User = new FakeGuildUser(Context.Guild)
			{
				VoiceChannel = _Channel,
			};
			Context.Guild.FakeEveryoneRole.Permissions = new GuildPermissions(viewChannel: true);
		}

		[TestMethod]
		public async Task FailsOnNotIGuildUser_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task UserCanBeMovedBecauseAdmin_Test()
		{
			var role = new FakeRole(Context.Guild);
			await role.ModifyAsync(x => x.Permissions = _Admin).CAF();
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			await _Channel.AddPermissionOverwriteAsync(Context.User, _Denied).CAF();
			await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task UserCanBeMovedBecauseChannelOverride_Test()
		{
			await _Channel.AddPermissionOverwriteAsync(Context.User, _Allowed).CAF();
			await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Allowed).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task UserCanBeMovedBecausePermissions_Test()
		{
			var role = new FakeRole(Context.Guild);
			await role.ModifyAsync(x => x.Permissions = _MoveMembers).CAF();
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task UserCannotBeMovedBecauseChannelOverride_Test()
		{
			var role = new FakeRole(Context.Guild);
			await role.ModifyAsync(x => x.Permissions = _MoveMembers).CAF();
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			await _Channel.AddPermissionOverwriteAsync(Context.User, _Denied).CAF();
			await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task UserCannotBeMovedBecausePermissions_Test()
		{
			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task UserNotInVoiceChannel_Test()
		{
			_User.VoiceChannel = null;

			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}