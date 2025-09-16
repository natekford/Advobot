using Advobot.Attributes;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Invites;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Invites))]
public sealed class Invites : AdvobotModuleBase
{
	// This can be done in mobile, but if there are too many invites it could be annoying
	[LocalizedCommand(nameof(Groups.DeleteInvite), nameof(Aliases.DeleteInvite))]
	[LocalizedSummary(nameof(Summaries.DeleteInvite))]
	[Id("993e5613-6cdb-4ff3-925d-98e3a534ddc8")]
	[Meta(IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels)]
	public sealed class DeleteInvite : AdvobotModuleBase
	{
		[Command]
		public async Task<AdvobotResult> Targeted(
			[FromThisGuild]
			IInviteMetadata invite)
		{
			await invite.DeleteAsync(GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.Deleted(invite);
		}
	}
}