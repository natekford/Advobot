using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Services.GuildSettings;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Services.GuildSettings;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions.Permissions
{
	[TestClass]
	public sealed class RequireGenericGuildPermissionsAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireGenericGuildPermissionsAttribute>
	{
		private readonly IGuildSettings _Settings;

		public RequireGenericGuildPermissionsAttribute_Tests()
		{
			_Settings = new GuildSettings();

			Services = new ServiceCollection()
				.AddSingleton<IGuildSettingsFactory>(new FakeGuildSettingsFactory(_Settings))
				.BuildServiceProvider();
		}

		[DataRow(GuildPermission.KickMembers)]
		[DataRow(GuildPermission.BanMembers)]
		[DataRow(GuildPermission.Administrator)]
		[DataRow(GuildPermission.ManageChannels)]
		[DataRow(GuildPermission.ManageGuild)]
		[DataRow(GuildPermission.ManageMessages)]
		[DataRow(GuildPermission.MuteMembers)]
		[DataRow(GuildPermission.DeafenMembers)]
		[DataRow(GuildPermission.MoveMembers)]
		[DataRow(GuildPermission.ManageNicknames)]
		[DataRow(GuildPermission.ManageRoles)]
		[DataRow(GuildPermission.ManageWebhooks)]
		[DataRow(GuildPermission.ManageEmojis)]
		[DataTestMethod]
		public async Task HasAPermission_Test(GuildPermission permission)
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
	}
}