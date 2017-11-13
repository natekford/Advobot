using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.Punishments;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Enums;
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
			var muteRole = await RoleActions.GetMuteRoleAsync(Context, Context.GuildSettings).CAF();
			if (user.RoleIds.Contains(muteRole.Id))
			{
				var remover = new PunishmentRemover(Context.Timers);
				await remover.UnrolemuteAsync(user, muteRole, new ModerationReason(Context.User, reason)).CAF();
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.RoleMuteAsync(user, muteRole, new ModerationReason(Context.User, reason)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
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
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.VoiceMuteAsync(user, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
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
				await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, remover.ToString()).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.DeafenAsync(user, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, giver.ToString()).CAF();
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
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("User is not in a voice channel.")).CAF();
				return;
			}
			else if (user.VoiceChannel == channel)
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("User is already in that channel.")).CAF();
				return;
			}

			await UserActions.MoveUserAsync(user, channel, new ModerationReason(Context.User, null)).CAF();
			var resp = $"Successfully moved `{user.FormatUser()}` to `{channel.FormatChannel()}`.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			var users = await inputChannel.GetUsersAsync().Flatten().CAF();
			await new MultiUserAction(Context, users, bypass).MoveManyUsersAsync(outputChannel, new ModerationReason(Context.User, null)).CAF();
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
			var amt = await GuildActions.PruneUsersAsync(Context.Guild, (int)days, !simulate, new ModerationReason(Context.User, null)).CAF();
			var resp = $"`{amt}` members{(!simulate ? " would" : "")} have been pruned with a prune period of `{days}` days.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
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
			=> await CommandRunner(user.Id, reason).CAF();
		[Command, Priority(0)]
		public async Task Command(ulong userId, [Optional, Remainder] string reason)
			=> await CommandRunner(userId, reason).CAF();

		private async Task CommandRunner(ulong userId, string reason)
		{
			var giver = new PunishmentGiver(0, Context.Timers);
			await giver.SoftbanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason)).CAF();
			await MessageActions.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
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
			=> await CommandRunner(user, time, reason).CAF();
		[Command]
		public async Task Command([OverrideTypeReader(typeof(UserIdTypeReader))] ulong user, [Optional, Remainder] string reason)
			=> await CommandRunner(user, 0, reason).CAF();

		private async Task CommandRunner(ulong userId, uint time, string reason)
		{
			if ((await Context.Guild.GetBansAsync().CAF()).Select(x => x.User.Id).Contains(userId))
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("That user is already banned.")).CAF();
				return;
			}

			var giver = new PunishmentGiver((int)time, Context.Timers);
			await giver.BanAsync(Context.Guild, userId, new ModerationReason(Context.User, reason), 1).CAF();
			await MessageActions.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
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
			await MessageActions.SendMessageAsync(Context.Channel, remover.ToString()).CAF();
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
			var embed = new AdvobotEmbed($"Ban reason for {ban.User.FormatUser()}", ban.Reason);
			await MessageActions.SendEmbedMessageAsync(Context.Channel, embed).CAF();
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
			await MessageActions.SendMessageAsync(Context.Channel, giver.ToString()).CAF();
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
				await MessageActions.SendMessageAsync(Context.Channel, "This guild has no bans.").CAF();
				return;
			}

			var desc = bans.FormatNumberedList("`{0}`", x => x.User.FormatUser());
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Current Bans", desc)).CAF();
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
			=> await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel).CAF();
		[Command]
		public async Task Command(uint requestCount,
			[Optional, VerifyObject(true, ObjectVerification.CanDeleteMessages)] ITextChannel channel, [Optional] IGuildUser user)
			=> await CommandRunner((int)requestCount, user, channel ?? Context.Channel as ITextChannel).CAF();

		private async Task CommandRunner(int requestCount, IGuildUser user, ITextChannel channel)
		{
			var serverLog = Context.GuildSettings.ServerLog?.Id == channel.Id;
			var modLog = Context.GuildSettings.ModLog?.Id == channel.Id;
			var imageLog = Context.GuildSettings.ImageLog?.Id == channel.Id;
			if (Context.User.Id != Context.Guild.OwnerId && (serverLog || modLog || imageLog))
			{
				var owner = await Context.Guild.GetOwnerAsync().CAF();
				var m = $"`{Context.User.FormatUser()}` is trying to delete messages from a log channel: `{channel.FormatChannel()}`.";
				await owner.SendMessageAsync(m).CAF();
				return;
			}

			var reason = new ModerationReason(Context.User, "message deletion");

			//If not the context channel then get the first message in that channel
			var messageToStartAt = Context.Message.Channel.Id == channel.Id
				? Context.Message
				: (await channel.GetMessagesAsync(1).Flatten().CAF()).FirstOrDefault();

			//If there is a non null user then delete messages specifically from that user
			var deletedAmt = user == null
				? await MessageActions.RemoveMessagesAsync(channel, messageToStartAt, requestCount, reason).CAF()
				: await MessageActions.RemoveMessagesFromUserAsync(channel, messageToStartAt, requestCount, user, reason).CAF();

			//If the context channel isn't the targetted channel then delete the start message
			//Increase by one to account for it not being targetted.
			if (Context.Message.Channel.Id != channel.Id)
			{
				await MessageActions.DeleteMessageAsync(messageToStartAt, reason).CAF();
				deletedAmt++;
			}

			var response = $"Successfully deleted `{deletedAmt}` message{GetActions.GetPlural(deletedAmt)}";
			var userResp = user != null ? $" from `{user.FormatUser()}`" : null;
			var chanResp = channel != null ? $" on `{channel.FormatChannel()}`" : null;
			var resp = $"{GeneralFormatting.JoinNonNullStrings(" ", response, userResp, chanResp)}.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}

	[Group(nameof(ModifySlowmode)), TopLevelShortAlias(typeof(ModifySlowmode))]
	[Summary("First arg is how many messages can be sent in a timeframe. " +
		"Second arg is the timeframe. " +
		"Third arg is guildwide; true means yes, false means no. " +
		"Fourth are the list of roles that are immune to slowmode.")]
	[PermissionRequirement(null, null)]
	[DefaultEnabled(true)]
	public sealed class ModifySlowmode : SavingModuleBase
	{
		[Command(nameof(Create)), ShortAlias(nameof(Create))]
		public async Task Create([VerifyNumber(1, 5)] uint messages, [VerifyNumber(1, 30)] uint interval, [Optional] params IRole[] immuneRoles)
		{
			Context.GuildSettings.Slowmode = new Slowmode((int)messages, (int)interval, immuneRoles);
			var resp = $"Successfully setup slowmode.\n{Context.GuildSettings.Slowmode.ToString()}";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Enable)), ShortAlias(nameof(Enable))]
		public async Task Enable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				var error = new ErrorReason("There must be a slowmode set up before one can be enabled or disabled.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			Context.GuildSettings.Slowmode.Enable();
			var resp = $"Successfully enabled slowmode.\n{Context.GuildSettings.Slowmode.ToString()}";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
		[Command(nameof(Disable)), ShortAlias(nameof(Disable))]
		public async Task Disable()
		{
			if (Context.GuildSettings.Slowmode == null)
			{
				var error = new ErrorReason("There must be a slowmode set up before one can be enabled or disabled.");
				await MessageActions.SendErrorMessageAsync(Context, error).CAF();
				return;
			}

			Context.GuildSettings.Slowmode.Disable();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, "Successfully disabled slowmode.").CAF();
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
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("Cannot give the role being gathered.")).CAF();
				return;
			}

			var users = (await Context.Guild.GetUsersTheBotAndUserCanEditAsync(Context.User).CAF()).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).GiveRoleToManyUsersAsync(givenRole, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(TakeRole)), ShortAlias(nameof(TakeRole))]
		public async Task TakeRole(IRole targetRole,
			[VerifyObject(false, ObjectVerification.CanBeEdited, ObjectVerification.IsEveryone, ObjectVerification.IsManaged)] IRole takenRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetUsersTheBotAndUserCanEditAsync(Context.User).CAF()).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).TakeRoleFromManyUsersAsync(takenRole, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(GiveNickname)), ShortAlias(nameof(GiveNickname))]
		public async Task GiveNickname([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole targetRole,
			[VerifyStringLength(Target.Nickname)] string nickname,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetUsersTheBotAndUserCanEditAsync(Context.User).CAF()).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).NicknameManyUsersAsync(nickname, new ModerationReason(Context.User, null)).CAF();
		}
		[Command(nameof(TakeNickname)), ShortAlias(nameof(TakeNickname))]
		public async Task TakeNickname([VerifyObject(false, ObjectVerification.CanBeEdited)] IRole targetRole,
			[Optional, OverrideTypeReader(typeof(BypassUserLimitTypeReader))] bool bypass)
		{
			var users = (await Context.Guild.GetUsersTheBotAndUserCanEditAsync(Context.User).CAF()).Where(x => x.RoleIds.Contains(targetRole.Id));
			await new MultiUserAction(Context, users, bypass).NicknameManyUsersAsync(null, new ModerationReason(Context.User, null)).CAF();
		}
	}
}
