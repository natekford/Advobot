using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrase : IGuildChild
	{
		bool Contains { get; }
		bool IsRegex { get; }
		string Phrase { get; }
		PunishmentType PunishmentType { get; }

		bool IsMatch(string content);
	}
}