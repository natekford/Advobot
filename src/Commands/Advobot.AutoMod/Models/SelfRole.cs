using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models;

public sealed record SelfRole(
	int GroupId,
	ulong GuildId,
	ulong RoleId
) : IGuildChild
{
	public SelfRole() : this(default, default, default) { }
}