using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using Advobot.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Standard
{
	public sealed class Users : ModuleBase
	{
		[Group(nameof(Mute)), ModuleInitialismAlias(typeof(Mute))]
		[LocalizedSummary(nameof(Summaries.Mute))]
		[UserPermissionRequirement(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
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
				var existingMuteRole = Context.Guild.GetRole(Context.Settings.MuteRoleId);
				IRole muteRole = existingMuteRole;
				var validationResult = await Context.User.ValidateRole(existingMuteRole, ValidationUtils.RoleIsNotEveryone, ValidationUtils.RoleIsNotManaged).CAF();
				if (!validationResult.IsSuccess)
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

		[Group(nameof(VoiceMute)), ModuleInitialismAlias(typeof(VoiceMute))]
		[LocalizedSummary(nameof(Summaries.VoiceMute))]
		[UserPermissionRequirement(GuildPermission.MuteMembers)]
		[EnabledByDefault(true)]
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

		[Group(nameof(Deafen)), ModuleInitialismAlias(typeof(Deafen))]
		[LocalizedSummary(nameof(Summaries.Deafen))]
		[UserPermissionRequirement(GuildPermission.DeafenMembers)]
		[EnabledByDefault(true)]
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

		[Group(nameof(Kick)), ModuleInitialismAlias(typeof(Kick))]
		[LocalizedSummary(nameof(Summaries.Kick))]
		[UserPermissionRequirement(GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class Kick : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[ValidateUser] IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.KickAsync(user, punishmentArgs).CAF();
				return Responses.Users.Kicked(user);
			}
		}

		[Group(nameof(SoftBan)), ModuleInitialismAlias(typeof(SoftBan))]
		[LocalizedSummary(nameof(Summaries.SoftBan))]
		[UserPermissionRequirement(GuildPermission.BanMembers, GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class SoftBan : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task<RuntimeResult> Command(
				[ValidateUser] IGuildUser user,
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

		[Group(nameof(Ban)), ModuleInitialismAlias(typeof(Ban))]
		[LocalizedSummary(nameof(Summaries.Ban))]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Ban : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task Command(
				[ValidateUser] IGuildUser user,
				[Optional, Remainder] ModerationReason reason)
				=> Command(user.Id, reason);
			[Command]
			public async Task<RuntimeResult> Command(
				[NotBanned] ulong userId,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.BanAsync(Context.Guild, userId, options: punishmentArgs).CAF();
				return Responses.Users.Banned(Context.Client.GetUser(userId));
			}
		}

		[Group(nameof(Unban)), ModuleInitialismAlias(typeof(Unban))]
		[LocalizedSummary(nameof(Summaries.Unban))]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Unban : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				IBan ban,
				[Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.UnbanAsync(Context.Guild, ban.User.Id, punishmentArgs).CAF();
				return Responses.Users.Unbanned(ban);
			}
		}

		[Group(nameof(MoveUser)), ModuleInitialismAlias(typeof(MoveUser))]
		[LocalizedSummary(nameof(Summaries.MoveUser))]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUser : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(
				[CanBeMoved] IGuildUser user,
				[ValidateVoiceChannel(MoveMembers)] IVoiceChannel channel)
			{
				if (user.VoiceChannel?.Id == channel.Id)
				{
					return Responses.Users.AlreadyInChannel(user, channel);
				}

				await user.ModifyAsync(x => x.Channel = Optional.Create<IVoiceChannel>(channel), GenerateRequestOptions()).CAF();
				return Responses.Users.Moved(user, channel);
			}
		}

		[Group(nameof(MoveUsers)), ModuleInitialismAlias(typeof(MoveUsers))]
		[LocalizedSummary(nameof(Summaries.MoveUsers))]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUsers : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command(
				[ValidateVoiceChannel(MoveMembers)] IVoiceChannel input,
				[ValidateVoiceChannel(MoveMembers)] IVoiceChannel output,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Users.MultiUserAction(i.AmountLeft).Reason, GenerateRequestOptions());
				var users = await input.GetUsersAsync().FlattenAsync().CAF();
				var amountChanged = await ProcessAsync(users, bypass,
					u => true,
					u => u.ModifyAsync(x => x.Channel = Optional.Create(output), GenerateRequestOptions())).CAF();
				return Responses.Users.MultiUserActionSuccess(amountChanged);
			}
		}

		[Group(nameof(PruneUsers)), ModuleInitialismAlias(typeof(PruneUsers))]
		[LocalizedSummary(nameof(Summaries.PruneUsers))]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class PruneUsers : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> RealPrune([ValidatePruneDays] int days)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: false, GenerateRequestOptions()).CAF();
				return Responses.Users.Pruned(days, amt);
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task<RuntimeResult> FakePrune([ValidatePruneDays] int days)
			{
				var amt = await Context.Guild.PruneUsersAsync(days, simulate: true, GenerateRequestOptions()).CAF();
				return Responses.Users.FakePruned(days, amt);
			}
		}

		[Group(nameof(GetBanReason)), ModuleInitialismAlias(typeof(GetBanReason))]
		[LocalizedSummary(nameof(Summaries.GetBanReason))]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class GetBanReason : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(IBan ban)
				=> Responses.Users.DisplayBanReason(ban);
		}

		[Group(nameof(DisplayCurrentBanList)), ModuleInitialismAlias(typeof(DisplayCurrentBanList))]
		[LocalizedSummary(nameof(Summaries.DisplayCurrentBanList))]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class DisplayCurrentBanList : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				var bans = await Context.Guild.GetBansAsync().CAF();
				return Responses.Users.DisplayBans(bans);
			}
		}

		[Group(nameof(RemoveMessages)), ModuleInitialismAlias(typeof(RemoveMessages))]
		[LocalizedSummary(nameof(Summaries.RemoveMessages))]
		[UserPermissionRequirement(GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
		public sealed class RemoveMessages : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[ValidatePositiveNumber] int requestCount,
				[Optional] IGuildUser user,
				[Optional, ValidateTextChannel(ManageMessages, FromContext = true)] ITextChannel channel)
				=> Command(requestCount, channel, user);
			[Command]
			public async Task<RuntimeResult> Command(
				[ValidatePositiveNumber] int requestCount,
				[Optional, ValidateTextChannel(ManageMessages, FromContext = true)] ITextChannel channel,
				[Optional] IGuildUser user)
			{
				channel ??= Context.Channel;

				//If not the context channel then get the first message in that channel
				var thisChannel = Context.Message.Channel.Id == channel.Id;
				var startMsg = thisChannel
					? Context.Message
					: (await channel.GetMessagesAsync(1).FlattenAsync().CAF()).FirstOrDefault();

				//If there is a non null user then delete messages specifically from that user
				var predicate = user == null ? default(Func<IMessage, bool>) : x => x.Author.Id == user?.Id;
				var deletedAmt = await MessageUtils.DeleteMessagesAsync(channel, startMsg, requestCount, GenerateRequestOptions(), predicate).CAF();

				//If the context channel isn't the targetted channel then delete the start message
				//Increase by one to account for it not being targetted.
				if (!thisChannel)
				{
					await startMsg.DeleteAsync(GenerateRequestOptions()).CAF();
					deletedAmt++;
				}

				return Responses.Users.RemovedMessages(channel, user, deletedAmt);
			}
		}

		[Group(nameof(ForAllWithRole)), ModuleInitialismAlias(typeof(ForAllWithRole))]
		[LocalizedSummary(nameof(Summaries.ForAllWithRole))]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class ForAllWithRole : MultiUserActionModule
		{
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveRole(
				IRole target,
				[NotEveryoneOrManaged] IRole give,
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
				[NotEveryoneOrManaged] IRole take,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, u => u.RemoveRoleAsync(take, GenerateRequestOptions()));
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveNickname(
				[ValidateRole] IRole target,
				[ValidateNickname] string nickname,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = nickname, GenerateRequestOptions())));
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> ClearNickname(
				[ValidateRole] IRole target,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = u.Username, GenerateRequestOptions())));

			private async Task<RuntimeResult> CommandRunner(IRole role, bool bypass, Func<IGuildUser, Task> update)
			{
				static string CreateResult(MultiUserActionProgressArgs i)
					=> Responses.Users.MultiUserAction(i.AmountLeft).Reason;
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, CreateResult, GenerateRequestOptions());

				var amountChanged = await ProcessAsync(bypass, u => u.RoleIds.Contains(role.Id), update).CAF();
				return Responses.Users.MultiUserActionSuccess(amountChanged);
			}
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
		}
	}
}
