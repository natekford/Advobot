using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.ParameterPreconditions.Discord.Users;
using Advobot.ParameterPreconditions.Numbers;
using Advobot.ParameterPreconditions.Strings;
using Advobot.Preconditions.Permissions;
using Advobot.Punishments;
using Advobot.Resources;
using Advobot.Services.GuildSettings;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Names.UsersCategory))]
public sealed class Users
{
	[Command(nameof(Names.Ban), nameof(Names.BanAlias))]
	[LocalizedSummary(nameof(Summaries.BanSummary))]
	[Meta("b798e679-3ca7-4af1-9544-585672ec9936", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Ban : AdvobotModuleBase
	{
		[Command]
		[Priority(1)]
		public Task<AdvobotResult> Targeted(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => BanAsync(user, user.Id, reason);

		[Command]
		[Priority(0)]
		public Task<AdvobotResult> Targeted(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		) => BanAsync(null, userId, reason);

		private async Task<AdvobotResult> BanAsync(
			IUser? user,
			ulong userId,
			ModerationReason reason)
		{
			user ??= await Context.Client.GetUserAsync(userId, CacheMode.AllowDownload).ConfigureAwait(false);
			if (user is null)
			{
				return Responses.Users.CannotFindUser(userId);
			}

			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, userId, true)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.Banned(user!, reason.Time);
		}
	}

	[Command(nameof(Names.Deafen), nameof(Names.DeafenAlias))]
	[LocalizedSummary(nameof(Summaries.DeafenSummary))]
	[Meta("99aa7f17-5710-41ce-ba12-291c2971c0a4", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.DeafenMembers)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var isGive = !user.IsDeafened;
			await PunishmentService.PunishAsync(new Punishments.Deafen(user, isGive)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.Deafened(isGive, user, reason.Time);
		}
	}

	[Command(nameof(Names.Kick), nameof(Names.KickAlias))]
	[LocalizedSummary(nameof(Summaries.KickSummary))]
	[Meta("1d86aa7d-da06-478c-861b-a62ca279523b", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.KickMembers)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			await PunishmentService.PunishAsync(new Punishments.Kick(user)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.Kicked(user);
		}
	}

	[Command(nameof(Names.MoveUser), nameof(Names.MoveUserAlias))]
	[LocalizedSummary(nameof(Summaries.MoveUserSummary))]
	[Meta("158dca6d-fc89-43b3-a6b5-d055f6672547", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			[CanBeMoved]
			IGuildUser user,
			[CanModifyChannel(ChannelPermission.MoveMembers)]
			IVoiceChannel channel
		)
		{
			if (user.VoiceChannel?.Id == channel.Id)
			{
				return Responses.Users.AlreadyInChannel(user, channel);
			}

			await user.ModifyAsync(x => x.Channel = new(channel), GetOptions()).ConfigureAwait(false);
			return Responses.Users.Moved(user, channel);
		}
	}

	[Command(nameof(Names.MoveUsers), nameof(Names.MoveUsersAlias))]
	[LocalizedSummary(nameof(Summaries.MoveUsersSummary))]
	[Meta("4e8439fa-cc29-4acb-9049-89865be825c8", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUsers : MultiUserActionModuleBase
	{
		[Command]
		public async Task<AdvobotResult> All(
			[CanModifyChannel(ChannelPermission.MoveMembers)]
			IVoiceChannel input,
			[CanModifyChannel(ChannelPermission.MoveMembers)]
			IVoiceChannel output
		)
		{
			var amountChanged = await ProcessAsync(
				true,
				u => u.VoiceChannel?.Id == input.Id,
				(u, o) => u.ModifyAsync(x => x.Channel = new(output), o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Response,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[Command(nameof(Names.Mute), nameof(Names.MuteAlias))]
	[LocalizedSummary(nameof(Summaries.MuteSummary))]
	[Meta("b9f305d4-d343-4350-a140-c54a42af8d8d", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
	public sealed class Mute : AdvobotModuleBase
	{
		private static readonly OverwritePermissions CategoryPerms = new(
			allowValue: 0,
			denyValue: ChannelPermissions.Category.RawValue
		);
		private static readonly OverwritePermissions TextPerms = new(
			allowValue: 0,
			denyValue: ChannelPermissions.Text.RawValue
		);
		private static readonly OverwritePermissions VoicePerms = new(
			allowValue: 0,
			denyValue: ChannelPermissions.Voice.RawValue
		);

		[InjectService]
		public required IGuildSettingsService GuildSettings { get; set; }

		[Command]
		public async Task<AdvobotResult> Targeted(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var role = await GuildSettings.GetMuteRoleAsync(Context.Guild).ConfigureAwait(false);
			await ConfigureMuteRoleAsync(role).ConfigureAwait(false);

			var isGive = !user.RoleIds.Contains(role.Id);
			await PunishmentService.PunishAsync(new Punishments.RoleMute(user, role, isGive)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.Muted(isGive, user, reason.Time);
		}

		private async Task ConfigureMuteRoleAsync(IRole role)
		{
			if (role.Permissions.RawValue != 0)
			{
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0)).ConfigureAwait(false);
			}

			var channels = await Context.Guild.GetChannelsAsync().ConfigureAwait(false);
			foreach (var channel in channels)
			{
				var perms = channel switch
				{
					ICategoryChannel _ => CategoryPerms,
					IVoiceChannel _ => VoicePerms,
					ITextChannel _ => TextPerms,
					_ => throw new InvalidOperationException("Invalid channel while configuring mute role."),
				};
				if (channel.GetPermissionOverwrite(role)?.DenyValue != perms.DenyValue)
				{
					await channel.AddPermissionOverwriteAsync(role, perms).ConfigureAwait(false);
				}
			}
		}
	}

	[Command(nameof(Names.RemoveMessages), nameof(Names.RemoveMessagesAlias))]
	[LocalizedSummary(nameof(Summaries.RemoveMessagesSummary))]
	[Meta("a4f3959e-1f56-4bf0-b377-dc98ef017906", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageMessages)]
	public sealed class RemoveMessages : AdvobotModuleBase
	{
		[InjectService]
		public required TimeProvider Time { get; set; }

		[Command]
		[RequireChannelPermissions(ChannelPermission.ManageMessages)]
		public Task<AdvobotResult> CurrentChannel(
			[Positive]
			int deleteCount
		) => DeleteAsync(deleteCount, Context.Channel, null);

		[Command]
		[RequireChannelPermissions(ChannelPermission.ManageMessages)]
		public Task<AdvobotResult> CurrentChannelFromUser(
			[Positive]
			int deleteCount,
			IGuildUser user
		) => DeleteAsync(deleteCount, Context.Channel, user);

		[Command]
		public Task<AdvobotResult> TargetedChannel(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ChannelPermission.ManageMessages)]
			ITextChannel channel
		) => DeleteAsync(deleteCount, channel, null);

		[Command]
		public Task<AdvobotResult> TargetedChannelFromUser(
			[Positive]
			int deleteCount,
			IGuildUser user,
			[CanModifyChannel(ChannelPermission.ManageMessages)]
			ITextChannel channel
		) => DeleteAsync(deleteCount, channel, user);

		[Command]
		public Task<AdvobotResult> TargetedChannelFromUser(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ChannelPermission.ManageMessages)]
			ITextChannel channel,
			IGuildUser user
		) => DeleteAsync(deleteCount, channel, user);

		private async Task<AdvobotResult> DeleteAsync(
			int deleteCount,
			ITextChannel channel,
			IUser? user)
		{
			var deleted = await channel.DeleteMessagesAsync(new DeleteMessageArgs
			{
				Now = Time.GetUtcNow(),
				DeleteCount = deleteCount,
				Options = GetOptions(),
				FromMessage = Context.Message.Channel.Id == channel.Id
					? Context.Message
					: null,
				Predicate = user is null
					? null
					: x => x.Author.Id == user.Id,
			}).ConfigureAwait(false);
			return Responses.Users.RemovedMessages(channel, user, deleted);
		}
	}

	[Command(nameof(Names.SoftBan), nameof(Names.SoftBanAlias))]
	[LocalizedSummary(nameof(Summaries.SoftBanSummary))]
	[Meta("a6084728-77bf-469c-af09-41e53ac021d9", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers, GuildPermission.KickMembers)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command]
		[Priority(1)]
		public Task<AdvobotResult> Targeted(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => SoftBanAsync(user, user.Id, reason);

		[Command]
		[Priority(0)]
		public Task<AdvobotResult> Targeted(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		) => SoftBanAsync(null, userId, reason);

		private async Task<AdvobotResult> SoftBanAsync(
			IUser? user,
			ulong userId,
			ModerationReason reason)
		{
			user ??= await Context.Client.GetUserAsync(userId, CacheMode.AllowDownload).ConfigureAwait(false);
			if (user is null)
			{
				return Responses.Users.CannotFindUser(userId);
			}

			await PunishmentService.PunishAsync(new Punishments.SoftBan(Context.Guild, userId)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.SoftBanned(user);
		}
	}

	[Command(nameof(Names.Unban), nameof(Names.UnbanAlias))]
	[LocalizedSummary(nameof(Summaries.UnbanSummary))]
	[Meta("417e9dd0-306b-4d1f-8b62-0427f01f921a", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			IBan ban,
			[Remainder]
			ModerationReason reason = default
		)
		{
			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, ban.User.Id, false)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.Unbanned(ban.User);
		}
	}

	[Command(nameof(Names.VoiceMute), nameof(Names.VoiceMuteAlias))]
	[LocalizedSummary(nameof(Summaries.VoiceMuteSummary))]
	[Meta("a51ea911-10be-4e40-8995-a507015a7e57", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MuteMembers)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var isGive = !user.IsMuted;
			await PunishmentService.PunishAsync(new Punishments.VoiceMute(user, isGive)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.VoiceMuted(isGive, user, reason.Time);
		}
	}
}