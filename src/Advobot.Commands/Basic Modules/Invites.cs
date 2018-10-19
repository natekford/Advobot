using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	[Group]
	public sealed class Invites : ModuleBase
	{
		[Group(nameof(DisplayInvites)), TopLevelShortAlias(typeof(DisplayInvites))]
		[Summary("Gives a list of all the instant invites on the guild.")]
		[UserPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class DisplayInvites : AdvobotModuleBase
		{
			[Command]
			public async Task Command()
			{
				var invites = (await Context.Guild.GetInvitesAsync().CAF()).OrderByDescending(x => x.Uses).ToArray();
				var lenForCode = 0;
				var lenForUses = 0;
				foreach (var invite in invites)
				{
					lenForCode = Math.Max(lenForCode, invite.Code.Length);
					lenForUses = Math.Max(lenForUses, invite.Uses.ToString().Length);
				}
				await ReplyIfAny(invites, "Invites", x =>
				{
					var code = x.Code.PadRight(lenForCode);
					var uses = x.Uses.ToString().PadRight(lenForUses);
					var inviter = x.Inviter.Format();
					return $"`{code}` `{uses}` `{inviter}`";
				}).CAF();
			}
		}

		[Group(nameof(CreateInvite)), TopLevelShortAlias(typeof(CreateInvite))]
		[Summary("Creates an invite on the given channel. " +
			"No time specifies to not expire. " +
			"No uses has no usage limit. " +
			"Temp membership means when the user goes offline they get kicked.")]
		[UserPermissionRequirement(GuildPermission.CreateInstantInvite)]
		[EnabledByDefault(true)]
		public sealed class CreateInvite : AdvobotModuleBase
		{
#warning redo how arguments are parsed here
			[Command]
			public async Task Command(
				[Optional, ValidateTextChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketTextChannel channel,
				[Optional, ValidateInviteTime] int time,
				[Optional, ValidateInviteUses] int uses,
				[Optional] bool tempMem)
				=> await CommandRunner(channel, time, uses, tempMem).CAF();
			[Command]
			public async Task Command(
				[ValidateVoiceChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketVoiceChannel channel,
				[Optional, ValidateInviteTime] int time,
				[Optional, ValidateInviteUses] int uses,
				[Optional] bool tempMem)
				=> await CommandRunner(channel, time, uses, tempMem).CAF();

			private async Task CommandRunner(SocketGuildChannel channel, int time, int uses, bool tempMem)
			{
				var nullableTime = time != 0 ? time as int? : 86400;
				var nullableUses = uses != 0 ? uses as int? : null;
				var inv = await channel.CreateInviteAsync(nullableTime, nullableUses, tempMem, false, GenerateRequestOptions()).CAF();

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

		[Group(nameof(DeleteInvite)), TopLevelShortAlias(typeof(DeleteInvite))]
		[Summary("Deletes the invite with the given code.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteInvite : AdvobotModuleBase
		{
			[Command]
			public async Task Command(IInvite invite)
			{
				await invite.DeleteAsync(GenerateRequestOptions()).CAF();
				await ReplyTimedAsync($"Successfully deleted the invite `{invite.Code}`.").CAF();
			}
		}

		[Group(nameof(DeleteMultipleInvites)), TopLevelShortAlias(typeof(DeleteMultipleInvites))]
		[Summary("Deletes all invites satisfying the given conditions. " +
			"CountTarget parameters are either `Equal`, `Below`, or `Above`. " +
			"IsTemporary, NeverExpires, and NoMaxUses are either `True`, or `False`.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
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
					await invite.DeleteAsync(GenerateRequestOptions()).CAF();
				}
				await ReplyTimedAsync($"Successfully deleted `{invites.Count()}` instant invites.").CAF();
			}
		}
	}
}
