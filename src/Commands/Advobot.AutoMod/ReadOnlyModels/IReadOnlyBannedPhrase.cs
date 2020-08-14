using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrase : IGuildChild
	{
		bool IsContains { get; }
		bool IsName { get; }
		bool IsRegex { get; }
		string Phrase { get; }
		PunishmentType PunishmentType { get; }

		bool IsMatch(string content);
	}
}