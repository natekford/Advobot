using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Quotes.Models
{
	public sealed class RuleCategory : IReadOnlyRuleCategory
	{
		public int Category { get; set; }
		public ulong GuildId { get; set; }
		public string Name { get; set; }

		public RuleCategory()
		{
			Name = "";
		}

		public RuleCategory(IReadOnlyRuleCategory other)
		{
			Category = other.Category;
			GuildId = other.GuildId;
			Name = other.Name;
		}
	}
}