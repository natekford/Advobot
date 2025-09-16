using Advobot.Attributes;
using Advobot.Modules;
using Advobot.Preconditions;
using Advobot.Resources;

using Discord;

using YACCS.Commands.Attributes;
using YACCS.Localization;

namespace Advobot.Standard.Commands;

[LocalizedCategory(nameof(Guilds))]
public sealed class Guilds : AdvobotModuleBase
{
	[LocalizedCommand(nameof(Groups.LeaveGuild), nameof(Aliases.LeaveGuild))]
	[LocalizedSummary(nameof(Summaries.LeaveGuild))]
	[Id("3090730c-1377-4a56-b379-485baed393e7")]
	[Meta(IsEnabled = true)]
	public sealed class LeaveGuild : AdvobotModuleBase
	{
		[Command]
		[RequireGuildOwner]
		public Task Current()
			=> Context.Guild.LeaveAsync();

		[Command]
		[RequireBotOwner]
		public async Task<AdvobotResult> Targeted([Remainder] IGuild guild)
		{
			await guild.LeaveAsync().ConfigureAwait(false);
			return Responses.Guilds.LeftGuild(guild);
		}
	}
}