using System;
using Discord;

namespace Advobot.Gacha.Counters
{
	public sealed class CounterService : ICounterService
	{
		private readonly CounterCollection _RollCheckers
			= new CounterCollection(10, TimeSpan.FromHours(1), -1);
		private readonly CounterCollection _ClaimCheckers
			= new CounterCollection(1, TimeSpan.FromHours(3));

		public ICounter<ulong> GetRolls(IGuild guild)
			=> _RollCheckers.GetCounter(guild);
		public ICounter<ulong> GetClaims(IGuild guild)
			=> _ClaimCheckers.GetCounter(guild);
	}
}
