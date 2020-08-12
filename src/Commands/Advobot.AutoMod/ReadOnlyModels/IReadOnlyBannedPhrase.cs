using Advobot.Services.GuildSettings.Settings;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrase : IGuildChild
	{
		string Phrase { get; }
		PunishmentType PunishmentType { get; }

		bool IsMatch(string content);
	}
}