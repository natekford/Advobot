using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	[TestClass]
	public sealed class CanModifyChannelAttribute_Tests
		: ParameterPreconditions_TestsBase<CanModifyChannelAttribute>
	{
		private static readonly OverwritePermissions _Allowed = new OverwritePermissions(
			viewChannel: PermValue.Allow,
			manageMessages: PermValue.Allow
		);

		private static readonly OverwritePermissions _Denied = new OverwritePermissions(
			viewChannel: PermValue.Allow,
			manageMessages: PermValue.Deny
		);

		private static readonly GuildPermissions _ManageMessages = new GuildPermissions(manageMessages: true);
		private readonly FakeTextChannel _Channel;

		public override CanModifyChannelAttribute Instance { get; }
			= new CanModifyChannelAttribute(ChannelPermission.ManageMessages);

		public CanModifyChannelAttribute_Tests()
		{
			_Channel = new FakeTextChannel(Context.Guild);
			Context.Guild.FakeEveryoneRole.Permissions = new GuildPermissions(viewChannel: true);
		}

		[TestMethod]
		public async Task CanModify_Test()
		{
			var role = new FakeRole(Context.Guild);
			await role.ModifyAsync(x => x.Permissions = _ManageMessages).CAF();
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			await _Channel.AddPermissionOverwriteAsync(Context.User, _Allowed).CAF();
			await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Allowed).CAF();

			var result = await CheckAsync(_Channel).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task CannotModify_Test()
		{
			var role = new FakeRole(Context.Guild);
			await role.ModifyAsync(x => x.Permissions = _ManageMessages).CAF();
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			await _Channel.AddPermissionOverwriteAsync(Context.User, _Denied).CAF();
			await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).CAF();

			var result = await CheckAsync(_Channel).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotIGuildChannel_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();
	}
}