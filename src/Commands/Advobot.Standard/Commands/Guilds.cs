using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Preconditions;
using Advobot.Resources;

using Discord;
using Discord.Commands;

namespace Advobot.Standard.Commands;

[Category(nameof(Guilds))]
public sealed class Guilds : ModuleBase
{
	[LocalizedGroup(nameof(Groups.LeaveGuild))]
	[LocalizedAlias(nameof(Aliases.LeaveGuild))]
	[LocalizedSummary(nameof(Summaries.LeaveGuild))]
	[Meta("3090730c-1377-4a56-b379-485baed393e7", IsEnabled = true)]
	public sealed class LeaveGuild : AdvobotModuleBase
	{
		[Command]
		[RequireGuildOwner]
		public Task Command()
			=> Context.Guild.LeaveAsync();

		[Command]
		[RequireBotOwner]
		public async Task<RuntimeResult> Command([Remainder] IGuild guild)
		{
			await guild.LeaveAsync().ConfigureAwait(false);
			return Responses.Guilds.LeftGuild(guild);
		}
	}
}