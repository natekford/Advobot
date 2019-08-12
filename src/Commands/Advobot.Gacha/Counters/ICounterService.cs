using Discord;

namespace Advobot.Gacha.Counters
{
	public interface ICounterService
	{
		ICounter<ulong> GetRolls(IGuild guild);
		ICounter<ulong> GetClaims(IGuild guild);
	}
}
