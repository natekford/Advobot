using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using CPerm = Discord.ChannelPermission;

namespace Advobot.Commands
{
	public sealed class Invites : ModuleBase
	{
		[Group(nameof(DisplayInvites)), ModuleInitialismAlias(typeof(DisplayInvites))]
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

		[Group(nameof(CreateInvite)), ModuleInitialismAlias(typeof(CreateInvite))]
		[Summary("Creates an invite on the given channel. " +
			"No time specifies to not expire. " +
			"No uses has no usage limit. " +
			"Temp membership means when the user goes offline they get kicked.")]
		[UserPermissionRequirement(GuildPermission.CreateInstantInvite)]
		[EnabledByDefault(true)]
		public sealed class CreateInvite : AdvobotModuleBase
		{
			[Command]
			public Task Command(
				[Optional, ValidateTextChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketTextChannel channel,
				[Optional] CreateInviteArguments arguments)
				=> CommandRunner(channel, arguments);
			[Command]
			public Task Command(
				[ValidateVoiceChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketVoiceChannel channel,
				[Optional] CreateInviteArguments arguments)
				=> CommandRunner(channel, arguments);

			private async Task CommandRunner(INestedChannel channel, CreateInviteArguments arguments)
			{
				var invite = await channel.CreateInviteAsync(arguments.Time * 60, arguments.Uses, arguments.TemporaryMembership, false, GenerateRequestOptions()).CAF();
				await ReplyAsync($"Successfully created `{invite.Format()}`.").CAF();
			}
		}

		[Group(nameof(DeleteInvite)), ModuleInitialismAlias(typeof(DeleteInvite))]
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

		[Group(nameof(DeleteMultipleInvites)), ModuleInitialismAlias(typeof(DeleteMultipleInvites))]
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
				var invites = gatherer.GatherInvites(await Context.Guild.GetInvitesAsync().CAF()).ToArray();
				if (!invites.Any())
				{
					await ReplyErrorAsync("No invites satisfied the given conditions.").CAF();
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
