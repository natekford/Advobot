using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.Settings;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Commands.Users
{
	[Group(nameof(Mute)), TopLevelShortAlias(typeof(Mute))]
	[Summary("Prevents a user from typing and speaking in the guild. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class Mute : NonSavingModuleBase
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
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			IRole muteRole = Context.Guild.GetRole(Context.GuildSettings.MuteRoleId);
			if (!muteRole.Verify(Context, new[] { ObjectVerification.CanBeEdited, ObjectVerification.IsNotManaged }).IsSuccess)
			{
				muteRole = await Context.Guild.CreateRoleAsync("Advobot_Mute", new GuildPermissions(0)).CAF();
				Context.GuildSettings.MuteRoleId = muteRole.Id;
				Context.GuildSettings.SaveSettings();
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

			if (user.Roles.Select(x => x.Id).Contains(muteRole.Id))
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnrolemuteAsync(user, muteRole, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver(reason.Time, Context.Timers);
			await giver.RoleMuteAsync(user, muteRole, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(VoiceMute)), TopLevelShortAlias(typeof(VoiceMute))]
	[Summary("Prevents a user from speaking. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class VoiceMute : NonSavingModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			if (user.IsMuted)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnvoicemuteAsync(user, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver(reason.Time, Context.Timers);
			await giver.VoiceMuteAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Deafen)), TopLevelShortAlias(typeof(Deafen))]
	[Summary("Prevents a user from hearing. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Deafen : NonSavingModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			if (user.IsDeafened)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UndeafenAsync(user, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver(reason.Time, Context.Timers);
			await giver.DeafenAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(MoveUser)), TopLevelShortAlias(typeof(MoveUser))]
	[Summary("Moves the user to the given voice channel.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUser : NonSavingModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [VerifyObject(false, ObjectVerification.CanMoveUsers)] SocketVoiceChannel channel)
		{
			if (user.VoiceChannel == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("User is not in a voice channel.")).CAF();
				return;
			}
			if (user.VoiceChannel?.Id == channel.Id)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("User is already in that channel.")).CAF();
				return;
			}

			await user.ModifyAsync(x => x.Channel = Optional.Create((IVoiceChannel)channel), GetRequestOptions()).CAF();
			var resp = $"Successfully moved `{user.Format()}` to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(MoveUsers)), TopLevelShortAlias(typeof(MoveUsers))]
	[Summary("Moves all users from one channel to another. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUsers : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[VerifyObject(false, ObjectVerification.CanMoveUsers)] SocketVoiceChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanMoveUsers)] SocketVoiceChannel outputChannel,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = inputChannel.Users.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).MoveUsersAsync(outputChannel, GetRequestOptions()).CAF();
		}
	}

	[Group(nameof(PruneUsers)), TopLevelShortAlias(typeof(PruneUsers))]
	[Summary("Removes users who have no roles and have not been seen in the given amount of days. " +
		"If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class PruneUsers : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyNumber(new[] { 1, 7, 30 })] uint days, [Optional, OverrideTypeReader(typeof(PruneTypeReader))] bool actual)
		{
			//Actual TRUE = PRUNE, FALSE = SIMULATION
			var amt = await Context.Guild.PruneUsersAsync((int)days, !actual, GetRequestOptions()).CAF();
			var resp = $"`{amt}` members{(!actual ? " would" : "")} have been pruned with a prune period of `{days}` days.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(SoftBan)), TopLevelShortAlias(typeof(SoftBan))]
	[Summary("Bans then unbans a user, which removes all recent messages from them.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftBan : NonSavingModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			await CommandRunner(user.Id, reason).CAF();
		}
		[Command]
		public async Task Command(ulong userId, [Optional, Remainder] ModerationReason reason)
		{
			await CommandRunner(userId, reason).CAF();
		}

		private async Task CommandRunner(ulong userId, ModerationReason reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.SoftbanAsync(Context.Guild, userId, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Ban)), TopLevelShortAlias(typeof(Ban))]
	[Summary("Bans the user from the guild. " +
		"Time specifies how long and is in minutes.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Ban : NonSavingModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			await CommandRunner(user.Id, reason).CAF();
		}
		[Command]
		public async Task Command(ulong user, [Optional, Remainder] ModerationReason reason)
		{
			await CommandRunner(user, reason).CAF();
		}

		private async Task CommandRunner(ulong userId, ModerationReason reason)
		{
			if ((await Context.Guild.GetBansAsync().CAF()).Select(x => x.User.Id).Contains(userId))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("That user is already banned.")).CAF();
				return;
			}

			var giver = new PunishmentGiver(reason.Time, Context.Timers);
			await giver.BanAsync(Context.Guild, userId, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Unban)), TopLevelShortAlias(typeof(Unban))]
	[Summary("Unbans the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Unban : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IBan ban, [Optional, Remainder] ModerationReason reason)
		{
			var remover = new PunishmentRemover(Context.Timers);
			await remover.UnbanAsync(Context.Guild, ban.User.Id, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, remover.ToString()).CAF();
		}
	}

	[Group(nameof(GetBanReason)), TopLevelShortAlias(typeof(GetBanReason))]
	[Summary("Lists the given reason for the ban.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class GetBanReason : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IBan ban)
		{
			var embed = new EmbedWrapper
			{
				Title = $"Ban reason for {ban.User.Format()}",
				Description = ban.Reason
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(Kick)), TopLevelShortAlias(typeof(Kick))]
	[Summary("Kicks the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Kick : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.KickAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(DisplayCurrentBanList)), TopLevelShortAlias(typeof(DisplayCurrentBanList))]
	[Summary("Displays all the bans on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayCurrentBanList : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			var bans = await Context.Guild.GetBansAsync().CAF();
			var embed = new EmbedWrapper
			{
				Title = "Current Bans",
				Description = !bans.Any()
					? "This guild has no bans."
					: bans.FormatNumberedList(x => x.User.Format())
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(RemoveMessages)), TopLevelShortAlias(typeof(RemoveMessages))]
	[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveMessages : NonSavingModuleBase
	{
		[Command]
		public async Task Command(
			uint requestCount, 
			[Optional] IGuildUser user,
			[Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] SocketTextChannel channel)
		{
			await CommandRunner((int)requestCount, user, channel ?? (SocketTextChannel)Context.Channel).CAF();
		}
		[Command]
		public async Task Command(
			uint requestCount,
			[Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] SocketTextChannel channel, 
			[Optional] IGuildUser user)
		{
			await CommandRunner((int)requestCount, user, channel ?? (SocketTextChannel)Context.Channel).CAF();
		}

		private async Task CommandRunner(int requestCount, IUser user, SocketTextChannel channel)
		{
			/* I don't know if anyone actually cares about this. 
			 * If someone ever does I guess I can uncomment it or make it a setting.
			var serverLog = Context.GuildSettings.ServerLog?.Id == channel.Id;
			var modLog = Context.GuildSettings.ModLog?.Id == channel.Id;
			var imageLog = Context.GuildSettings.ImageLog?.Id == channel.Id;
			if (Context.User.Id != Context.Guild.OwnerId && (serverLog || modLog || imageLog))
			{
				var owner = await Context.Guild.GetOwnerAsync().CAF();
				var m = $"`{Context.User.FormatUser()}` is trying to delete messages from a log channel: `{channel.FormatChannel()}`.";
				await owner.SendMessageAsync(m).CAF();
				return;
			}*/

			//If not the context channel then get the first message in that channel
			var messageToStartAt = Context.Message.Channel.Id == channel.Id
				? Context.Message
				: (await channel.GetMessagesAsync(1).FlattenAsync().CAF()).FirstOrDefault();

			//If there is a non null user then delete messages specifically from that user
			var deletedAmt = await MessageUtils.DeleteMessagesAsync(channel, messageToStartAt, requestCount, GetRequestOptions(), user).CAF();

			//If the context channel isn't the targetted channel then delete the start message
			//Increase by one to account for it not being targetted.
			if (Context.Message.Channel.Id != channel.Id)
			{
				await MessageUtils.DeleteMessageAsync(messageToStartAt, GetRequestOptions()).CAF();
				deletedAmt++;
			}

			var response = $"Successfully deleted `{deletedAmt}` message(s)";
			var userResp = user != null ? $" from `{user.Format()}`" : null;
			var chanResp = $" on `{channel.Format()}`";
			var resp = $"{new[] { response, userResp, chanResp }.JoinNonNullStrings(" ")}.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifySlowmode)), TopLevelShortAlias(typeof(ModifySlowmode))]
	[Summary("First arg is how many messages can be sent in a timeframe. " +
		"Second arg is the timeframe. " +
		"Third arg is guildwide; true means yes, false means no. " +
		"Fourth are the list of roles that are immune to slowmode.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ModifySlowmode : GuildSettingsSavingModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create([VerifyNumber(1, 5)] uint messages, [VerifyNumber(1, 30)] uint interval, [Optional] params SocketRole[] immuneRoles)
		{
			Context.GuildSettings.Slowmode = new Slowmode((int)messages, (int)interval, immuneRoles);
			var resp = $"Successfully setup slowmode.\n{Context.GuildSettings.Slowmode}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				var error = new Error("There must be a slowmode set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			Context.GuildSettings.Slowmode.Enabled = true;
			var resp = $"Successfully enabled slowmode.\n{Context.GuildSettings.Slowmode}";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				var error = new Error("There must be a slowmode set up before one can be enabled or disabled.");
				await MessageUtils.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			Context.GuildSettings.Slowmode.Enabled = false;
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled slowmode.").CAF();
		}
	}

	[Group(nameof(ForAllWithRole)), TopLevelShortAlias(typeof(ForAllWithRole))]
	[Summary("All actions but `TakeNickame` require the output role/nickname. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ForAllWithRole : NonSavingModuleBase
	{
		[Command(nameof(GiveRole)), ShortAlias(nameof(GiveRole))]
		public async Task GiveRole(
			SocketRole targetRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsNotEveryone, ObjectVerification.IsNotManaged)] SocketRole givenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			if (targetRole.Id == givenRole.Id)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Cannot give the role being gathered.")).CAF();
				return;
			}

			await CommandRunner(targetRole, bypass, async (m) => await m.GiveRolesAsync(givenRole, GetRequestOptions()).CAF());
		}
		[Command(nameof(TakeRole)), ShortAlias(nameof(TakeRole))]
		public async Task TakeRole(
			SocketRole targetRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsNotEveryone, ObjectVerification.IsNotManaged)] SocketRole takenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			await CommandRunner(targetRole, bypass, async (m) => await m.TakeRolesAsync(takenRole, GetRequestOptions()).CAF());
		}
		[Command(nameof(GiveNickname)), ShortAlias(nameof(GiveNickname))]
		public async Task GiveNickname(
			[VerifyObject(false, ObjectVerification.CanBeEdited)] SocketRole targetRole,
			[VerifyStringLength(Target.Nickname)] string nickname,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			await CommandRunner(targetRole, bypass, async (m) => await m.ModifyNicknamesAsync(nickname, GetRequestOptions()).CAF());
		}
		[Command(nameof(TakeNickname)), ShortAlias(nameof(TakeNickname))]
		public async Task TakeNickname(
			[VerifyObject(false, ObjectVerification.CanBeEdited)] SocketRole targetRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			await CommandRunner(targetRole, bypass, async (m) => await m.ModifyNicknamesAsync(null, GetRequestOptions()).CAF());
		}

		private async Task CommandRunner(SocketRole target, bool bypass, Func<MultiUserAction, Task> callback)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser)
				   .Where(x => x.Roles.Select(r => r.Id).Contains(target.Id))
				   .Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await callback(new MultiUserAction(Context, Context.Timers, users)).CAF();
		}
	}
}
