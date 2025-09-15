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
using Advobot.Services.Time;
using Advobot.Utilities;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Commands.Building;
using YACCS.Localization;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Users))]
public sealed class Users : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.Ban), nameof(Aliases.Ban))]
	[LocalizedSummary(nameof(Summaries.Ban))]
	[Id("b798e679-3ca7-4af1-9544-585672ec9936")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Ban : AdvobotModuleBase
	{
		[LocalizedCommand]
		[Priority(1)]
		public Task<AdvobotResult> Command(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => CommandRunner(user, user.Id, reason);

		[Command]
		[Priority(0)]
		public Task<AdvobotResult> Command(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		) => CommandRunner(null, userId, reason);

		private async Task<AdvobotResult> CommandRunner(
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

	[LocalizedCommand(nameof(Groups.Deafen), nameof(Aliases.Deafen))]
	[LocalizedSummary(nameof(Summaries.Deafen))]
	[Id("99aa7f17-5710-41ce-ba12-291c2971c0a4")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.DeafenMembers)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(
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

	[LocalizedCommand(nameof(Groups.ForAllWithRole), nameof(Aliases.ForAllWithRole))]
	[LocalizedSummary(nameof(Summaries.ForAllWithRole))]
	[Id("0dd92f6d-e4ad-4c80-82f0-da6c3e02743c")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions]
	public sealed class ForAllWithRole : MultiUserActionModuleBase
	{
		[LocalizedCommand(nameof(Groups.ClearNickname), nameof(Aliases.ClearNickname))]
		public Task<AdvobotResult> ClearNickname(
			IRole target,
			bool getUnlimitedUsers = false
		)
		{
			return CommandRunner(target, getUnlimitedUsers, (user, options) =>
			{
				if (user.Nickname != null)
				{
					return user.ModifyAsync(x => x.Nickname = user.Username, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.GiveNickname), nameof(Aliases.GiveNickname))]
		public Task<AdvobotResult> GiveNickname(
			IRole target,
			[Nickname]
			string nickname,
			bool getUnlimitedUsers = false
		)
		{
			return CommandRunner(target, getUnlimitedUsers, (user, options) =>
			{
				if (user.Nickname != nickname)
				{
					return user.ModifyAsync(x => x.Nickname = nickname, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.GiveRole), nameof(Aliases.GiveRole))]
		public Task<AdvobotResult> GiveRole(
			IRole target,
			[CanModifyRole, NotEveryone, NotManaged]
			IRole give,
			bool getUnlimitedUsers = false
		)
		{
			if (target.Id == give.Id)
			{
				return Responses.Users.CannotGiveGatheredRole();
			}

			return CommandRunner(target, getUnlimitedUsers, (user, options) =>
			{
				if (!user.RoleIds.Contains(give.Id))
				{
					return user.AddRoleAsync(give, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.TakeRole), nameof(Aliases.TakeRole))]
		public Task<AdvobotResult> TakeRole(
			IRole target,
			[CanModifyRole, NotEveryone, NotManaged]
			IRole take,
			bool getUnlimitedUsers = false
		)
		{
			return CommandRunner(target, getUnlimitedUsers, (user, options) =>
			{
				if (user.RoleIds.Contains(take.Id))
				{
					return user.RemoveRoleAsync(take, options);
				}
				return Task.CompletedTask;
			});
		}

		private async Task<AdvobotResult> CommandRunner(
			IRole role,
			bool getUnlimitedUsers,
			Func<IGuildUser, RequestOptions, Task> update)
		{
			var amountChanged = await ProcessAsync(
				getUnlimitedUsers,
				u => u.RoleIds.Contains(role.Id),
				update,
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Response,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedCommand(nameof(Groups.Kick), nameof(Aliases.Kick))]
	[LocalizedSummary(nameof(Summaries.Kick))]
	[Id("1d86aa7d-da06-478c-861b-a62ca279523b")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.KickMembers)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(
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

	[LocalizedCommand(nameof(Groups.MoveUser), nameof(Aliases.MoveUser))]
	[LocalizedSummary(nameof(Summaries.MoveUser))]
	[Id("158dca6d-fc89-43b3-a6b5-d055f6672547")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(
			[CanBeMoved]
			IGuildUser user,
			[CanModifyChannel(MoveMembers)]
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

	[LocalizedCommand(nameof(Groups.MoveUsers), nameof(Aliases.MoveUsers))]
	[LocalizedSummary(nameof(Summaries.MoveUsers))]
	[Id("4e8439fa-cc29-4acb-9049-89865be825c8")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUsers : MultiUserActionModuleBase
	{
		[LocalizedCommand]
		public async Task<AdvobotResult> Command(
			[CanModifyChannel(MoveMembers)]
			IVoiceChannel input,
			[CanModifyChannel(MoveMembers)]
			IVoiceChannel output,
			bool getUnlimitedUsers = false
		)
		{
			var users = await input.GetUsersAsync().FlattenAsync().ConfigureAwait(false);
			var amountChanged = await ProcessAsync(
				users,
				getUnlimitedUsers,
				_ => true,
				(u, o) => u.ModifyAsync(x => x.Channel = new(output), o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Response,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedCommand(nameof(Groups.Mute), nameof(Aliases.Mute))]
	[LocalizedSummary(nameof(Summaries.Mute))]
	[Id("b9f305d4-d343-4350-a140-c54a42af8d8d")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
	public sealed class Mute : AdvobotModuleBase
	{
		private static readonly OverwritePermissions CategoryPerms = new(
			0,
			TextPerms.DenyValue | VoicePerms.DenyValue
		);
		private static readonly OverwritePermissions TextPerms = new(
			0,
			(ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| SendMessages
				| ManageMessages
				| AddReactions
			)
		);
		private static readonly OverwritePermissions VoicePerms = new(
			0,
			(ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| Speak
				| MuteMembers
				| DeafenMembers
				| MoveMembers
			)
		);

		[InjectService]
		public required IGuildSettingsService GuildSettings { get; set; }

		[Command]
		public async Task<AdvobotResult> Command(
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

	[LocalizedCommand(nameof(Groups.RemoveMessages), nameof(Aliases.RemoveMessages))]
	[LocalizedSummary(nameof(Summaries.RemoveMessages))]
	[Id("a4f3959e-1f56-4bf0-b377-dc98ef017906")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageMessages)]
	public sealed class RemoveMessages : AdvobotModuleBase
	{
		[InjectService]
		public required ITimeService Time { get; set; }

		[Command]
		[RequireChannelPermissions(ManageMessages)]
		public Task<AdvobotResult> Command(
			[Positive]
			int deleteCount
		) => CommandRunner(deleteCount, Context.Channel, null);

		[Command]
		[RequireChannelPermissions(ManageMessages)]
		public Task<AdvobotResult> Command(
			[Positive]
			int deleteCount,
			IGuildUser user
		) => CommandRunner(deleteCount, Context.Channel, user);

		[Command]
		public Task<AdvobotResult> Command(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel
		) => CommandRunner(deleteCount, channel, null);

		[Command]
		public Task<AdvobotResult> Command(
			[Positive]
			int deleteCount,
			IGuildUser user,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel
		) => CommandRunner(deleteCount, channel, user);

		[Command]
		public Task<AdvobotResult> Command(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel,
			IGuildUser user
		) => CommandRunner(deleteCount, channel, user);

		private async Task<AdvobotResult> CommandRunner(
			int deleteCount,
			ITextChannel channel,
			IUser? user)
		{
			var deleted = await channel.DeleteMessagesAsync(new DeleteMessageArgs
			{
				Now = Time.UtcNow,
				DeleteCount = deleteCount,
				Options = GetOptions(),
				FromMessage = Context.Message.Channel.Id == channel.Id
					? Context.Message
					: null,
				Predicate = user is null
					? null
					: x => x.Author.Id == user?.Id,
			}).ConfigureAwait(false);
			return Responses.Users.RemovedMessages(channel, user, deleted);
		}
	}

	[LocalizedCommand(nameof(Groups.SoftBan), nameof(Aliases.SoftBan))]
	[LocalizedSummary(nameof(Summaries.SoftBan))]
	[Id("a6084728-77bf-469c-af09-41e53ac021d9")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers, GuildPermission.KickMembers)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command]
		[Priority(1)]
		public Task<AdvobotResult> Command(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => Command(user.Id, reason);

		[Command]
		public async Task<AdvobotResult> Command(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		)
		{
			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, userId, true)
			{
				Duration = reason.Time,
			}, GetOptions(reason.Reason)).ConfigureAwait(false);
			return Responses.Users.SoftBanned(userId);
		}
	}

	[LocalizedCommand(nameof(Groups.Unban), nameof(Aliases.Unban))]
	[LocalizedSummary(nameof(Summaries.Unban))]
	[Id("417e9dd0-306b-4d1f-8b62-0427f01f921a")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(
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

	[LocalizedCommand(nameof(Groups.VoiceMute), nameof(Aliases.VoiceMute))]
	[LocalizedSummary(nameof(Summaries.VoiceMute))]
	[Id("a51ea911-10be-4e40-8995-a507015a7e57")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MuteMembers)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Command(
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