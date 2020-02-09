using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.Models
{
	public interface IReadOnlySpamPrevention : IGuildChild
	{
		bool Enabled { get; }
		int Instances { get; }
		Punishment Punishment { get; }
		int Size { get; }
	}

	public sealed class SpamPrevention
	{
	}
}