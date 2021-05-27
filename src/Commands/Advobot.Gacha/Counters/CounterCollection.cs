using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Discord;

namespace Advobot.Gacha.Counters
{
	public sealed class CounterCollection
	{
		private static readonly DateTimeOffset _Epoch = new DateTime(2019, 1, 1);
		private static readonly TimeSpan _Minute = TimeSpan.FromMinutes(1);
		private static readonly TimeSpan _Period = Timeout.InfiniteTimeSpan;
		private static readonly TimeSpan _StaggerInterval = TimeSpan.FromHours(1);

		private readonly ConcurrentDictionary<ulong, ICounter<ulong>> _Counters = new();
		private readonly int _DefaultAmount;
		private readonly TimeSpan _Interval;
		private readonly bool _IsStaggered;
		private readonly int _ResetMinute;
		private readonly Timer _Timer;

		public CounterCollection(int defaultAmount, TimeSpan interval, int resetMinute = 0)
		{
			if (resetMinute < -1 || resetMinute > 59)
			{
				const string msg = $"{nameof(resetMinute)} must be between -1 and 59 inclusive.";
				throw new ArgumentException(msg, nameof(resetMinute));
			}
			if (interval < _StaggerInterval)
			{
				var msg = $"{nameof(interval)} must be bigger than {_StaggerInterval}.";
				throw new ArgumentException(msg, nameof(interval));
			}

			_DefaultAmount = defaultAmount;
			_Interval = interval;
			_ResetMinute = resetMinute;
			_IsStaggered = resetMinute == -1;

			var firstInterval = CalculateFirstInterval();
			_Timer = new Timer(Callback, new StrongBox<ulong>(0), firstInterval, _Period);
		}

		public ICounter<ulong> GetCounter(IGuild guild)
			=> _Counters.GetOrAdd(guild.Id, _ => new Counter(_DefaultAmount));

		private TimeSpan CalculateFirstInterval()
		{
			var diff = DateTimeOffset.UtcNow.Ticks - _Epoch.Ticks;
			var mod = diff % _Interval.Ticks;
			var ts = _Interval - new TimeSpan(mod);
			return _IsStaggered ? ts : ts + TimeSpan.FromMinutes(_ResetMinute);
		}

		private void Callback(object? state)
		{
			if (!_IsStaggered)
			{
				_Counters.Clear();
				_Timer.Change(_Interval, _Period);
				return;
			}
			if (state is not StrongBox<ulong> i)
			{
				throw new ArgumentException($"State is not a {nameof(StrongBox<ulong>)}.", nameof(state));
			}

			//Remove guilds where their id % time is the current minute state
			var minutes = (ulong)_StaggerInterval.TotalMinutes;
			var removable = _Counters.Keys.Where(x => x % minutes == i.Value).ToArray();
			foreach (var key in removable)
			{
				_Counters.TryRemove(key, out _);
			}

			//Still not over the stagger period so repeat each minute
			if (++i.Value < minutes)
			{
				_Timer.Change(_Minute, _Period);
				return;
			}

			//Stagger period over so reset state and go back to waiting for the regular interval
			i.Value = 0;
			_Timer.Change(_Interval - _StaggerInterval, _Period);
		}
	}
}