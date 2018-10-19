using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	[Group]
	public sealed class Users : ModuleBase
	{
		[Group(nameof(Mute)), TopLevelShortAlias(typeof(Mute))]
		[Summary("Prevents a user from typing and speaking in the guild. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.ManageRoles, GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
		public sealed class Mute : AdvobotModuleBase
		{
			private const CPerm MUTE_ROLE_TEXT_PERMS = 0
				| CPerm.CreateInstantInvite
				| CPerm.ManageChannels
				| CPerm.ManageRoles
				| CPerm.ManageWebhooks
				| CPerm.SendMessages
				| CPerm.ManageMessages
				| CPerm.AddReactions;
			private const CPerm MUTE_ROLE_VOICE_PERMS = 0
				| CPerm.CreateInstantInvite
				| CPerm.ManageChannels
				| CPerm.ManageRoles
				| CPerm.ManageWebhooks
				| CPerm.Speak
				| CPerm.MuteMembers
				| CPerm.DeafenMembers
				| CPerm.MoveMembers;

			[Command]
			public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				var muteRole = await GetOrCreateMuteRoleAsync().CAF();
				if (user.Roles.Select(x => x.Id).Contains(muteRole.Id))
				{
					var remover = new Punisher(TimeSpan.FromMinutes(0), default);
					await remover.UnrolemuteAsync(user, muteRole, GenerateRequestOptions(reason.Reason)).CAF();
					await ReplyTimedAsync(remover.ToString()).CAF();
					return;
				}

				var giver = new Punisher(reason.Time, Timers);
				await giver.RoleMuteAsync(user, muteRole, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}

			private async Task<IRole> GetOrCreateMuteRoleAsync()
			{
				var existingMuteRole = Context.Guild.GetRole(Context.GuildSettings.MuteRoleId);
				IRole muteRole = existingMuteRole;
				if (!Context.GetGuildUser().ValidateRole(existingMuteRole, ValidationUtils.RoleIsNotEveryone, ValidationUtils.RoleIsNotManaged).IsSuccess)
				{
					muteRole = await Context.Guild.CreateRoleAsync("Advobot_Mute", new GuildPermissions(0)).CAF();
					Context.GuildSettings.MuteRoleId = muteRole.Id;
					Context.GuildSettings.SaveSettings(BotSettings);
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

		[Group(nameof(VoiceMute)), TopLevelShortAlias(typeof(VoiceMute))]
		[Summary("Prevents a user from speaking. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.MuteMembers)]
		[EnabledByDefault(true)]
		public sealed class VoiceMute : AdvobotModuleBase
		{
			[Command]
			public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				if (user.IsMuted)
				{
					var remover = new Punisher(TimeSpan.FromMinutes(0), default);
					await remover.UnvoicemuteAsync(user, GenerateRequestOptions(reason.Reason)).CAF();
					await ReplyTimedAsync(remover.ToString()).CAF();
					return;
				}

				var giver = new Punisher(reason.Time, Timers);
				await giver.VoiceMuteAsync(user, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}
		}

		[Group(nameof(Deafen)), TopLevelShortAlias(typeof(Deafen))]
		[Summary("Prevents a user from hearing. " +
			"Time is in minutes, and if no time is given then the mute will not expire.")]
		[UserPermissionRequirement(GuildPermission.DeafenMembers)]
		[EnabledByDefault(true)]
		public sealed class Deafen : AdvobotModuleBase
		{
			[Command]
			public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				if (user.IsDeafened)
				{
					var remover = new Punisher(TimeSpan.FromMinutes(0), default);
					await remover.UndeafenAsync(user, GenerateRequestOptions(reason.Reason)).CAF();
					await ReplyTimedAsync(remover.ToString()).CAF();
					return;
				}

				var giver = new Punisher(reason.Time, Timers);
				await giver.DeafenAsync(user, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}
		}

		[Group(nameof(MoveUser)), TopLevelShortAlias(typeof(MoveUser))]
		[Summary("Moves the user to the given voice channel.")]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUser : AdvobotModuleBase
		{
			[Command]
			public async Task Command([CanBeMoved] SocketGuildUser user, [ValidateVoiceChannel(CPerm.MoveMembers)] SocketVoiceChannel channel)
			{
				if (user.VoiceChannel?.Id == channel.Id)
				{
					await ReplyErrorAsync(new Error("User is already in that channel.")).CAF();
					return;
				}

				await user.ModifyAsync(x => x.Channel = Optional.Create((IVoiceChannel)channel), GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully moved `{user.Format()}` to `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(MoveUsers)), TopLevelShortAlias(typeof(MoveUsers))]
		[Summary("Moves all users from one channel to another. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.MoveMembers)]
		[EnabledByDefault(true)]
		public sealed class MoveUsers : MultiUserActionModule
		{
			[Command(RunMode = RunMode.Async)]
			public async Task Command(
				[ValidateVoiceChannel(CPerm.MoveMembers)] SocketVoiceChannel input,
				[ValidateVoiceChannel(CPerm.MoveMembers)] SocketVoiceChannel output,
				[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> await Process(input.Users, bypass, x => true, (u, o) => u.ModifyAsync(x => x.Channel = output, o)).CAF();
		}

		[Group(nameof(PruneUsers)), TopLevelShortAlias(typeof(PruneUsers))]
		[Summary("Removes users who have no roles and have not been seen in the given amount of days. " +
			"If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class PruneUsers : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidatePruneDays] int days, [Optional, OverrideTypeReader(typeof(PruneTypeReader))] bool actual)
			{
				//Actual TRUE = PRUNE, FALSE = SIMULATION
				var amt = await Context.Guild.PruneUsersAsync(days, !actual, GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"`{amt}` members{(actual ? "" : " would")} have been pruned with a prune period of `{days}` days.").CAF();
			}
		}

		[Group(nameof(SoftBan)), TopLevelShortAlias(typeof(SoftBan))]
		[Summary("Bans then unbans a user, which removes all recent messages from them.")]
		[UserPermissionRequirement(GuildPermission.BanMembers, GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class SoftBan : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
				=> await Command(user.Id, reason).CAF();
			[Command]
			public async Task Command(ulong userId, [Optional, Remainder] ModerationReason reason)
			{
				var giver = new Punisher(TimeSpan.FromMinutes(0), default);
				await giver.SoftbanAsync(Context.Guild, userId, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}
		}

		[Group(nameof(Ban)), TopLevelShortAlias(typeof(Ban))]
		[Summary("Bans the user from the guild. " +
			"Time specifies how long and is in minutes.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Ban : AdvobotModuleBase
		{
			[Command, Priority(1)]
			public async Task Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
				=> await Command(user.Id, reason).CAF();
			[Command]
			public async Task Command(ulong userId, [Optional, Remainder] ModerationReason reason)
			{
				if ((await Context.Guild.GetBansAsync().CAF()).Select(x => x.User.Id).Contains(userId))
				{
					await ReplyErrorAsync(new Error("That user is already banned.")).CAF();
					return;
				}

				var giver = new Punisher(reason.Time, Timers);
				await giver.BanAsync(Context.Guild, userId, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}
		}

		[Group(nameof(Unban)), TopLevelShortAlias(typeof(Unban))]
		[Summary("Unbans the user from the guild.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class Unban : AdvobotModuleBase
		{
			[Command]
			public async Task Command(IBan ban, [Optional, Remainder] ModerationReason reason)
			{
				var remover = new Punisher(TimeSpan.FromMinutes(0), default);
				await remover.UnbanAsync(Context.Guild, ban.User.Id, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(remover.ToString()).CAF();
			}
		}

		[Group(nameof(GetBanReason)), TopLevelShortAlias(typeof(GetBanReason))]
		[Summary("Lists the given reason for the ban.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class GetBanReason : AdvobotModuleBase
		{
			[Command]
			public async Task Command(IBan ban)
			{
				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = $"Ban reason for {ban.User.Format()}",
					Description = ban.Reason ?? "No reason listed.",
				}).CAF();
			}
		}

		[Group(nameof(Kick)), TopLevelShortAlias(typeof(Kick))]
		[Summary("Kicks the user from the guild.")]
		[UserPermissionRequirement(GuildPermission.KickMembers)]
		[EnabledByDefault(true)]
		public sealed class Kick : AdvobotModuleBase
		{
			[Command]
			public async Task Command([ValidateUser] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
			{
				var giver = new Punisher(TimeSpan.FromMinutes(0), default);
				await giver.KickAsync(user, GenerateRequestOptions(reason.Reason)).CAF();
				await ReplyTimedAsync(giver.ToString()).CAF();
			}
		}

		[Group(nameof(DisplayCurrentBanList)), TopLevelShortAlias(typeof(DisplayCurrentBanList))]
		[Summary("Displays all the bans on the guild.")]
		[UserPermissionRequirement(GuildPermission.BanMembers)]
		[EnabledByDefault(true)]
		public sealed class DisplayCurrentBanList : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var bans = await Context.Guild.GetBansAsync().CAF();
				await ReplyEmbedAsync(new EmbedWrapper
				{
					Title = "Current Bans",
					Description = bans.Any() ? bans.FormatNumberedList(x => x.User.Format()) : "This guild has no bans.",
				}).CAF();
			}
		}

		[Group(nameof(RemoveMessages)), TopLevelShortAlias(typeof(RemoveMessages))]
		[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
		[UserPermissionRequirement(GuildPermission.ManageMessages)]
		[EnabledByDefault(true)]
		public sealed class RemoveMessages : AdvobotModuleBase
		{
			[Command]
			public async Task Command(
				[ValidatePositiveNumber] int requestCount,
				[Optional] IGuildUser user,
				[Optional, ValidateTextChannel(CPerm.ManageMessages, FromContext = true)] SocketTextChannel channel)
				=> await CommandRunner(requestCount, user, channel ?? Context.Channel).CAF();
			[Command]
			public async Task Command(
				[ValidatePositiveNumber] int requestCount,
				[Optional, ValidateTextChannel(CPerm.ManageMessages, FromContext = true)] SocketTextChannel channel,
				[Optional] IGuildUser user)
				=> await CommandRunner(requestCount, user, channel ?? Context.Channel).CAF();

			private async Task CommandRunner(int requestCount, IUser user, SocketTextChannel channel)
			{
				//If not the context channel then get the first message in that channel
				var messageToStartAt = Context.Message.Channel.Id == channel.Id
					? Context.Message
					: (await channel.GetMessagesAsync(1).FlattenAsync().CAF()).FirstOrDefault();

				//If there is a non null user then delete messages specifically from that user
				var deletedAmt = await MessageUtils.DeleteMessagesAsync(channel, messageToStartAt, requestCount, GenerateRequestOptions(), user).CAF();

				//If the context channel isn't the targetted channel then delete the start message
				//Increase by one to account for it not being targetted.
				if (Context.Message.Channel.Id != channel.Id)
				{
					await messageToStartAt.DeleteAsync(GenerateRequestOptions()).CAF();
					deletedAmt++;
				}

				var userStr = user != null ? $" from `{user.Format()}`" : "";
				await ReplyTimedAsync($"Successfully deleted `{deletedAmt}` message(s){userStr} on `{channel.Format()}`.").CAF();
			}
		}

		[Group(nameof(ForAllWithRole)), TopLevelShortAlias(typeof(ForAllWithRole))]
		[Summary("All actions but `TakeNickame` require the output role/nickname. " +
			"Max is 100 users per use unless the bypass string is said.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(true)]
		public sealed class ForAllWithRole : MultiUserActionModule
		{
			[Command(nameof(GiveRole)), ShortAlias(nameof(GiveRole))]
			public async Task GiveRole(
				SocketRole target,
				[NotEveryoneOrManaged] SocketRole give,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			{
				if (target.Id == give.Id)
				{
					await ReplyErrorAsync(new Error("Cannot give the role being gathered.")).CAF();
					return;
				}
				await Process(bypass, x => x.Roles.Select(r => r.Id).Contains(target.Id), (u, o) => u.AddRoleAsync(give, o)).CAF();
			}
			[Command(nameof(TakeRole)), ShortAlias(nameof(TakeRole))]
			public async Task TakeRole(
				SocketRole target,
				[NotEveryoneOrManaged] SocketRole take,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> await Process(bypass, x => x.Roles.Select(r => r.Id).Contains(target.Id), (u, o) => u.RemoveRoleAsync(take, o)).CAF();
			[Command(nameof(GiveNickname)), ShortAlias(nameof(GiveNickname))]
			public async Task GiveNickname(
				[ValidateRole] SocketRole target,
				[ValidateNickname] string nickname,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> await Process(bypass, x => x.Roles.Select(r => r.Id).Contains(target.Id), (u, o) => u.ModifyAsync(x => x.Nickname = nickname)).CAF();
			[Command(nameof(ClearNickname)), ShortAlias(nameof(ClearNickname))]
			public async Task ClearNickname(
				[ValidateRole] SocketRole target,
				[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
				=> await Process(bypass, x => x.Roles.Select(r => r.Id).Contains(target.Id), (u, o) => u.ModifyAsync(x => x.Nickname = u.Username)).CAF();
		}
	}
}
