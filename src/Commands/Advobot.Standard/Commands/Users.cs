using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Modules;
using Advobot.Standard.Localization;
using Advobot.Standard.Resources;
using Advobot.TypeReaders;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands
{
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
			[Command, Priority(1)]
			public Task Command(
				[CanModifyUser] IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
				=> Command(user.Id, reason);

			[Command]
			public async Task<RuntimeResult> Command(
				[NotBanned] ulong userId,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.BanAsync(Context.Guild, userId, options: punishmentArgs).CAF();
				var user = await Context.Client.Rest.GetUserAsync(userId).CAF();
				return Responses.Users.Banned(true, user, punishmentArgs);
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
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				var shouldPunish = !user.IsDeafened;
				if (shouldPunish)
				{
					await PunishmentUtils.DeafenAsync(user, punishmentArgs).CAF();
				}
				else
				{
					await PunishmentUtils.RemoveDeafenAsync(user, punishmentArgs).CAF();
				}
				return Responses.Users.Deafened(shouldPunish, user, punishmentArgs);
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
				var bans = await Context.Guild.GetBansAsync().CAF();
				return Responses.Users.DisplayBans(bans);
			}
		}

		[LocalizedGroup(nameof(Groups.ForAllWithRole))]
		[LocalizedAlias(nameof(Aliases.ForAllWithRole))]
		[LocalizedSummary(nameof(Summaries.ForAllWithRole))]
		[Meta("0dd92f6d-e4ad-4c80-82f0-da6c3e02743c", IsEnabled = true)]
		[RequireGuildPermissions]
		public sealed class ForAllWithRole : MultiUserActionModule
		{
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> ClearNickname(
				IRole target,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = u.Username, GenerateRequestOptions())));

			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveNickname(
				IRole target,
				[Nickname] string nickname,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = nickname, GenerateRequestOptions())));

			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveRole(
				IRole target,
				[CanModifyRole, NotEveryone, NotManaged] IRole give,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				if (target.Id == give.Id)
				{
					return Responses.Users.CannotGiveGatheredRole();
				}
				return CommandRunner(target, bypass, u => u.AddRoleAsync(give, GenerateRequestOptions()));
			}

			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> TakeRole(
				IRole target,
				[CanModifyRole, NotEveryone, NotManaged] IRole take,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, u => u.RemoveRoleAsync(take, GenerateRequestOptions()));

			private Func<IGuildUser, Task> CanModify(Func<IGuildUser, Task> func)
			{
				return async u =>
				{
					var bot = await u.Guild.GetCurrentUserAsync().CAF();
					if (bot.CanModify(bot.Id, u))
					{
						await func(u).CAF();
					}
				};
			}

			private async Task<RuntimeResult> CommandRunner(IRole role, bool bypass, Func<IGuildUser, Task> update)
			{
				static string CreateResult(MultiUserActionProgressArgs i)
					=> Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason;
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, CreateResult, GenerateRequestOptions());

				var amountChanged = await ProcessAsync(bypass, u => u.RoleIds.Contains(role.Id), update).CAF();
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
				[CanModifyUser] IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.KickAsync(user, punishmentArgs).CAF();
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
				[CanBeMoved] IGuildUser user,
				[CanModifyChannel(MoveMembers)] IVoiceChannel channel)
			{
				if (user.VoiceChannel?.Id == channel.Id)
				{
					return Responses.Users.AlreadyInChannel(user, channel);
				}

				await user.ModifyAsync(x => x.Channel = Optional.Create(channel), GenerateRequestOptions()).CAF();
				return Responses.Users.Moved(user, channel);
			}
		}

		[LocalizedGroup(nameof(Groups.MoveUsers))]
		[LocalizedAlias(nameof(Aliases.MoveUsers))]
		[LocalizedSummary(nameof(Summaries.MoveUsers))]
		[Meta("4e8439fa-cc29-4acb-9049-89865be825c8", IsEnabled = true)]
		[RequireGuildPermissions(GuildPermission.MoveMembers)]
		public sealed class MoveUsers : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[CanModifyChannel(MoveMembers)] IVoiceChannel input,
				[CanModifyChannel(MoveMembers)] IVoiceChannel output,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Users.MultiUserActionProgress(i.AmountLeft).Reason, GenerateRequestOptions());
				var users = await input.GetUsersAsync().FlattenAsync().CAF();
				var amountChanged = await ProcessAsync(users, bypass,
					u => true,
					u => u.ModifyAsync(x => x.Channel = Optional.Create(output), GenerateRequestOptions())).CAF();
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
			private static readonly OverwritePermissions TextPerms = new OverwritePermissions(0, (ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| SendMessages
				| ManageMessages
				| AddReactions));

			private static readonly OverwritePermissions VoicePerms = new OverwritePermissions(0, (ulong)(0
				| CreateInstantInvite
				| ManageChannels
				| ManageRoles
				| ManageWebhooks
				| Speak
				| MuteMembers
				| DeafenMembers
				| MoveMembers));

			[Command]
			public async Task<RuntimeResult> Command(
				IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
			{
				var muteRole = await GetOrCreateMuteRoleAsync().CAF();

				var punishmentArgs = reason.ToPunishmentArgs(this);
				var shouldPunish = !user.RoleIds.Contains(muteRole.Id);
				if (shouldPunish)
				{
					await PunishmentUtils.RoleMuteAsync(user, muteRole, punishmentArgs).CAF();
				}
				else
				{
					await PunishmentUtils.RemoveRoleMuteAsync(user, muteRole, punishmentArgs).CAF();
				}
				return Responses.Users.Muted(shouldPunish, user, punishmentArgs);
			}

			private async Task<IRole> GetOrCreateMuteRoleAsync()
			{
				IRole muteRole = Context.Guild.GetRole(Context.Settings.MuteRoleId);
				var result = await Context.User.ValidateRole(muteRole).CAF();
				if (!result.IsSuccess)
				{
					muteRole = await Context.Guild.CreateRoleAsync("Advobot Mute", new GuildPermissions(0)).CAF();
					Context.Settings.MuteRoleId = muteRole.Id;
					Context.Settings.Save();
				}

				foreach (var textChannel in Context.Guild.TextChannels)
				{
					if (textChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await textChannel.AddPermissionOverwriteAsync(muteRole, TextPerms).CAF();
					}
				}
				foreach (var voiceChannel in Context.Guild.VoiceChannels)
				{
					if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await voiceChannel.AddPermissionOverwriteAsync(muteRole, VoicePerms).CAF();
					}
				}
				return muteRole;
			}
		}

		[LocalizedGroup(nameof(Groups.PruneUsers))]
		[LocalizedAlias(nameof(Aliases.PruneUsers))]
		[LocalizedSummary(nameof(Summaries.PruneUsers))]
		[Meta("89abd319-c597-4e1a-9397-4f7d079f4e0e", IsEnabled = true)]
		[RequireGuildPermissions]
		public sealed class PruneUsers : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> FakePrune([PruneDays] int days)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: true, GenerateRequestOptions()).CAF();
				return Responses.Users.FakePruned(days, amt);
			}

			[ImplicitCommand] //No alias on purpose
			public async Task<RuntimeResult> RealPrune([PruneDays] int days)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: false, GenerateRequestOptions()).CAF();
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
			[Command]
			[RequireChannelPermissions(ManageMessages)]
			public Task<RuntimeResult> Command(
				[Positive] int requestCount)
				=> CommandRunner(requestCount, Context.Channel, null);

			[Command]
			[RequireChannelPermissions(ManageMessages)]
			public Task<RuntimeResult> Command(
				[Positive] int requestCount,
				IGuildUser user)
				=> CommandRunner(requestCount, Context.Channel, user);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive] int requestCount,
				[CanModifyChannel(ManageMessages)] ITextChannel channel)
				=> CommandRunner(requestCount, channel, null);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive] int requestCount,
				IGuildUser user,
				[CanModifyChannel(ManageMessages)] ITextChannel channel)
				=> CommandRunner(requestCount, channel, user);

			[Command]
			public Task<RuntimeResult> Command(
				[Positive] int requestCount,
				[CanModifyChannel(ManageMessages)] ITextChannel channel,
				IGuildUser user)
				=> CommandRunner(requestCount, channel, user);

			private async Task<RuntimeResult> CommandRunner(
				int req,
				ITextChannel channel,
				IUser? user)
			{
				//If not the context channel then get the first message in that channel
				var thisChannel = Context.Message.Channel.Id == channel.Id;
				IMessage start = Context.Message;
				if (!thisChannel)
				{
					var msgs = await channel.GetMessagesAsync(1).FlattenAsync().CAF();
					start = msgs.FirstOrDefault();
				}

				//If there is a non null user then delete messages specifically from that user
				Func<IMessage, bool>? predicate = null;
				if (user != null)
				{
					predicate = x => x.Author.Id == user?.Id;
				}
				var deleted = await MessageUtils.DeleteMessagesAsync(channel, start, req, GenerateRequestOptions(), predicate).CAF();

				//If the context channel isn't the targetted channel then delete the start message
				//Increase by one to account for it not being targetted.
				if (!thisChannel)
				{
					await start.DeleteAsync(GenerateRequestOptions()).CAF();
					++deleted;
				}

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
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[CanModifyUser] IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
				=> Command(user.Id, reason);

			[Command]
			public async Task<RuntimeResult> Command(
				[NotBanned] ulong userId,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.SoftbanAsync(Context.Guild, userId, punishmentArgs).CAF();
				return Responses.Users.SoftBanned(Context.Client.GetUser(userId));
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
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.UnbanAsync(Context.Guild, ban.User.Id, punishmentArgs).CAF();
				return Responses.Users.Banned(false, ban.User, punishmentArgs);
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
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				var shouldPunish = !user.IsMuted;
				if (shouldPunish)
				{
					await PunishmentUtils.VoiceMuteAsync(user, punishmentArgs).CAF();
				}
				else
				{
					await PunishmentUtils.RemoveVoiceMuteAsync(user, punishmentArgs).CAF();
				}
				return Responses.Users.VoiceMuted(shouldPunish, user, punishmentArgs);
			}
		}
	}
}