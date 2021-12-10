using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.Models;

public sealed record RuleCategory(
	int Category,
	ulong GuildId,
	string Value
) : IGuildChild
{
	public RuleCategory() : this(default, default, "") { }
}