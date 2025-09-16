using Advobot.ParameterPreconditions.Discord.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using Discord;

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
	protected override CanBeMoved Instance { get; } = new();
	private FakeVoiceChannel Channel { get; set; }
	private FakeGuildUser User { get; set; }

	[TestMethod]
	public async Task UserCanBeMovedBecauseAdmin_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _Admin).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).ConfigureAwait(false);

		await Channel.AddPermissionOverwriteAsync(Context.User, _Denied).ConfigureAwait(false);
		await Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).ConfigureAwait(false);

		await AssertSuccessAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task UserCanBeMovedBecauseChannelOverride_Test()
	{
		await Channel.AddPermissionOverwriteAsync(Context.User, _Allowed).ConfigureAwait(false);
		await Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Allowed).ConfigureAwait(false);

		await AssertSuccessAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task UserCanBeMovedBecausePermissions_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _MoveMembers).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).ConfigureAwait(false);

		await AssertSuccessAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task UserCannotBeMovedBecauseChannelOverride_Test()
	{
		var role = new FakeRole(Context.Guild);
		await role.ModifyAsync(x => x.Permissions = _MoveMembers).ConfigureAwait(false);
		await Context.User.AddRoleAsync(role).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).ConfigureAwait(false);

		await Channel.AddPermissionOverwriteAsync(Context.User, _Denied).ConfigureAwait(false);
		await Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, _Denied).ConfigureAwait(false);

		await AssertFailureAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task UserCannotBeMovedBecausePermissions_Test()
		=> await AssertFailureAsync(User).ConfigureAwait(false);

	[TestMethod]
	public async Task UserNotInVoiceChannel_Test()
	{
		User.VoiceChannel = null;

		await AssertFailureAsync(User).ConfigureAwait(false);
	}

	protected override Task SetupAsync()
	{
		Channel = new(Context.Guild);
		User = new(Context.Guild) { VoiceChannel = Channel, };
		Context.Guild.FakeEveryoneRole.Permissions = new(viewChannel: true);
		return Task.CompletedTask;
	}
}