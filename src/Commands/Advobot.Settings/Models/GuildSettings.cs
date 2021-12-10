using Advobot.SQLite.Relationships;

namespace Advobot.Settings.Models;

public sealed record GuildSettings(
	string? Culture,
	ulong GuildId,
	ulong MuteRoleId,
	string? Prefix
) : IGuildChild
{
	public GuildSettings() : this(default, default, default, default) { }
}