using System;

using Advobot.Punishments;
using Advobot.SQLite.Relationships;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyPunishment : IGuildChild
	{
		int Instances { get; }
		TimeSpan? Length { get; }
		PunishmentType PunishmentType { get; }
		ulong RoleId { get; }
	}
}