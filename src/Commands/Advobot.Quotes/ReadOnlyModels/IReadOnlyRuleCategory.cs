using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.ReadOnlyModels
{
	public interface IReadOnlyRuleCategory : IGuildChild
	{
		int Category { get; }
		string Value { get; }
	}
}