using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Quotes.Models
{
	public sealed class RuleCategory : IReadOnlyRuleCategory
	{
		public int Category { get; set; }
		public ulong GuildId { get; set; }
		public string Value { get; set; }

		public RuleCategory()
		{
			Value = "";
		}

		public RuleCategory(IReadOnlyRuleCategory other)
		{
			Category = other.Category;
			GuildId = other.GuildId;
			Value = other.Value;
		}
	}
}