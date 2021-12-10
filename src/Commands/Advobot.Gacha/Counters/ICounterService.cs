using Discord;

namespace Advobot.Gacha.Counters;

public interface ICounterService
{
	ICounter<ulong> GetClaims(IGuild guild);

	ICounter<ulong> GetRolls(IGuild guild);
}