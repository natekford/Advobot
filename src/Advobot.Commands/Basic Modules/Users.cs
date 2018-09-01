using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Settings;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Users
{
	[Category(typeof(Mute)), Group(nameof(Mute)), TopLevelShortAlias(typeof(Mute))]
	[Summary("Prevents a user from typing and speaking in the guild. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
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
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			IRole muteRole = Context.Guild.GetRole(Context.GuildSettings.MuteRoleId);
			if (!muteRole.Verify(Context, new[] { Verif.CanBeEdited, Verif.IsNotManaged }).IsSuccess)
			{
				muteRole = await Context.Guild.CreateRoleAsync("Advobot_Mute", new GuildPermissions(0)).CAF();
				Context.GuildSettings.MuteRoleId = muteRole.Id;
				Context.GuildSettings.SaveSettings(Context.BotSettings);
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
				var remover = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
				await remover.UnrolemuteAsync(user, muteRole, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new Punisher(reason.Time, Context.Provider);
			await giver.RoleMuteAsync(user, muteRole, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(VoiceMute)), Group(nameof(VoiceMute)), TopLevelShortAlias(typeof(VoiceMute))]
	[Summary("Prevents a user from speaking. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			if (user.IsMuted)
			{
				var remover = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
				await remover.UnvoicemuteAsync(user, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new Punisher(reason.Time, Context.Provider);
			await giver.VoiceMuteAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(Deafen)), Group(nameof(Deafen)), TopLevelShortAlias(typeof(Deafen))]
	[Summary("Prevents a user from hearing. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			if (user.IsDeafened)
			{
				var remover = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
				await remover.UndeafenAsync(user, GetRequestOptions(reason.Reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new Punisher(reason.Time, Context.Provider);
			await giver.DeafenAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(MoveUser)), Group(nameof(MoveUser)), TopLevelShortAlias(typeof(MoveUser))]
	[Summary("Moves the user to the given voice channel.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task Command(SocketGuildUser user, [ValidateObject(false, Verif.CanMoveUsers)] SocketVoiceChannel channel)
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

	[Category(typeof(MoveUsers)), Group(nameof(MoveUsers)), TopLevelShortAlias(typeof(MoveUsers))]
	[Summary("Moves all users from one channel to another. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUsers : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command(
			[ValidateObject(false, Verif.CanMoveUsers)] SocketVoiceChannel inputChannel,
			[ValidateObject(false, Verif.CanMoveUsers)] SocketVoiceChannel outputChannel,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = inputChannel.Users.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, users).MoveUsersAsync(outputChannel, GetRequestOptions()).CAF();
		}
	}

	[Category(typeof(PruneUsers)), Group(nameof(PruneUsers)), TopLevelShortAlias(typeof(PruneUsers))]
	[Summary("Removes users who have no roles and have not been seen in the given amount of days. " +
		"If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class PruneUsers : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateNumber(new[] { 1, 7, 30 })] uint days, [Optional, OverrideTypeReader(typeof(PruneTypeReader))] bool actual)
		{
			//Actual TRUE = PRUNE, FALSE = SIMULATION
			var amt = await Context.Guild.PruneUsersAsync((int)days, !actual, GetRequestOptions()).CAF();
			var resp = $"`{amt}` members{(!actual ? " would" : "")} have been pruned with a prune period of `{days}` days.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Category(typeof(SoftBan)), Group(nameof(SoftBan)), TopLevelShortAlias(typeof(SoftBan))]
	[Summary("Bans then unbans a user, which removes all recent messages from them.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command(
			[ValidateObject(false, Verif.CanBeEdited)] IGuildUser user,
			[Optional, Remainder] ModerationReason reason)
			=> await CommandRunner(user.Id, reason).CAF();
		[Command]
		public async Task Command(ulong userId, [Optional, Remainder] ModerationReason reason)
			=> await CommandRunner(userId, reason).CAF();

		private async Task CommandRunner(ulong userId, ModerationReason reason)
		{
			var giver = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
			await giver.SoftbanAsync(Context.Guild, userId, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(Ban)), Group(nameof(Ban)), TopLevelShortAlias(typeof(Ban))]
	[Summary("Bans the user from the guild. " +
		"Time specifies how long and is in minutes.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Ban : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command(
			[ValidateObject(false, Verif.CanBeEdited)] IGuildUser user,
			[Optional, Remainder] ModerationReason reason)
			=> await CommandRunner(user.Id, reason).CAF();
		[Command]
		public async Task Command(ulong user, [Optional, Remainder] ModerationReason reason)
			=> await CommandRunner(user, reason).CAF();

		private async Task CommandRunner(ulong userId, ModerationReason reason)
		{
			if ((await Context.Guild.GetBansAsync().CAF()).Select(x => x.User.Id).Contains(userId))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("That user is already banned.")).CAF();
				return;
			}

			var giver = new Punisher(reason.Time, Context.Provider);
			await giver.BanAsync(Context.Guild, userId, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(Unban)), Group(nameof(Unban)), TopLevelShortAlias(typeof(Unban))]
	[Summary("Unbans the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IBan ban, [Optional, Remainder] ModerationReason reason)
		{
			var remover = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
			await remover.UnbanAsync(Context.Guild, ban.User.Id, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, remover.ToString()).CAF();
		}
	}

	[Category(typeof(GetBanReason)), Group(nameof(GetBanReason)), TopLevelShortAlias(typeof(GetBanReason))]
	[Summary("Lists the given reason for the ban.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class GetBanReason : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IBan ban)
		{
			var embed = new EmbedWrapper
			{
				Title = $"Ban reason for {ban.User.Format()}",
				Description = ban.Reason,
			};
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
	}

	[Category(typeof(Kick)), Group(nameof(Kick)), TopLevelShortAlias(typeof(Kick))]
	[Summary("Kicks the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task Command([ValidateObject(false, Verif.CanBeEdited)] SocketGuildUser user, [Optional, Remainder] ModerationReason reason)
		{
			var giver = new Punisher(TimeSpan.FromMinutes(0), default(ITimerService));
			await giver.KickAsync(user, GetRequestOptions(reason.Reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Category(typeof(DisplayCurrentBanList)), Group(nameof(DisplayCurrentBanList)), TopLevelShortAlias(typeof(DisplayCurrentBanList))]
	[Summary("Displays all the bans on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayCurrentBanList : AdvobotModuleBase
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
			await MessageUtils.SendMessageAsync(Context.Channel, null, embed).CAF();
		}
	}

	[Category(typeof(RemoveMessages)), Group(nameof(RemoveMessages)), TopLevelShortAlias(typeof(RemoveMessages))]
	[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveMessages : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			uint requestCount,
			[Optional] IGuildUser user,
			[Optional, ValidateObject(true, Verif.CanDeleteMessages)] SocketTextChannel channel)
			=> await CommandRunner((int)requestCount, user, channel ?? (SocketTextChannel)Context.Channel).CAF();
		[Command]
		public async Task Command(
			uint requestCount,
			[Optional, ValidateObject(true, Verif.CanDeleteMessages)] SocketTextChannel channel,
			[Optional] IGuildUser user)
			=> await CommandRunner((int)requestCount, user, channel ?? (SocketTextChannel)Context.Channel).CAF();

		private async Task CommandRunner(int requestCount, IUser user, SocketTextChannel channel)
		{
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

	[Category(typeof(ModifySlowmode)), Group(nameof(ModifySlowmode)), TopLevelShortAlias(typeof(ModifySlowmode))]
	[Summary("First arg is how many messages can be sent in a timeframe. " +
		"Second arg is the timeframe. " +
		"Third are the list of roles that are immune to slowmode.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	[SaveGuildSettings]
	public sealed class ModifySlowmode : AdvobotModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create([ValidateNumber(1, 5)] uint messages, [ValidateNumber(1, 30)] uint interval, [Optional] params SocketRole[] immuneRoles)
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

	[Category(typeof(ForAllWithRole)), Group(nameof(ForAllWithRole)), TopLevelShortAlias(typeof(ForAllWithRole))]
	[Summary("All actions but `TakeNickame` require the output role/nickname. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ForAllWithRole : AdvobotModuleBase
	{
		[Command(nameof(GiveRole)), ShortAlias(nameof(GiveRole))]
		public async Task GiveRole(
			SocketRole targetRole,
			[ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] SocketRole givenRole,
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
			[ValidateObject(false, Verif.CanBeEdited, Verif.IsNotEveryone, Verif.IsNotManaged)] SocketRole takenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await CommandRunner(targetRole, bypass, async (m) => await m.TakeRolesAsync(takenRole, GetRequestOptions()).CAF());
		[Command(nameof(GiveNickname)), ShortAlias(nameof(GiveNickname))]
		public async Task GiveNickname(
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole targetRole,
			[ValidateString(Target.Nickname)] string nickname,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await CommandRunner(targetRole, bypass, async (m) => await m.ModifyNicknamesAsync(nickname, GetRequestOptions()).CAF());
		[Command(nameof(TakeNickname)), ShortAlias(nameof(TakeNickname))]
		public async Task TakeNickname(
			[ValidateObject(false, Verif.CanBeEdited)] SocketRole targetRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
			=> await CommandRunner(targetRole, bypass, async (m) => await m.ModifyNicknamesAsync(null, GetRequestOptions()).CAF());

		private async Task CommandRunner(SocketRole target, bool bypass, Func<MultiUserAction, Task> callback)
		{
			var users = Context.Guild.GetEditableUsers(Context.User as SocketGuildUser)
				   .Where(x => x.Roles.Select(r => r.Id).Contains(target.Id))
				   .Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await callback(new MultiUserAction(Context, users)).CAF();
		}
	}
}
