using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.GuildSettings;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.UserModeration
{
	[Group(nameof(Mute)), TopLevelShortAlias(typeof(Mute))]
	[Summary("Prevents a user from typing and speaking in the guild. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.ManageRoles, GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class Mute : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time, [Optional, Remainder] string reason)
		{
			var muteRole = await RoleUtils.GetMuteRoleAsync(Context, Context.GuildSettings).CAF();
			if (user.RoleIds.Contains(muteRole.Id))
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnrolemuteAsync(user, muteRole, new ModerationReason(Context.User, reason)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.RoleMuteAsync(user, muteRole, new ModerationReason(Context.User, reason)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
		}
	}

	[Group(nameof(VoiceMute)), TopLevelShortAlias(typeof(VoiceMute))]
	[Summary("Prevents a user from speaking. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.MuteMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class VoiceMute : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time)
		{
			if (user.IsMuted)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnvoicemuteAsync(user, new ModerationReason(Context.User, null)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.VoiceMuteAsync(user, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Deafen)), TopLevelShortAlias(typeof(Deafen))]
	[Summary("Prevents a user from hearing. " +
		"Time is in minutes, and if no time is given then the mute will not expire.")]
	[PermissionRequirement(new[] { GuildPermission.DeafenMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Deafen : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [Optional] uint time)
		{
			if (user.IsDeafened)
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UndeafenAsync(user, new ModerationReason(Context.User, null)).CAF();
				await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.DeafenAsync(user, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
		}
	}

	[Group(nameof(MoveUser)), TopLevelShortAlias(typeof(MoveUser))]
	[Summary("Moves the user to the given voice channel.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUser : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IGuildUser user, [VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel channel)
		{
			if (user.VoiceChannel == null)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("User is not in a voice channel.")).CAF();
				return;
			}
			else if (user.VoiceChannel == channel)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("User is already in that channel.")).CAF();
				return;
			}

			await UserUtils.MoveUserAsync(user, channel, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully moved `{user.Format()}` to `{channel.Format()}`.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(MoveUsers)), TopLevelShortAlias(typeof(MoveUsers))]
	[Summary("Moves all users from one channel to another. " +
		"Max is 100 users per use unless the bypass string is said.")]
	[PermissionRequirement(new[] { GuildPermission.MoveMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class MoveUsers : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel inputChannel,
			[VerifyObject(false, ObjectVerification.CanMoveUsers)] IVoiceChannel outputChannel,
			[OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await inputChannel.GetUsersAsync().FlattenAsync().CAF())
				.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).MoveUsersAsync(outputChannel, new ModerationReason(Context.User, null)).CAF();
		}
	}

	[Group(nameof(PruneUsers)), TopLevelShortAlias(typeof(PruneUsers))]
	[Summary("Removes users who have no roles and have not been seen in the given amount of days. " +
		"If the optional argument is not typed exactly, then the bot will only give a number of how many people will be kicked.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class PruneUsers : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyNumber(new[] { 1, 7, 30 })] uint days, [Optional, OverrideTypeReader(typeof(PruneTypeReader))] bool simulate)
		{
			var amt = await GuildUtils.PruneUsersAsync(Context.Guild, (int)days, !simulate, new ModerationReason(Context.User, null)).CAF();
			var resp = $"`{amt}` members{(!simulate ? " would" : "")} have been pruned with a prune period of `{days}` days.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(SoftBan)), TopLevelShortAlias(typeof(SoftBan))]
	[Summary("Bans then unbans a user, which removes all recent messages from them.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class SoftBan : AdvobotModuleBase
	{
		[Command, Priority(1)]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] string reason)
		{
			await CommandRunner(user.Id, reason).CAF();
		}
		[Command, Priority(0)]
		public async Task Command(ulong userId, [Optional, Remainder] string reason)
		{
			await CommandRunner(userId, reason).CAF();
		}

		private async Task CommandRunner(ulong userId, string reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.SoftbanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Ban)), TopLevelShortAlias(typeof(Ban))]
	[Summary("Bans the user from the guild. " +
		"Time specifies how long and is in minutes.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Ban : AdvobotModuleBase
	{
		[Command]
		public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong user, [Optional] uint time,
			[Optional, Remainder] string reason)
		{
			await CommandRunner(user, time, reason).CAF();
		}
		[Command]
		public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong user, [Optional, Remainder] string reason)
		{
			await CommandRunner(user, 0, reason).CAF();
		}

		private async Task CommandRunner(ulong userId, uint time, string reason)
		{
			if ((await Context.Guild.GetBansAsync().CAF()).Select(x => x.User.Id).Contains(userId))
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("That user is already banned.")).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.BanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason), 1).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(Unban)), TopLevelShortAlias(typeof(Unban))]
	[Summary("Unbans the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Unban : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IBan ban, [Optional, Remainder] string reason)
		{
			var remover = new PunishmentRemover(Context.Timers);
			await remover.UnbanAsync(Context.Guild, ban.User.Id, new ModerationReason(Context.User, reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, remover.ToString()).CAF();
		}
	}

	[Group(nameof(GetBanReason)), TopLevelShortAlias(typeof(GetBanReason))]
	[Summary("Lists the given reason for the ban.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers}, null)]
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
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(Kick)), TopLevelShortAlias(typeof(Kick))]
	[Summary("Kicks the user from the guild.")]
	[PermissionRequirement(new[] { GuildPermission.KickMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class Kick : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(false, ObjectVerification.CanBeEdited)] IGuildUser user, [Optional, Remainder] string reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.KickAsync(user, new ModerationReason(Context.User, reason)).CAF();
			await MessageUtils.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
		}
	}

	[Group(nameof(DisplayCurrentBanList)), TopLevelShortAlias(typeof(DisplayCurrentBanList))]
	[Summary("Displays all the bans on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.BanMembers }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayCurrentBanList : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var bans = await Context.Guild.GetBansAsync().CAF();
			if (!bans.Any())
			{
				await MessageUtils.SendMessageAsync(Context.Channel, "This guild has no bans.").CAF();
				return;
			}

			var embed = new EmbedWrapper
			{
				Title = "Current Bans",
				Description = bans.FormatNumberedList("`{0}`", x => x.User.Format()),
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(RemoveMessages)), TopLevelShortAlias(typeof(RemoveMessages))]
	[Summary("Removes the provided number of messages from either the user, the channel, both, or, if neither is input, the current channel.")]
	[PermissionRequirement(new[] { GuildPermission.ManageMessages }, null)]
	[DefaultEnabled(true)]
	public sealed class RemoveMessages : AdvobotModuleBase
	{
		[Command]
		public async Task Command(uint requestCount, [Optional] IGuildUser user, 
			[Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] ITextChannel channel)
		{
			await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel).CAF();
		}
		[Command]
		public async Task Command(uint requestCount, 
			[Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] ITextChannel channel, [Optional] IGuildUser user)
		{
			await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel).CAF();
		}

		private async Task CommandRunner(int requestCount, IGuildUser user, ITextChannel channel)
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

			var reason = new ModerationReason(Context.User, "message deletion");

			//If not the context channel then get the first message in that channel
			var messageToStartAt = Context.Message.Channel.Id == channel.Id
				? Context.Message
				: (await channel.GetMessagesAsync(1).FlattenAsync().CAF()).FirstOrDefault();

			//If there is a non null user then delete messages specifically from that user
			var deletedAmt = await MessageUtils.DeleteMessagesAsync(channel, messageToStartAt, requestCount, reason, user).CAF();

			//If the context channel isn't the targetted channel then delete the start message
			//Increase by one to account for it not being targetted.
			if (Context.Message.Channel.Id != channel.Id)
			{
				await MessageUtils.DeleteMessageAsync(messageToStartAt, reason).CAF();
				deletedAmt++;
			}

			var response = $"Successfully deleted `{deletedAmt}` message{GeneralFormatting.FormatPlural(deletedAmt)}";
			var userResp = user != null ? $" from `{user.Format()}`" : null;
			var chanResp = channel != null ? $" on `{channel.Format()}`" : null;
			var resp = $"{GeneralFormatting.JoinNonNullStrings(" ", response, userResp, chanResp)}.";
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
	public sealed class ModifySlowmode : AdvobotSavingModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create([VerifyNumber(1, 5)] uint messages, [VerifyNumber(1, 30)] uint interval, [Optional] params IRole[] immuneRoles)
		{
			Context.GuildSettings.Slowmode = new Slowmode((int)messages, (int)interval, immuneRoles);
			var resp = $"Successfully setup slowmode.\n{Context.GuildSettings.Slowmode.ToString()}";
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
			var resp = $"Successfully enabled slowmode.\n{Context.GuildSettings.Slowmode.ToString()}";
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
	public sealed class ForAllWithRole : AdvobotModuleBase
	{
		[Command(nameof(GiveRole)), ShortAlias(nameof(GiveRole))]
		public async Task GiveRole(IRole targetRole, 
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole givenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			if (targetRole.Id == givenRole.Id)
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("Cannot give the role being gathered.")).CAF();
				return;
			}

			var users = (await Context.Guild.GetEditableUsersAsync(Context.User).CAF())
				.Where(x => x.RoleIds.Contains(targetRole.Id))
				.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).GiveRolesAsync(givenRole, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(TakeRole)), ShortAlias(nameof(TakeRole))]
		public async Task TakeRole(IRole targetRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole takenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetEditableUsersAsync(Context.User).CAF())
				.Where(x => x.RoleIds.Contains(targetRole.Id))
				.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).TakeRolesAsync(takenRole, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(GiveNickname)), ShortAlias(nameof(GiveNickname))]
		public async Task GiveNickname([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole targetRole,
			[VerifyStringLength(Target.Nickname)] string nickname,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetEditableUsersAsync(Context.User).CAF())
				.Where(x => x.RoleIds.Contains(targetRole.Id))
				.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).ModifyNicknamesAsync(nickname, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(TakeNickname)), ShortAlias(nameof(TakeNickname))]
		public async Task TakeNickname([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole targetRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetEditableUsersAsync(Context.User).CAF())
				.Where(x => x.RoleIds.Contains(targetRole.Id))
				.Take(bypass ? int.MaxValue : Context.BotSettings.MaxUserGatherCount);
			await new MultiUserAction(Context, Context.Timers, users).ModifyNicknamesAsync(null, new ModerationReason(Context.User, null)).CAF();
		}
	}
}
