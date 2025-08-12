using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Database.Models;

public record PersistentRole(
	ulong GuildId,
	ulong RoleId,
	ulong UserId
) : IGuildChild, IUserChild
{
	public PersistentRole() : this(default, default, default) { }
}