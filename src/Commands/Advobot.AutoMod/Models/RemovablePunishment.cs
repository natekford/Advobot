
using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.Models
{
	public record RemovablePunishment(
		long EndTimeTicks,
		ulong GuildId,
		PunishmentType PunishmentType,
		ulong RoleId,
		ulong UserId
	) : IGuildChild, IUserChild
	{
		public DateTime EndTime => new(EndTimeTicks);

		public RemovablePunishment() : this(default, default, default, default, default) { }
	}
}