using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels
{
	[TestClass]
	public sealed class CanModifyChannelAttribute_Tests
		: ParameterPreconditionTestsBase<CanModifyChannelAttribute>
	{
		private static readonly OverwritePermissions _Allowed = new(
			viewChannel: PermValue.Allow,
			manageMessages: PermValue.Allow
		);
		private static readonly OverwritePermissions _Denied = new(
			viewChannel: PermValue.Allow,
			manageMessages: PermValue.Deny
		);
		private static readonly GuildPermissions _ManageMessages = new(manageMessages: true);
		private readonly FakeTextChannel _Channel;

		protected override CanModifyChannelAttribute Instance { get; } = new(ChannelPermission.ManageMessages);

		public CanModifyChannelAttribute_Tests()
		{
			_Channel = new(Context.Guild);
			Context.Guild.FakeEveryoneRole.Permissions = new(viewChannel: true);
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

			await AssertSuccessAsync(_Channel).CAF();
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

			await AssertFailureAsync(_Channel).CAF();
		}
	}
}