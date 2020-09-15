using Advobot.SQLite.Relationships;

namespace Advobot.Settings.ReadOnlyModels
{
	public interface IReadOnlyGuildSettings : IGuildChild
	{
		string? Culture { get; }
		ulong MuteRoleId { get; }
		string? Prefix { get; }
	}
}