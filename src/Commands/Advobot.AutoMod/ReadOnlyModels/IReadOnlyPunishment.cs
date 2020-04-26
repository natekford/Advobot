using System;

using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyPunishment : IGuildChild
	{
		int Instances { get; }
		TimeSpan? Length { get; }
		PunishmentType PunishmentType { get; }
		ulong? RoleId { get; }
	}
}