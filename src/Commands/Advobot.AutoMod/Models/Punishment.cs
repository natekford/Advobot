using System;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;

namespace Advobot.AutoMod.Models
{
	public class Punishment : IReadOnlyPunishment
	{
		public ulong GuildId { get; set; }
		public int Instances { get; set; }
		public TimeSpan? Length
		{
			get
			{
				if (LengthTicks.HasValue)
				{
					return new TimeSpan(LengthTicks.Value);
				}
				return null;
			}
		}
		public long? LengthTicks { get; set; }
		public PunishmentType PunishmentType { get; set; }
		public ulong RoleId { get; set; }

		public Punishment()
		{
		}

		public Punishment(IReadOnlyPunishment other)
		{
			GuildId = other.GuildId;
			Instances = other.Instances;
			LengthTicks = other.Length?.Ticks;
			PunishmentType = other.PunishmentType;
			RoleId = other.RoleId;
		}
	}
}