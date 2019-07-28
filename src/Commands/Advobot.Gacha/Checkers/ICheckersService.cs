using Discord;

namespace Advobot.Gacha.Checkers
{
	public interface ICheckersService
	{
		IChecker<ulong> GetRollChecker(IGuild guild);
		IChecker<ulong> GetClaimChecker(IGuild guild);
	}
}
