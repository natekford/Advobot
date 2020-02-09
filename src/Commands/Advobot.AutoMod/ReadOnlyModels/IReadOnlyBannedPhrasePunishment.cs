using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyBannedPhrasePunishment : IGuildChild
	{
		int Instances { get; }
		Punishment Punishment { get; }
		ulong? RoleId { get; }
		int? Time { get; }
	}
}