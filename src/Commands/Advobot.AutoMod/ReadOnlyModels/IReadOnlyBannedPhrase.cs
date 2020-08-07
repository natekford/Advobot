using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrase : IGuildChild
	{
		string Phrase { get; }
		PunishmentType PunishmentType { get; }

		bool IsMatch(string content);
	}
}