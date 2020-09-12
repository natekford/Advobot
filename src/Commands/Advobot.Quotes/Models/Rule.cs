using Advobot.Quotes.ReadOnlyModels;

namespace Advobot.Quotes.Models
{
	public sealed class Rule : IReadOnlyRule
	{
		public int Category { get; set; }
		public ulong GuildId { get; set; }
		public int Position { get; set; }
		public string Value { get; set; }

		public Rule()
		{
			Value = "";
		}

		public Rule(IReadOnlyRule other)
		{
			Category = other.Category;
			GuildId = other.GuildId;
			Position = other.Position;
			Value = other.Value;
		}
	}
}