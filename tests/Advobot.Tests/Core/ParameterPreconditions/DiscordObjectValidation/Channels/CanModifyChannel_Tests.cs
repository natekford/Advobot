using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.TestBases;

using Discord;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Channels;

[TestClass]
public sealed class CanModifyChannel_Tests : ParameterPrecondition_Tests<CanModifyChannel>
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

	protected override CanModifyChannel Instance { get; } = new(ChannelPermission.ManageMessages);

	public CanModifyChannel_Tests()
	{
		_Channel = new(Context.Guild);
		Context.Guild.FakeEveryoneRole.Permissions = new(viewChannel: true);
	}

	[TestMethod]
	public async Task CanModify_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _ManageMessages).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).ConfigureAwait(false);

		await _Channel.AddPermissionOverwriteAsync(Context.User, _Allowed).ConfigureAwait(false);
		await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Allowed).ConfigureAwait(false);

		await AssertSuccessAsync(_Channel).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task CannotModify_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _ManageMessages).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).ConfigureAwait(false);

		await _Channel.AddPermissionOverwriteAsync(Context.User, _Denied).ConfigureAwait(false);
		await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).ConfigureAwait(false);

		await AssertFailureAsync(_Channel).ConfigureAwait(false);
	}
}