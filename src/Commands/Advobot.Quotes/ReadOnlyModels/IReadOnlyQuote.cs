using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.ReadOnlyModels
{
	public interface IReadOnlyQuote : IGuildChild
	{
		string Description { get; }
		string Name { get; }
	}
}