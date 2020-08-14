using System;

using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Models
{
	public class TimedPrevention : Punishment, IReadOnlyTimedPrevention
	{
		public bool Enabled { get; set; }
		public TimeSpan Interval => new TimeSpan(IntervalTicks);
		public long IntervalTicks { get; set; }
		public int Size { get; set; }

		public TimedPrevention()
		{
		}

		public TimedPrevention(IReadOnlyTimedPrevention other) : base(other)
		{
			Enabled = other.Enabled;
			IntervalTicks = other.Interval.Ticks;
			Size = other.Size;
		}
	}
}