using Advobot.Attributes;
using Advobot.Localization;
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
using Advobot.TypeReaders;
using Advobot.Utilities;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[Category(nameof(Users))]
public sealed class Users : ModuleBase
{
	[LocalizedGroup(nameof(Groups.Ban))]
	[LocalizedAlias(nameof(Aliases.Ban))]
	[LocalizedSummary(nameof(Summaries.Ban))]
	[Meta("b798e679-3ca7-4af1-9544-585672ec9936", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Ban : AdvobotModuleBase
	{
		[Command]
		[Priority(1)]
		public Task Command(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => Command(user.Id, reason);

		[Command]
		[Priority(0)]
		public async Task<RuntimeResult> Command(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var user = await Context.Client.GetUserAsync(userId).ConfigureAwait(false);
			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, userId, true)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Banned(true, user!, reason.Time);
		}
	}

	[LocalizedGroup(nameof(Groups.Deafen))]
	[LocalizedAlias(nameof(Aliases.Deafen))]
	[LocalizedSummary(nameof(Summaries.Deafen))]
	[Meta("99aa7f17-5710-41ce-ba12-291c2971c0a4", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.DeafenMembers)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var isGive = !user.IsDeafened;
			await PunishmentService.PunishAsync(new Punishments.Deafen(user, isGive)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Deafened(isGive, user, reason.Time);
		}
	}

	[LocalizedGroup(nameof(Groups.DisplayCurrentBanList))]
	[LocalizedAlias(nameof(Aliases.DisplayCurrentBanList))]
	[LocalizedSummary(nameof(Summaries.DisplayCurrentBanList))]
	[Meta("419c1846-b232-475e-aa19-45cb282dc9e0", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class DisplayCurrentBanList : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command()
		{
			var bans = new List<IBan>();
			await foreach (var list in Context.Guild.GetBansAsync(int.MaxValue))
			{
				bans.AddRange(list);
			}
			return Responses.Users.DisplayBans(bans);
		}
	}

	[LocalizedGroup(nameof(Groups.ForAllWithRole))]
	[LocalizedAlias(nameof(Aliases.ForAllWithRole))]
	[LocalizedSummary(nameof(Summaries.ForAllWithRole))]
	[Meta("0dd92f6d-e4ad-4c80-82f0-da6c3e02743c", IsEnabled = true)]
	[RequireGuildPermissions]
	public sealed class ForAllWithRole : MultiUserActionModuleBase
	{
		[LocalizedCommand(nameof(Groups.ClearNickname), RunMode = RunMode.Async)]
		[LocalizedAlias(nameof(Aliases.ClearNickname))]
		public Task<RuntimeResult> ClearNickname(
			IRole target,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			return CommandRunner(target, bypass, (user, options) =>
			{
				if (user.Nickname != null)
				{
					return user.ModifyAsync(x => x.Nickname = user.Username, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.GiveNickname), RunMode = RunMode.Async)]
		[LocalizedAlias(nameof(Aliases.GiveNickname))]
		public Task<RuntimeResult> GiveNickname(
			IRole target,
			[Nickname]
			string nickname,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			return CommandRunner(target, bypass, (user, options) =>
			{
				if (user.Nickname != nickname)
				{
					return user.ModifyAsync(x => x.Nickname = nickname, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.GiveRole), RunMode = RunMode.Async)]
		[LocalizedAlias(nameof(Aliases.GiveRole))]
		public Task<RuntimeResult> GiveRole(
			IRole target,
			[CanModifyRole, NotEveryone, NotManaged]
			IRole give,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			if (target.Id == give.Id)
			{
				return Responses.Users.CannotGiveGatheredRole();
			}

			return CommandRunner(target, bypass, (user, options) =>
			{
				if (!user.RoleIds.Contains(give.Id))
				{
					return user.AddRoleAsync(give, options);
				}
				return Task.CompletedTask;
			});
		}

		[LocalizedCommand(nameof(Groups.TakeRole), RunMode = RunMode.Async)]
		[LocalizedAlias(nameof(Aliases.TakeRole))]
		public Task<RuntimeResult> TakeRole(
			IRole target,
			[CanModifyRole, NotEveryone, NotManaged]
			IRole take,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass = false
		)
		{
			return CommandRunner(target, bypass, (user, options) =>
			{
				if (user.RoleIds.Contains(take.Id))
				{
					return user.RemoveRoleAsync(take, options);
				}
				return Task.CompletedTask;
			});
		}

		private async Task<RuntimeResult> CommandRunner(
			IRole role,
			bool bypass,
			Func<IGuildUser, RequestOptions, Task> update)
		{
			var amountChanged = await ProcessAsync(
				bypass,
				u => u.RoleIds.Contains(role.Id),
				update,
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedGroup(nameof(Groups.GetBanReason))]
	[LocalizedAlias(nameof(Aliases.GetBanReason))]
	[LocalizedSummary(nameof(Summaries.GetBanReason))]
	[Meta("5ba658c2-c689-4b3a-b7f1-77329dd6e971", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class GetBanReason : AdvobotModuleBase
	{
		[Command]
		public Task<RuntimeResult> Command(IBan ban)
			=> Responses.Users.DisplayBanReason(ban);
	}

	[LocalizedGroup(nameof(Groups.Kick))]
	[LocalizedAlias(nameof(Aliases.Kick))]
	[LocalizedSummary(nameof(Summaries.Kick))]
	[Meta("1d86aa7d-da06-478c-861b-a62ca279523b", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.KickMembers)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			await PunishmentService.PunishAsync(new Punishments.Kick(user)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Kicked(user);
		}
	}

	[LocalizedGroup(nameof(Groups.MoveUser))]
	[LocalizedAlias(nameof(Aliases.MoveUser))]
	[LocalizedSummary(nameof(Summaries.MoveUser))]
	[Meta("158dca6d-fc89-43b3-a6b5-d055f6672547", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
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

			await user.ModifyAsync(x => x.Channel = Optional.Create(channel), GetOptions()).ConfigureAwait(false);
			return Responses.Users.Moved(user, channel);
		}
	}

	[LocalizedGroup(nameof(Groups.MoveUsers))]
	[LocalizedAlias(nameof(Aliases.MoveUsers))]
	[LocalizedSummary(nameof(Summaries.MoveUsers))]
	[Meta("4e8439fa-cc29-4acb-9049-89865be825c8", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MoveMembers)]
	public sealed class MoveUsers : MultiUserActionModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task<RuntimeResult> Command(
			[CanModifyChannel(MoveMembers)]
			IVoiceChannel input,
			[CanModifyChannel(MoveMembers)]
			IVoiceChannel output,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))]
			bool bypass
		)
		{
			var users = await input.GetUsersAsync().FlattenAsync().ConfigureAwait(false);
			var amountChanged = await ProcessAsync(
				users,
				bypass,
				_ => true,
				(u, o) => u.ModifyAsync(x => x.Channel = Optional.Create(output), o),
				i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason,
				GetOptions()
			).ConfigureAwait(false);
			return Responses.Users.MultiUserActionSuccess(amountChanged);
		}
	}

	[LocalizedGroup(nameof(Groups.Mute))]
	[LocalizedAlias(nameof(Aliases.Mute))]
	[LocalizedSummary(nameof(Summaries.Mute))]
	[Meta("b9f305d4-d343-4350-a140-c54a42af8d8d", IsEnabled = true)]
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

		public required IGuildSettingsService GuildSettings { get; set; }

		[Command]
		public async Task<RuntimeResult> Command(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var role = await GuildSettings.GetMuteRoleAsync(Context.Guild).ConfigureAwait(false);
			await ConfigureMuteRoleAsync(role).ConfigureAwait(false);

			var isGive = !user.RoleIds.Contains(role.Id);
			await PunishmentService.PunishAsync(new Punishments.RoleMute(user, isGive, role)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Muted(isGive, user, reason.Time);
		}

		private async Task ConfigureMuteRoleAsync(IRole role)
		{
			if (role.Permissions.RawValue != 0)
			{
				await role.ModifyAsync(x => x.Permissions = new GuildPermissions(0)).ConfigureAwait(false);
			}

			static async Task ConfigureChannelsAsync(
				IRole role,
				IReadOnlyCollection<IGuildChannel> channels,
				OverwritePermissions perms)
			{
				foreach (var c in channels)
				{
					if (c.GetPermissionOverwrite(role)?.DenyValue != perms.DenyValue)
					{
						await c.AddPermissionOverwriteAsync(role, perms).ConfigureAwait(false);
					}
				}
			}

			await ConfigureChannelsAsync(role, Context.Guild.CategoryChannels, CategoryPerms).ConfigureAwait(false);
			await ConfigureChannelsAsync(role, Context.Guild.TextChannels, TextPerms).ConfigureAwait(false);
			await ConfigureChannelsAsync(role, Context.Guild.VoiceChannels, VoicePerms).ConfigureAwait(false);
		}
	}

	[LocalizedGroup(nameof(Groups.PruneUsers))]
	[LocalizedAlias(nameof(Aliases.PruneUsers))]
	[LocalizedSummary(nameof(Summaries.PruneUsers))]
	[Meta("89abd319-c597-4e1a-9397-4f7d079f4e0e", IsEnabled = true)]
	[RequireGuildPermissions]
	public sealed class PruneUsers : AdvobotModuleBase
	{
		[LocalizedCommand(nameof(Groups.Fake))]
		[LocalizedAlias(nameof(Aliases.Fake))]
		public async Task<RuntimeResult> Fake(
			[PruneDays]
			int days
		)
		{
			var amt = await Context.Guild.PruneUsersAsync(days, simulate: true, GetOptions()).ConfigureAwait(false);
			return Responses.Users.FakePruned(days, amt);
		}

		[LocalizedCommand(nameof(Groups.Real))]
		public async Task<RuntimeResult> Real(
			[PruneDays]
			int days
		)
		{
			var amt = await Context.Guild.PruneUsersAsync(days, simulate: false, GetOptions()).ConfigureAwait(false);
			return Responses.Users.Pruned(days, amt);
		}
	}

	[LocalizedGroup(nameof(Groups.RemoveMessages))]
	[LocalizedAlias(nameof(Aliases.RemoveMessages))]
	[LocalizedSummary(nameof(Summaries.RemoveMessages))]
	[Meta("a4f3959e-1f56-4bf0-b377-dc98ef017906", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageMessages)]
	public sealed class RemoveMessages : AdvobotModuleBase
	{
		public required ITimeService Time { get; set; }

		[Command]
		[RequireChannelPermissions(ManageMessages)]
		public Task<RuntimeResult> Command(
			[Positive]
			int deleteCount
		) => CommandRunner(deleteCount, Context.Channel, null);

		[Command]
		[RequireChannelPermissions(ManageMessages)]
		public Task<RuntimeResult> Command(
			[Positive]
			int deleteCount,
			IUser user
		) => CommandRunner(deleteCount, Context.Channel, user);

		[Command]
		public Task<RuntimeResult> Command(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel
		) => CommandRunner(deleteCount, channel, null);

		[Command]
		public Task<RuntimeResult> Command(
			[Positive]
			int deleteCount,
			IUser user,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel
		) => CommandRunner(deleteCount, channel, user);

		[Command]
		public Task<RuntimeResult> Command(
			[Positive]
			int deleteCount,
			[CanModifyChannel(ManageMessages)]
			ITextChannel channel,
			IUser user
		) => CommandRunner(deleteCount, channel, user);

		private async Task<RuntimeResult> CommandRunner(
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

	[LocalizedGroup(nameof(Groups.SoftBan))]
	[LocalizedAlias(nameof(Aliases.SoftBan))]
	[LocalizedSummary(nameof(Summaries.SoftBan))]
	[Meta("a6084728-77bf-469c-af09-41e53ac021d9", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers, GuildPermission.KickMembers)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command]
		[Priority(1)]
		public Task<RuntimeResult> Command(
			[CanModifyUser]
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		) => Command(user.Id, reason);

		[Command]
		public async Task<RuntimeResult> Command(
			[NotBanned]
			ulong userId,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var user = await Context.Client.GetUserAsync(userId).ConfigureAwait(false);
			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, userId, true)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Banned(true, user!, reason.Time);
		}
	}

	[LocalizedGroup(nameof(Groups.Unban))]
	[LocalizedAlias(nameof(Aliases.Unban))]
	[LocalizedSummary(nameof(Summaries.Unban))]
	[Meta("417e9dd0-306b-4d1f-8b62-0427f01f921a", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.BanMembers)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			IBan ban,
			[Remainder]
			ModerationReason reason = default
		)
		{
			await PunishmentService.PunishAsync(new Punishments.Ban(Context.Guild, ban.User.Id, false)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Banned(false, ban.User, reason.Time);
		}
	}

	[LocalizedGroup(nameof(Groups.VoiceMute))]
	[LocalizedAlias(nameof(Aliases.VoiceMute))]
	[LocalizedSummary(nameof(Summaries.VoiceMute))]
	[Meta("a51ea911-10be-4e40-8995-a507015a7e57", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.MuteMembers)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(
			IGuildUser user,
			[Remainder]
			ModerationReason reason = default
		)
		{
			var isGive = !user.IsMuted;
			await PunishmentService.PunishAsync(new Punishments.Mute(user, isGive)
			{
				Duration = reason.Time,
				Options = GetOptions(reason.Reason),
			}).ConfigureAwait(false);
			return Responses.Users.Deafened(isGive, user, reason.Time);
		}
	}
}