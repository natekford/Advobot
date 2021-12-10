using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models;

public record AutoModSettings(
	ulong GuildId,
	long Ticks,
	bool IgnoreAdmins,
	bool IgnoreHigherHierarchy
) : IGuildChild
{
	public bool CheckDuration => Duration != Timeout.InfiniteTimeSpan;
	public TimeSpan Duration => new(Ticks);

	public AutoModSettings() : this(default, Ticks: TimeSpan.TicksPerHour, IgnoreAdmins: true, IgnoreHigherHierarchy: true) { }
}