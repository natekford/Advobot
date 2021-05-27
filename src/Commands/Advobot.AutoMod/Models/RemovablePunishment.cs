using System;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;

namespace Advobot.AutoMod.Models
{
	public class RemovablePunishment : IReadOnlyRemovablePunishment
	{
		public DateTime EndTime => new(EndTimeTicks);
		public long EndTimeTicks { get; set; }
		public ulong GuildId { get; set; }
		public PunishmentType PunishmentType { get; set; }
		public ulong RoleId { get; set; }
		public ulong UserId { get; set; }
	}
}