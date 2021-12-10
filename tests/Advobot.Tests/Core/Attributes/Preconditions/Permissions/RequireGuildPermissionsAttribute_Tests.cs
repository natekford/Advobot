using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.Permissions;

[TestClass]
public sealed class RequireGuildPermissionsAttribute_Tests : PreconditionTestsBase
{
	private const GuildPermission FLAGS1 = GuildPermission.KickMembers | GuildPermission.BanMembers;
	private const GuildPermission FLAGS2 = GuildPermission.ManageMessages | GuildPermission.ManageRoles;
	private const GuildPermission FLAGS3 = GuildPermission.ManageGuild;

	protected override PreconditionAttribute Instance { get; }
		= new RequireGuildPermissionsAttribute(FLAGS1, FLAGS2, FLAGS3);

	[DataRow(GuildPermission.CreateInstantInvite)]
	[DataRow(GuildPermission.AddReactions)]
	[DataRow(GuildPermission.ViewAuditLog)]
	[DataRow(GuildPermission.PrioritySpeaker)]
	[DataRow(GuildPermission.ViewChannel)]
	[DataRow(GuildPermission.SendMessages)]
	[DataRow(GuildPermission.SendTTSMessages)]
	[DataRow(GuildPermission.EmbedLinks)]
	[DataRow(GuildPermission.AttachFiles)]
	[DataRow(GuildPermission.ReadMessageHistory)]
	[DataRow(GuildPermission.MentionEveryone)]
	[DataRow(GuildPermission.UseExternalEmojis)]
	[DataRow(GuildPermission.Connect)]
	[DataRow(GuildPermission.Speak)]
	[DataRow(GuildPermission.UseVAD)]
	[DataRow(GuildPermission.ChangeNickname)]
	[DataRow(GuildPermission.KickMembers)]
	[DataRow(GuildPermission.BanMembers)]
	[DataRow(GuildPermission.ManageMessages)]
	[DataRow(GuildPermission.ManageRoles)]
	[DataRow(GuildPermission.KickMembers | GuildPermission.ManageMessages)]
	[DataTestMethod]
	public async Task InvalidPermissions_Test(ulong permission)
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new GuildPermissions(permission),
		};
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[DataRow(GuildPermission.Administrator)]
	[DataRow(FLAGS1)]
	[DataRow(FLAGS2)]
	[DataRow(FLAGS3)]
	[DataRow(FLAGS1 | GuildPermission.ManageEmojisAndStickers)]
	[DataRow(FLAGS1 | FLAGS2 | FLAGS3)]
	[DataTestMethod]
	public async Task ValidPermissions_Test(ulong permission)
	{
		var role = new FakeRole(Context.Guild)
		{
			Permissions = new GuildPermissions(permission),
		};
		await Context.User.AddRoleAsync(role).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}