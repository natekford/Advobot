using System;

using Discord;

namespace Advobot.Gacha.Counters
{
	public sealed class CounterService : ICounterService
	{
		private readonly CounterCollection _ClaimCheckers = new(1, TimeSpan.FromHours(3));
		private readonly CounterCollection _RollCheckers = new(10, TimeSpan.FromHours(1), -1);

		public ICounter<ulong> GetClaims(IGuild guild)
			=> _ClaimCheckers.GetCounter(guild);

		public ICounter<ulong> GetRolls(IGuild guild)
			=> _RollCheckers.GetCounter(guild);
	}
}