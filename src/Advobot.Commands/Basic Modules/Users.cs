using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands
{
	public sealed class Users : ModuleBase
	{
		[Group(nameof(Mute)), ModuleInitialismAlias(typeof(Mute))]
		[Summary("Prevents a user from typing and speaking in the guild. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
		public sealed class Mute : AdvobotModuleBase
		{
			private const ChannelPermission MUTE_ROLE_TEXT_PERMS = 0
				| ChannelPermission.CreateInstantInvite
				| ChannelPermission.ManageChannels
				| ChannelPermission.ManageRoles
				| ChannelPermission.ManageWebhooks
				| ChannelPermission.SendMessages
				| ChannelPermission.ManageMessages
				| ChannelPermission.AddReactions;
			private const ChannelPermission MUTE_ROLE_VOICE_PERMS = 0
				| ChannelPermission.CreateInstantInvite
				| ChannelPermission.ManageChannels
				| ChannelPermission.ManageRoles
				| ChannelPermission.ManageWebhooks
				| ChannelPermission.Speak
				| ChannelPermission.MuteMembers
				| ChannelPermission.DeafenMembers
				| ChannelPermission.MoveMembers;

			[Command]
			public async Task<RuntimeResult> Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				var muteRole = await GetOrCreateMuteRoleAsync().CAF();

				var punishmentArgs = reason.ToPunishmentArgs(this);
				var shouldPunish = !user.Roles.Select(x => x.Id).Contains(muteRole.Id);
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
				var existingMuteRole = Context.Guild.GetRole(Context.GuildSettings.MuteRoleId);
				IRole muteRole = existingMuteRole;
				var validationResult = await Context.User.ValidateRole(existingMuteRole, ValidationUtils.RoleIsNotEveryone, ValidationUtils.RoleIsNotManaged).CAF();
				if (!validationResult.IsSuccess)
				{
					muteRole = await Context.Guild.CreateRoleAsync("Advobot Mute", new GuildPermissions(0)).CAF();
					Context.GuildSettings.MuteRoleId = muteRole.Id;
					Context.GuildSettings.Save(BotSettings);
				}

				foreach (var textChannel in Context.Guild.TextChannels)
				{
					if (textChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await textChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, (ulong)MUTE_ROLE_TEXT_PERMS)).CAF();
					}
				}
				foreach (var voiceChannel in Context.Guild.VoiceChannels)
				{
					if (voiceChannel.GetPermissionOverwrite(muteRole) == null)
					{
						await voiceChannel.AddPermissionOverwriteAsync(muteRole, new OverwritePermissions(0, (ulong)MUTE_ROLE_VOICE_PERMS)).CAF();
					}
				}
				return muteRole;
			}
		}

		[Group(nameof(VoiceMute)), ModuleInitialismAlias(typeof(VoiceMute))]
		[Summary("Prevents a user from speaking. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.MuteMembers)]
		[EnabledByDefault(true)]
		public sealed class VoiceMute : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
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
		[Summary("Prevents a user from hearing. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.DeafenMembers)]
		[EnabledByDefault(true)]
		public sealed class Deafen : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
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
		[Summary("Kicks the user from the guild.")]
		[UserPermissionRequirement(GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class Kick : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.KickAsync(user, punishmentArgs).CAF();
				return Responses.Users.Kicked(user);
			}
		}

		[Group(nameof(SoftBan)), ModuleInitialismAlias(typeof(SoftBan))]
		[Summary("Bans then unbans a user, which removes all recent messages from them.")]
		[UserPermissionRequirement(GuildPermission.BanMembers, GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class SoftBan : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task<RuntimeResult> Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
				=> Command(user.Id, reason);
			[Command]
			public async Task<RuntimeResult> Command([NotBanned] ulong userId, [Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.SoftbanAsync(Context.Guild, userId, punishmentArgs).CAF();
				return Responses.Users.SoftBanned(Context.Client.GetUser(userId));
			}
		}

		[Group(nameof(Ban)), ModuleInitialismAlias(typeof(Ban))]
		[Summary("Bans the user from the guild. " +
			"Time specifies how long and is in minutes.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Ban : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public Task Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
				=> Command(user.Id, reason);
			[Command]
			public async Task<RuntimeResult> Command([NotBanned] ulong userId, [Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.BanAsync(Context.Guild, userId, options: punishmentArgs).CAF();
				return Responses.Users.Banned(Context.Client.GetUser(userId));
			}
		}

		[Group(nameof(Unban)), ModuleInitialismAlias(typeof(Unban))]
		[Summary("Unbans the user from the guild.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Unban : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IBan ban, [Optional, Remainder] ModerationReason reason)
			{
				var punishmentArgs = reason.ToPunishmentArgs(this);
				await PunishmentUtils.UnbanAsync(Context.Guild, ban.User.Id, punishmentArgs).CAF();
				return Responses.Users.Unbanned(ban);
			}
		}

		[Group(nameof(MoveUser)), ModuleInitialismAlias(typeof(MoveUser))]
		[Summary("Moves the user to the given voice channel.")]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUser : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([CanBeMoved] SocketGuildUser user, [ValidateVoiceChannel(ChannelPermission.MoveMembers)] SocketVoiceChannel channel)
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
		[Summary("Moves all users from one channel to another. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUsers : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task<RuntimeResult> Command([ValidateVoiceChannel(ChannelPermission.MoveMembers)] SocketVoiceChannel input,
				[ValidateVoiceChannel(ChannelPermission.MoveMembers)] SocketVoiceChannel output,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				ProgressLogger = new MultiUserActionProgressLogger(Context.Channel, i => Responses.Users.MultiUserAction(i.AmountLeft).Reason, GenerateRequestOptions());
				var amountChanged = await ProcessAsync(input.Users, bypass,
					u => true,
					u => u.ModifyAsync(x => x.Channel = output, GenerateRequestOptions())).CAF();
				return Responses.Users.MultiUserActionSuccess(amountChanged);
			}
		}

		[Group(nameof(PruneUsers)), ModuleInitialismAlias(typeof(PruneUsers))]
		[Summary("Removes users who have no roles and have not been seen in the given amount of days. " +
			"If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
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
		[Summary("Lists the given reason for the ban.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class GetBanReason : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(IBan ban)
				=> Responses.Users.DisplayBanReason(ban);
		}

		[Group(nameof(DisplayCurrentBanList)), ModuleInitialismAlias(typeof(DisplayCurrentBanList))]
		[Summary("Displays all the bans on the guild.")]
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
		[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
		[UserPermissionRequirement(GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
		public sealed class RemoveMessages : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidatePositiveNumber] int requestCount,
				[Optional] IGuildUser user,
				[Optional, ValidateTextChannel(ChannelPermission.ManageMessages, FromContext = true)] ITextChannel channel)
				=> Command(requestCount, channel, user);
			[Command]
			public async Task<RuntimeResult> Command([ValidatePositiveNumber] int requestCount,
				[Optional, ValidateTextChannel(ChannelPermission.ManageMessages, FromContext = true)] ITextChannel channel,
				[Optional] IGuildUser user)
			{
				channel ??= Context.Channel;

				//If not the context channel then get the first message in that channel
				var thisChannel = Context.Message.Channel.Id == channel.Id;
				var startMsg = thisChannel ? Context.Message : (await channel.GetMessagesAsync(1).FlattenAsync().CAF()).FirstOrDefault();

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
		[Summary("All actions but `TakeNickame` require the output role/nickname. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class ForAllWithRole : MultiUserActionModule
		{
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveRole(IRole target,
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
			public Task<RuntimeResult> TakeRole(IRole target,
				[NotEveryoneOrManaged] IRole take,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, u => u.RemoveRoleAsync(take, GenerateRequestOptions()));
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> GiveNickname([ValidateRole] IRole target,
				[ValidateNickname] string nickname,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = nickname, GenerateRequestOptions())));
			[ImplicitCommand(RunMode = RunMode.Async), ImplicitAlias]
			public Task<RuntimeResult> ClearNickname([ValidateRole] IRole target,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> CommandRunner(target, bypass, CanModify(u => u.ModifyAsync(x => x.Nickname = u.Username, GenerateRequestOptions())));

			private async Task<RuntimeResult> CommandRunner(IRole role, bool bypass, Func<IGuildUser, Task> update)
			{
				static string CreateResult(MultiUserActionProgressArgs i) => Responses.Users.MultiUserAction(i.AmountLeft).Reason;
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
