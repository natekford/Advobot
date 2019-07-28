using Discord;
using System;
using System.Collections.Concurrent;
using System.Timers;

namespace Advobot.Gacha.Checkers
{
	public sealed class CounterCollection
	{
		private readonly ConcurrentDictionary<ulong, IChecker<ulong>> _Counters
			= new ConcurrentDictionary<ulong, IChecker<ulong>>();
		private readonly Timer _Timer;
		private readonly int _DefaultAmount;

		public CounterCollection(int defaultAmount, TimeSpan reset)
		{
			_DefaultAmount = defaultAmount;
			_Timer = new Timer(reset.TotalMilliseconds);
			_Timer.Elapsed += (sender, e) => _Counters.Clear();
			_Timer.Start();
		}

		public IChecker<ulong> GetCounter(IGuild guild)
			=> _Counters.GetOrAdd(guild.Id, key => new Counter(_DefaultAmount));
	}
}
