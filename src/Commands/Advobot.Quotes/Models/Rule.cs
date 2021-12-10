using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.Models;

public sealed record Rule(
	int Category,
	ulong GuildId,
	int Position,
	string Value
) : IGuildChild
{
	public Rule() : this(default, default, default, "") { }
}