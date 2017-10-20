using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Advobot.Commands.InviteModeration
{
	[Group(nameof(DisplayInvites)), TopLevelShortAlias(typeof(DisplayInvites))]
	[Summary("Gives a list of all the instant invites on the guild.")]
	[OtherRequirement(Precondition.UserHasAPerm)]
	[DefaultEnabled(true)]
	public sealed class DisplayInvites : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var invites = (await Context.Guild.GetInvitesAsync().CAF()).OrderByDescending(x => x.Uses);
			if (!invites.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("This guild has no invites.")).CAF();
				return;
			}

			var lenForCode = invites.Max(x => x.Code.Length);
			var lenForUses = invites.Max(x => x.Uses).GetLengthOfNumber();
			var desc = String.Join("\n", invites.FormatNumberedList("`{0}` `{1}` `{2}`",
				x => x.Code.PadRight(lenForCode),
				x => x.Uses.ToString().PadRight(lenForUses),
				x => x.Inviter.FormatUser()));
			await MessageActions.SendEmbedMessageAsync(Context.Channel, new AdvobotEmbed("Instant Invite List", desc)).CAF();
		}
	}

	[Group(nameof(CreateInvite)), TopLevelShortAlias(typeof(CreateInvite))]
	[Summary("Creates an invite on the given channel. " +
		"No time specifies to not expire. " +
		"No uses has no usage limit. " +
		"Temp membership means when the user goes offline they get kicked.")]
	[PermissionRequirement(new[] { GuildPermission.CreateInstantInvite }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateInvite : AdvobotModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(true, ObjectVerification.CanCreateInstantInvite)] IGuildChannel channel,
			[Optional, VerifyNumber(0, 1800, 3600, 21600, 43200, 86400)] int time,
			[Optional, VerifyNumber(0, 1, 5, 10, 25, 50, 100)] int uses,
			[Optional] bool tempMem)
		{
			var nullableTime = time != 0 ? time as int? : 86400;
			var nullableUses = uses != 0 ? uses as int? : null;
			var inv = await InviteActions.CreateInviteAsync(channel, nullableTime, nullableUses, tempMem, false, new ModerationReason(Context.User, null)).CAF();

			var timeOutputStr = nullableTime.HasValue
				? $"It will last for this amount of time: `{nullableTime}`." 
				: "It will last until manually revoked.";
			var usesOutputStr = nullableUses.HasValue 
				? $"It will last for this amount of uses: `{nullableUses}`." 
				: "It has no usage limit.";
			var tempOutputStr = tempMem 
				? "Users will be kicked when they go offline unless they get a role." 
				: "Users will not be kicked when they go offline and do not have a role.";
			var joined = GeneralFormatting.JoinNonNullStrings("\n", inv.Url, timeOutputStr, usesOutputStr, tempOutputStr);
			var resp = $"Here is your invite for `{channel.FormatChannel()}`: {joined}";
			await MessageActions.SendMessageAsync(Context.Channel, resp).CAF();
		}
	}

	[Group(nameof(DeleteInvite)), TopLevelShortAlias(typeof(DeleteInvite))]
	[Summary("Deletes the invite with the given code.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteInvite : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IInvite invite)
		{
			await InviteActions.DeleteInviteAsync(invite, new ModerationReason(Context.User, null)).CAF();
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the invite `{invite.Code}`.").CAF();
		}
	}

	[Group(nameof(DeleteMultipleInvites)), TopLevelShortAlias(typeof(DeleteMultipleInvites))]
	[Summary("Deletes all invites satisfying the given conditions. " +
		"CountTarget parameters are either `Equal`, `Below`, or `Above`. " +
		"IsTemporary, NeverExpires, and NoMaxUses are either `True`, or `False`.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteMultipleInvites : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Remainder] CustomArguments<MultipleInviteGatherer> gatherer)
		{
			var invites = gatherer.CreateObject().GatherInvites(await Context.Guild.GetInvitesAsync().CAF());
			if (!invites.Any())
			{
				await MessageActions.SendErrorMessageAsync(Context, new ErrorReason("No invites satisfied the given conditions.")).CAF();
				return;
			}

			foreach (var invite in invites)
			{
				await InviteActions.DeleteInviteAsync(invite, new ModerationReason(Context.User, null)).CAF();
			}

			var resp = $"Successfully deleted `{invites.Count()}` instant invites.";
			await MessageActions.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
