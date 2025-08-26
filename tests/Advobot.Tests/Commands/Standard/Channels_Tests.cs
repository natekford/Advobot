using Advobot.Standard.Commands;
using Advobot.Tests.TestBases;

using Discord;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Channels_Tests : Command_Tests
{
	[TestMethod]
	public async Task ClearChannelPerms_Empty_Test()
	{
		var input = $"{nameof(Channels.ClearChannelPerms)} {Context.Channel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	[TestMethod]
	public async Task ClearChannelPerms_Test()
	{
		var input = $"{nameof(Channels.ClearChannelPerms)} {Context.Channel}";

		await Context.Channel.AddPermissionOverwriteAsync(
			user: Context.User,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		Assert.IsNotEmpty(Context.Channel.PermissionOverwrites);

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsEmpty(Context.Channel.PermissionOverwrites);
	}

	[TestMethod]
	public async Task CopyChannelPerms_All_Test()
	{
		var input = $"{nameof(Channels.CopyChannelPerms)} {Context.Channel} {OtherTextChannel}";

		await Context.Channel.AddPermissionOverwriteAsync(
			user: Context.User,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		await Context.Channel.AddPermissionOverwriteAsync(
			role: AdminRole,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		Assert.IsEmpty(OtherTextChannel.PermissionOverwrites);

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.HasCount(2, OtherTextChannel.PermissionOverwrites);
	}

	[TestMethod]
	public async Task CopyChannelPerms_Mismatch_Test()
	{
		var input = $"{nameof(Channels.CopyChannelPerms)} {Context.Channel} {VoiceChannel}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task CopyChannelPerms_Role_Test()
	{
		var input = $"{nameof(Channels.CopyChannelPerms)} {Context.Channel} {OtherTextChannel} {AdminRole}";

		await Context.Channel.AddPermissionOverwriteAsync(
			user: Context.User,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		await Context.Channel.AddPermissionOverwriteAsync(
			role: AdminRole,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		Assert.IsEmpty(OtherTextChannel.PermissionOverwrites);

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(AdminRole.Id, OtherTextChannel.PermissionOverwrites.Single().TargetId);
	}

	[TestMethod]
	public async Task CopyChannelPerms_User_Test()
	{
		var input = $"{nameof(Channels.CopyChannelPerms)} {Context.Channel} {OtherTextChannel} {Context.User}";

		await Context.Channel.AddPermissionOverwriteAsync(
			user: Context.User,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		await Context.Channel.AddPermissionOverwriteAsync(
			role: AdminRole,
			permissions: OverwritePermissions.AllowAll(Context.Channel)
		).ConfigureAwait(false);
		Assert.IsEmpty(OtherTextChannel.PermissionOverwrites);

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.AreEqual(Context.User.Id, OtherTextChannel.PermissionOverwrites.Single().TargetId);
	}
}