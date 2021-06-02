using System;
using System.Threading;

using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models
{
	public record AutoModSettings(
		ulong GuildId,
		long Ticks = 0,
		bool IgnoreAdmins = true,
		bool IgnoreHigherHierarchy = true
	) : IGuildChild
	{
		public bool CheckDuration => Duration != Timeout.InfiniteTimeSpan;
		public TimeSpan Duration => new(Ticks);

		public AutoModSettings() : this(0) { }
	}
}