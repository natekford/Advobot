using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Models
{
	public sealed class SpamPrevention : TimedPrevention, IReadOnlySpamPrevention
	{
		public SpamType SpamType { get; set; }

		public SpamPrevention()
		{
		}

		public SpamPrevention(IReadOnlySpamPrevention other) : base(other)
		{
			SpamType = other.SpamType;
		}
	}
}