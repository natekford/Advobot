using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Models
{
	public sealed class RaidPrevention : TimedPrevention, IReadOnlyRaidPrevention
	{
		public RaidType RaidType { get; set; }

		public RaidPrevention()
		{
		}

		public RaidPrevention(IReadOnlyRaidPrevention other) : base(other)
		{
			RaidType = other.RaidType;
		}
	}
}