using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.ReadOnlyModels
{
	public interface IReadOnlyRule : IGuildChild
	{
		int Category { get; }
		int Position { get; }
		string Value { get; }
	}
}