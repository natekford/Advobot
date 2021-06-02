namespace Advobot.AutoMod.Models
{
	public sealed record RaidPrevention(
		RaidType RaidType
	) : TimedPrevention
	{
		public RaidPrevention() : this(default(RaidType)) { }
	}
}