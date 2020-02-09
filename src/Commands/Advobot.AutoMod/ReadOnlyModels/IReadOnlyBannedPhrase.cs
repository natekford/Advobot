using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrase : IGuildChild
	{
		bool Contains { get; }
		string Phrase { get; }
		Punishment Punishment { get; }
	}
}