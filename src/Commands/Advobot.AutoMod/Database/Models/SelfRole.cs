using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Database.Models;

public sealed record SelfRole(
	int GroupId,
	ulong GuildId,
	ulong RoleId
) : IGuildChild
{
	public const int NO_GROUP = 0;

	public SelfRole() : this(NO_GROUP, default, default) { }
}