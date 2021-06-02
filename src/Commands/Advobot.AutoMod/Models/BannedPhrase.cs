using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models
{
	public record BannedPhrase(
		ulong GuildId,
		bool IsContains,
		bool IsName,
		bool IsRegex,
		string Phrase,
		PunishmentType PunishmentType
	) : IGuildChild
	{
		public BannedPhrase() : this(default, default, default, default, "", default) { }
	}
}