namespace Advobot.AutoMod.Models;

public sealed record SpamPrevention(
	SpamType SpamType
) : TimedPrevention
{
	public SpamPrevention() : this(default(SpamType)) { }
}