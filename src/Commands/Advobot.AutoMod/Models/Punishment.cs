using System;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

namespace Advobot.AutoMod.Models
{
	public class Punishment : IReadOnlyPunishment
	{
		public string GuildId { get; set; } = "";
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
		public string? RoleId { get; set; }

		ulong IGuildChild.GuildId => GuildId.ToId();
		ulong IReadOnlyPunishment.RoleId => RoleId.ToId();
	}
}