using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Enums;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Advobot.Commands.Invites
{
	[Category(typeof(DisplayInvites)), Group(nameof(DisplayInvites)), TopLevelShortAlias(typeof(DisplayInvites))]
	[Summary("Gives a list of all the instant invites on the guild.")]
	[PermissionRequirement(new[] { GuildPermission.ManageGuild }, null)]
	[DefaultEnabled(true)]
	public sealed class DisplayInvites : AdvobotModuleBase
	{
		[Command]
		public async Task Command()
		{
			var invites = (await Context.Guild.GetInvitesAsync().CAF()).OrderByDescending(x => x.Uses).ToList();
			if (!invites.Any())
			{
				await ReplyErrorAsync(new Error("This guild has no invites.")).CAF();
				return;
			}

			var lenForCode = invites.Max(x => x.Code.Length);
			var lenForUses = invites.Max(x => x.Uses).ToString().Length;
			var desc = string.Join("\n", invites.FormatNumberedList(x =>
			{
				var code = x.Code.PadRight(lenForCode);
				var uses = x.Uses.ToString().PadRight(lenForUses);
				var inviter = x.Inviter.Format();
				return $"`{code}` `{uses}` `{inviter}`";
			}));
			await ReplyEmbedAsync(new EmbedWrapper
			{
				Title = "Instant Invite List",
				Description = desc,
			}).CAF();
		}
	}

	[Category(typeof(CreateInvite)), Group(nameof(CreateInvite)), TopLevelShortAlias(typeof(CreateInvite))]
	[Summary("Creates an invite on the given channel. " +
		"No time specifies to not expire. " +
		"No uses has no usage limit. " +
		"Temp membership means when the user goes offline they get kicked.")]
	[PermissionRequirement(new[] { GuildPermission.CreateInstantInvite }, null)]
	[DefaultEnabled(true)]
	public sealed class CreateInvite : AdvobotModuleBase
	{
		[Command]
		public async Task Command(
			[ValidateObject(Verif.CanCreateInstantInvite, IfNullCheckFromContext = true)] SocketGuildChannel channel,
			[Optional, ValidateNumber(new[] { 0, 1800, 3600, 21600, 43200, 86400 })] int time,
			[Optional, ValidateNumber(new[] { 0, 1, 5, 10, 25, 50, 100 })] int uses,
			[Optional] bool tempMem)
		{
			var nullableTime = time != 0 ? time as int? : 86400;
			var nullableUses = uses != 0 ? uses as int? : null;
			var inv = await channel.CreateInviteAsync(nullableTime, nullableUses, tempMem, false, GetRequestOptions()).CAF();

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
			await ReplyAsync($"Here is your invite for `{channel.Format()}`: {joined}").CAF();
		}
	}

	[Category(typeof(DeleteInvite)), Group(nameof(DeleteInvite)), TopLevelShortAlias(typeof(DeleteInvite))]
	[Summary("Deletes the invite with the given code.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteInvite : AdvobotModuleBase
	{
		[Command]
		public async Task Command(IInvite invite)
		{
			await invite.DeleteAsync(GetRequestOptions()).CAF();
			await ReplyTimedAsync($"Successfully deleted the invite `{invite.Code}`.").CAF();
		}
	}

	[Category(typeof(DeleteMultipleInvites)), Group(nameof(DeleteMultipleInvites)), TopLevelShortAlias(typeof(DeleteMultipleInvites))]
	[Summary("Deletes all invites satisfying the given conditions. " +
		"CountTarget parameters are either `Equal`, `Below`, or `Above`. " +
		"IsTemporary, NeverExpires, and NoMaxUses are either `True`, or `False`.")]
	[PermissionRequirement(new[] { GuildPermission.ManageChannels }, null)]
	[DefaultEnabled(true)]
	public sealed class DeleteMultipleInvites : AdvobotModuleBase
	{
		[Command(RunMode = RunMode.Async)]
		public async Task Command([Remainder] LocalInviteGatherer gatherer)
		{
			var invites = gatherer.GatherInvites(await Context.Guild.GetInvitesAsync().CAF()).ToList();
			if (!invites.Any())
			{
				await ReplyErrorAsync(new Error("No invites satisfied the given conditions.")).CAF();
				return;
			}

			foreach (var invite in invites)
			{
				await invite.DeleteAsync(GetRequestOptions()).CAF();
			}
			await ReplyTimedAsync($"Successfully deleted `{invites.Count()}` instant invites.").CAF();
		}
	}
}
