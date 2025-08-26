using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.ParameterPreconditions.Discord.Channels;
using Advobot.ParameterPreconditions.Discord.Invites;
using Advobot.Preconditions.Permissions;
using Advobot.Resources;
using Advobot.TypeReaders;

using Discord;
using Discord.Commands;

using static Discord.ChannelPermission;

namespace Advobot.Standard.Commands;

[Category(nameof(Invites))]
public sealed class Invites : ModuleBase
{
	// This can be done in mobile, but if there are too many invites it could be annoying
	[LocalizedGroup(nameof(Groups.DeleteInvite))]
	[LocalizedAlias(nameof(Aliases.DeleteInvite))]
	[LocalizedSummary(nameof(Summaries.DeleteInvite))]
	[Meta("993e5613-6cdb-4ff3-925d-98e3a534ddc8", IsEnabled = true)]
	[RequireGuildPermissions(GuildPermission.ManageChannels)]
	public sealed class DeleteInvite : AdvobotModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command([FromThisGuild] IInviteMetadata invite)
		{
			await invite.DeleteAsync(GetOptions()).ConfigureAwait(false);
			return Responses.Snowflakes.Deleted(invite);
		}
	}
}