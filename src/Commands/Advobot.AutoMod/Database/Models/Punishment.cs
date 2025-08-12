using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Database.Models;

public record Punishment(
	ulong GuildId,
	int Instances,
	long? LengthTicks,
	PunishmentType PunishmentType,
	ulong RoleId
) : IGuildChild
{
	public TimeSpan? Length => LengthTicks is long temp ? new(temp) : null;

	public Punishment() : this(default, default, default, default, default) { }
}