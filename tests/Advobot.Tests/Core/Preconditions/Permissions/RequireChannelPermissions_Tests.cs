using Advobot.Preconditions.Permissions;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

namespace Advobot.Tests.Core.Preconditions.Permissions;

[TestClass]
public sealed class RequireChannelPermissions_Tests
	: Precondition_Tests<RequireChannelPermissions>
{
	private const ChannelPermission FLAGS0 = ChannelPermission.ViewChannel | ChannelPermission.SendMessages;
	private const ChannelPermission FLAGS1 = ChannelPermission.AttachFiles | ChannelPermission.EmbedLinks;
	private const ChannelPermission FLAGS2 = ChannelPermission.ManageChannels | ChannelPermission.ManageRoles;
	private const ChannelPermission FLAGS3 = ChannelPermission.ManageMessages;

	protected override RequireChannelPermissions Instance { get; }
		= new(FLAGS1, FLAGS2, FLAGS3);

	[DataRow(ChannelPermission.CreateInstantInvite)]
	[DataRow(ChannelPermission.AddReactions)]
	[DataRow(ChannelPermission.PrioritySpeaker)]
	[DataRow(ChannelPermission.ViewChannel)]
	[DataRow(ChannelPermission.SendMessages)]
	[DataRow(ChannelPermission.SendTTSMessages)]
	[DataRow(ChannelPermission.ReadMessageHistory)]
	[DataRow(ChannelPermission.MentionEveryone)]
	[DataRow(ChannelPermission.UseExternalEmojis)]
	[DataRow(ChannelPermission.Connect)]
	[DataRow(ChannelPermission.Speak)]
	[DataRow(ChannelPermission.UseVAD)]
	[DataRow(ChannelPermission.AttachFiles)]
	[DataRow(ChannelPermission.EmbedLinks)]
	[DataRow(ChannelPermission.ManageChannels)]
	[DataRow(ChannelPermission.ManageRoles)]
	[DataRow(ChannelPermission.AttachFiles | ChannelPermission.ManageChannels)]
	[TestMethod]
	public async Task InvalidPermissions_Test(ChannelPermission permission)
	{
		var permissions = new OverwritePermissions(allowValue: (ulong)permission, denyValue: 0);
		await Context.Channel.AddPermissionOverwriteAsync(Context.User, permissions).CAF();
		await Context.Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, permissions).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[DataRow(FLAGS0 | FLAGS1)]
	[DataRow(FLAGS0 | FLAGS2)]
	[DataRow(FLAGS0 | FLAGS3)]
	[DataRow(FLAGS0 | FLAGS1 | ChannelPermission.ManageWebhooks)]
	[DataRow(FLAGS0 | FLAGS1 | FLAGS2 | FLAGS3)]
	[TestMethod]
	public async Task ValidPermissionsButChannelDenied_Test(ChannelPermission permission)
	{
		var permissions = new OverwritePermissions(allowValue: 0, denyValue: (ulong)permission);
		await Context.Channel.AddPermissionOverwriteAsync(Context.User, permissions).CAF();
		await Context.Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, permissions).CAF();

		var role = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)permission),
		};
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[DataRow(FLAGS0 | FLAGS1)]
	[DataRow(FLAGS0 | FLAGS2)]
	[DataRow(FLAGS0 | FLAGS3)]
	[DataRow(FLAGS0 | FLAGS1 | ChannelPermission.ManageWebhooks)]
	[DataRow(FLAGS0 | FLAGS1 | FLAGS2 | FLAGS3)]
	[TestMethod]
	public async Task ValidPermissionsByChannel_Test(ChannelPermission permission)
	{
		var permissions = new OverwritePermissions(allowValue: (ulong)permission, denyValue: 0);
		await Context.Channel.AddPermissionOverwriteAsync(Context.User, permissions).CAF();
		await Context.Channel.AddPermissionOverwriteAsync(Context.Guild.FakeCurrentUser, permissions).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	[DataRow(FLAGS0 | FLAGS1)]
	[DataRow(FLAGS0 | FLAGS2)]
	[DataRow(FLAGS0 | FLAGS3)]
	[DataRow(FLAGS0 | FLAGS1 | ChannelPermission.ManageWebhooks)]
	[DataRow(FLAGS0 | FLAGS1 | FLAGS2 | FLAGS3)]
	[TestMethod]
	public async Task ValidPermissionsByGuild_Test(ChannelPermission permission)
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new((ulong)permission),
		};
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}