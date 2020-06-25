using System.Collections.Generic;

using Advobot.Services.GuildSettings.Settings;

using Discord;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyRaidPrevention : IReadOnlyTimedPrevention
	{
		RaidType RaidType { get; }

		bool IsRaid(IGuildUser user);

		bool ShouldPunish(IEnumerable<ulong> users);
	}
}