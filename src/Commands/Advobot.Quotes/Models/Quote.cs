using Advobot.SQLite.Relationships;

namespace Advobot.Quotes.Models
{
	public sealed record Quote(
		string Description,
		ulong GuildId,
		string Name
	) : IGuildChild
	{
		public Quote() : this("", default, "") { }
	}
}