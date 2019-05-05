using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
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
			public async Task<RuntimeResult> Command()
			{
				var invites = (await Context.Guild.GetInvitesAsync().CAF()).OrderByDescending(x => x.Uses).ToArray();
				return Responses.Invites.DisplayInvites(invites);
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
			public Task<RuntimeResult> Command([Optional, ValidateTextChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketTextChannel channel,
				[Optional] CreateInviteArguments arguments)
				=> CommandRunner(channel, arguments);
			[Command]
			public Task<RuntimeResult> Command([ValidateVoiceChannel(CPerm.CreateInstantInvite, FromContext = true)] SocketVoiceChannel channel,
				[Optional] CreateInviteArguments arguments)
				=> CommandRunner(channel, arguments);

			private async Task<RuntimeResult> CommandRunner(INestedChannel channel, CreateInviteArguments arguments)
			{
				var invite = await channel.CreateInviteAsync(arguments.Time * 60, arguments.Uses, arguments.TemporaryMembership, false, GenerateRequestOptions()).CAF();
				return Responses.Invites.CreatedInvite(invite);
			}
		}

		[Group(nameof(DeleteInvite)), ModuleInitialismAlias(typeof(DeleteInvite))]
		[Summary("Deletes the invite with the given code.")]
		[UserPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteInvite : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command(IInvite invite)
			{
				await invite.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Invites.DeletedInvite(invite);
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
			public async Task<RuntimeResult> Command([Remainder] LocalInviteGatherer gatherer)
			{
				var invites = gatherer.GatherInvites(await Context.Guild.GetInvitesAsync().CAF()).ToArray();
				if (!invites.Any())
				{
					return Responses.Invites.NoInviteMatches();
				}

				foreach (var invite in invites)
				{
					await invite.DeleteAsync(GenerateRequestOptions()).CAF();
				}
				return Responses.Invites.DeletedMultipleInvites(invites);
			}
		}
	}
}
