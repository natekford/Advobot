using Advobot.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Users;

[TestClass]
public sealed class CanBeMoved_Tests : ParameterPrecondition_Tests<CanBeMoved>
{
	private static readonly GuildPermissions _Admin = new(
		administrator: true
	);
	private static readonly OverwritePermissions _Allowed = new(
		viewChannel: PermValue.Allow,
		moveMembers: PermValue.Allow
	);
	private static readonly OverwritePermissions _Denied = new(
		viewChannel: PermValue.Allow,
		moveMembers: PermValue.Deny
	);
	private static readonly GuildPermissions _MoveMembers = new(
		moveMembers: true
	);
	private readonly FakeVoiceChannel _Channel;
	private readonly FakeGuildUser _User;

	protected override CanBeMoved Instance { get; } = new();

	public CanBeMoved_Tests()
	{
		_Channel = new(Context.Guild);
		_User = new(Context.Guild) { VoiceChannel = _Channel, };
		Context.Guild.FakeEveryoneRole.Permissions = new(viewChannel: true);
	}

	[TestMethod]
	public async Task UserCanBeMovedBecauseAdmin_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _Admin).CAF();
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		await _Channel.AddPermissionOverwriteAsync(Context.User, _Denied).CAF();
		await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).CAF();

		await AssertSuccessAsync(_User).CAF();
	}

	[TestMethod]
	public async Task UserCanBeMovedBecauseChannelOverride_Test()
	{
		await _Channel.AddPermissionOverwriteAsync(Context.User, _Allowed).CAF();
		await _Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Allowed).CAF();

		await AssertSuccessAsync(_User).CAF();
	}

	[TestMethod]
	public async Task UserCanBeMovedBecausePermissions_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _MoveMembers).CAF();
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		await AssertSuccessAsync(_User).CAF();
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

		await AssertFailureAsync(_User).CAF();
	}

	[TestMethod]
	public async Task UserCannotBeMovedBecausePermissions_Test()
		=> await AssertFailureAsync(_User).CAF();

	[TestMethod]
	public async Task UserNotInVoiceChannel_Test()
	{
		_User.VoiceChannel = null;

		await AssertFailureAsync(_User).CAF();
	}
}