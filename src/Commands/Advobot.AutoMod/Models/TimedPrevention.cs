namespace Advobot.AutoMod.Models;

public record TimedPrevention(
	bool Enabled,
	long IntervalTicks,
	int Size
) : Punishment
{
	public TimeSpan Interval => new(IntervalTicks);

	public TimedPrevention() : this(default, default, default) { }
}