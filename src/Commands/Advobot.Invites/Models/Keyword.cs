using Advobot.SQLite.Relationships;

namespace Advobot.Invites.Models;

public sealed record Keyword(
	ulong GuildId,
	string Word
) : IGuildChild
{
	public Keyword() : this(default, "") { }
}