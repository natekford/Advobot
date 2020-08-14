using System;

using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyRemovablePunishment : IGuildChild, IUserChild
	{
		public DateTime EndTime { get; }
		public PunishmentType PunishmentType { get; }
		public ulong RoleId { get; }
	}
}