using System.Threading.Tasks;

using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Services.GuildSettings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.Permissions
{
	[TestClass]
	public sealed class RequireGuildPermissionsAttribute_Tests
		: Preconditions_TestBase<RequireGuildPermissionsAttribute>
	{
		private const GuildPermission FLAGS1 = GuildPermission.KickMembers | GuildPermission.BanMembers;
		private const GuildPermission FLAGS2 = GuildPermission.ManageMessages | GuildPermission.ManageRoles;
		private const GuildPermission FLAGS3 = GuildPermission.ManageGuild;

		private readonly IGuildSettings _Settings;

		protected override RequireGuildPermissionsAttribute Instance
			=> new RequireGuildPermissionsAttribute(FLAGS1, FLAGS2, FLAGS3);

		public RequireGuildPermissionsAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

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
		public async Task InvalidPermissions_Test(GuildPermission permission)
		{
			var val = (ulong)permission;
			var role = new FakeRole(Context.Guild)
			{
				Permissions = new GuildPermissions(val),
			};
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);

			await Context.User.RemoveRoleAsync(role).CAF();
			_Settings.BotUsers.Add(new BotUser(Context.User.Id, val));

			var result2 = await CheckAsync().CAF();
			Assert.IsFalse(result2.IsSuccess);
		}

		[DataRow(GuildPermission.Administrator)]
		[DataRow(FLAGS1)]
		[DataRow(FLAGS2)]
		[DataRow(FLAGS3)]
		[DataRow(FLAGS1 | GuildPermission.ManageEmojis)]
		[DataRow(FLAGS1 | FLAGS2 | FLAGS3)]
		[DataTestMethod]
		public async Task ValidPermissions_Test(GuildPermission permission)
		{
			var val = (ulong)permission;
			var role = new FakeRole(Context.Guild)
			{
				Permissions = new GuildPermissions(val),
			};
			await Context.User.AddRoleAsync(role).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(role).CAF();

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);

			await Context.User.RemoveRoleAsync(role).CAF();
			_Settings.BotUsers.Add(new BotUser(Context.User.Id, val));

			var result2 = await CheckAsync().CAF();
			Assert.IsTrue(result2.IsSuccess);
		}
	}
}