using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Core.Classes;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.NamedArguments;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.InviteModeration
{
	[Group(nameof(DisplayInvites)), TopLevelShortAlias(typeof(DisplayInvites))]
	[Summary("Gives a list of all the instant invites on the guild.")]
	[OtherRequirement(Precondition.GenericPerms)]
	[DefaultEnabled(true)]
	public sealed class DisplayInvites : NonSavingModuleBase
	{
		[Command]
		public async Task Command()
		{
			var invites = (await Context.Guild.GetInvitesAsync().CAF()).OrderByDescending(x => x.Uses).ToList();
			if (!invites.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("This guild has no invites.")).CAF();
				return;
			}

			var lenForCode = invites.Max(x => x.Code.Length);
			var lenForUses = invites.Max(x => x.Uses).ToString().Length;
			var desc = String.Join("\n", invites.FormatNumberedList(x =>
			{
				var code = x.Code.PadRight(lenForCode);
				var uses = x.Uses.ToString().PadRight(lenForUses);
				var inviter = x.Inviter.Format();
				return $"`{code}` `{uses}` `{inviter}`";
			}));
			var embed = new EmbedWrapper
			{
				Title = "Instant Invite List",
				Description = desc
			};
			await MessageUtils.SendEmbedMessageAsync(Context.Channel, embed).CAF();
		}
	}

	[Group(nameof(CreateInvite)), TopLevelShortAlias(typeof(CreateInvite))]
	[Summary("Creates an invite on the given channel. " +
		"No time specifies to not expire. " +
		"No uses has no usage limit. " +
		"Temp membership means when the user goes offline they get kicked.")]
	[PermissionRequirement(new[] { GuildPermission.CreateInstantInvite }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateInvite : NonSavingModuleBase
	{
		[Command]
		public async Task Command([VerifyObject(true, ObjectVerification.CanCreateInstantInvite)] IGuildChannel channel,
			[Optional, VerifyNumber(new[] { 0, 1800, 3600, 21600, 43200, 86400 })] int time,
			[Optional, VerifyNumber(new[] { 0, 1, 5, 10, 25, 50, 100 })] int uses,
			[Optional] bool tempMem)
		{
			var nullableTime = time != 0 ? time as int? : 86400;
			var nullableUses = uses != 0 ? uses as int? : null;
			var inv = await InviteUtils.CreateInviteAsync(channel, nullableTime, nullableUses, tempMem, false, new ModerationReason(Context.User, null)).CAF();

			var timeOutputStr = uses != 0
				? $"It will last for this amount of time: `{nullableTime}`." 
				: "It will last until manually revoked.";
			var usesOutputStr = time != 0
				? $"It will last for this amount of uses: `{nullableUses}`." 
				: "It has no usage limit.";
			var tempOutputStr = tempMem 
				? "Users will be kicked when they go offline unless they get a role." 
				: "Users will not be kicked when they go offline and do not have a role.";
			var joined = new[] { inv.Url, timeOutputStr, usesOutputStr, tempOutputStr }.JoinNonNullStrings("\n");
			var resp = $"Here is your invite for `{channel.Format()}`: {joined}";
			await MessageUtils.SendMessageAsync(Context.Channel, resp).CAF();
		}
	}

	[Group(nameof(DeleteInvite)), TopLevelShortAlias(typeof(DeleteInvite))]
	[Summary("Deletes the invite with the given code.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteInvite : NonSavingModuleBase
	{
		[Command]
		public async Task Command(IInvite invite)
		{
			await InviteUtils.DeleteInviteAsync(invite, new ModerationReason(Context.User, null)).CAF();
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, $"Successfully deleted the invite `{invite.Code}`.").CAF();
		}
	}

	[Group(nameof(DeleteMultipleInvites)), TopLevelShortAlias(typeof(DeleteMultipleInvites))]
	[Summary("Deletes all invites satisfying the given conditions. " +
		"CountTarget parameters are either `Equal`, `Below`, or `Above`. " +
		"IsTemporary, NeverExpires, and NoMaxUses are either `True`, or `False`.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteMultipleInvites : NonSavingModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Remainder] NamedArguments<MultipleInviteGatherer> gatherer)
		{
			var invites = gatherer.CreateObject().GatherInvites(await Context.Guild.GetInvitesAsync().CAF()).ToList();
			if (!invites.Any())
			{
				await MessageUtils.SendErrorMessageAsync(Context, new Error("No invites satisfied the given conditions.")).CAF();
				return;
			}

			foreach (var invite in invites)
			{
				await InviteUtils.DeleteInviteAsync(invite, new ModerationReason(Context.User, null)).CAF();
			}

			var resp = $"Successfully deleted `{invites.Count()}` instant invites.";
			await MessageUtils.MakeAndDeleteSecondaryMessageAsync(Context, resp).CAF();
		}
	}
}
