using Discord;
using System;

namespace Advobot.Gacha.Checkers
{
	public sealed class CheckersService : ICheckersService
	{
		private readonly CounterCollection _RollCheckers = new CounterCollection(10, TimeSpan.FromHours(1));
		private readonly CounterCollection _ClaimCheckers = new CounterCollection(1, TimeSpan.FromHours(3));

		public IChecker<ulong> GetRollChecker(IGuild guild)
			=> _RollCheckers.GetCounter(guild);
		public IChecker<ulong> GetClaimChecker(IGuild guild)
			=> _ClaimCheckers.GetCounter(guild);
	}
}
