using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Channels;
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Classes;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Modules;
using AdvorangesUtils;
using Discord;
using Discord.Commands;
using static Discord.ChannelPermission;

namespace Advobot.Commands.Standard
{
	public sealed class Invites : ModuleBase
	{
		[Group(nameof(DisplayInvites)), ModuleInitialismAlias(typeof(DisplayInvites))]
		[LocalizedSummary(nameof(Summaries.DisplayInvites))]
		[GuildPermissionRequirement(GuildPermission.ManageGuild)]
		[EnabledByDefault(true)]
		public sealed class DisplayInvites : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command()
			{
				var invites = await Context.Guild.GetInvitesAsync().CAF();
				var ordered = invites.OrderByDescending(x => x.Uses).ToArray();
				return Responses.Invites.DisplayInvites(ordered);
			}
		}

		[Group(nameof(CreateInvite)), ModuleInitialismAlias(typeof(CreateInvite))]
		[LocalizedSummary(nameof(Summaries.CreateInvite))]
		[GuildPermissionRequirement(GuildPermission.CreateInstantInvite)]
		[EnabledByDefault(true)]
		public sealed class CreateInvite : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(CreateInstantInvite)] ITextChannel channel,
				[Optional] CreateInviteArguments? arguments)
				=> CommandRunner(channel, arguments);
			[Command]
			public Task<RuntimeResult> Command(
				[Channel(CreateInstantInvite)] IVoiceChannel channel,
				[Optional] CreateInviteArguments? arguments)
				=> CommandRunner(channel, arguments);

			private async Task<RuntimeResult> CommandRunner(
				INestedChannel channel,
				CreateInviteArguments? arguments)
			{
				arguments ??= new CreateInviteArguments();
				var invite = await arguments.CreateInviteAsync(channel, GenerateRequestOptions()).CAF();
				return Responses.Invites.CreatedInvite(invite);
			}
		}

		[Group(nameof(DeleteInvite)), ModuleInitialismAlias(typeof(DeleteInvite))]
		[LocalizedSummary(nameof(Summaries.DeleteInvite))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
		[EnabledByDefault(true)]
		public sealed class DeleteInvite : AdvobotModuleBase
		{
			[Command]
			public async Task<RuntimeResult> Command([FromThisGuild] IInviteMetadata invite)
			{
				await invite.DeleteAsync(GenerateRequestOptions()).CAF();
				return Responses.Invites.DeletedInvite(invite);
			}
		}

		[Group(nameof(DeleteMultipleInvites)), ModuleInitialismAlias(typeof(DeleteMultipleInvites))]
		[LocalizedSummary(nameof(Summaries.DeleteMultipleInvites))]
		[GuildPermissionRequirement(GuildPermission.ManageChannels)]
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
