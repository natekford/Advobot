using System;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;
using Advobot.TypeReaders;

using Discord.Commands;

namespace Advobot.AutoMod.Models
{
	[NamedArgumentType]
	public sealed class TimedPunishmentArgs
	{
		[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
		public int? Instances { get; set; }
		public TimeSpan? Interval { get; set; }
		public TimeSpan? Length { get; set; }
		public PunishmentType? PunishmentType { get; set; }
		public ulong? RoleId { get; set; }
		[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
		public int? Size { get; set; }

		public T Create<T>(IReadOnlyTimedPrevention? original) where T : TimedPrevention, new()
		{
			original ??= new T();

			return new T
			{
				GuildId = original.GuildId,
				Enabled = original.Enabled,
				Instances = Instances ?? original.Instances,
				IntervalTicks = (Interval ?? original.Interval).Ticks,
				LengthTicks = (Length ?? original.Length)?.Ticks,
				PunishmentType = PunishmentType ?? original.PunishmentType,
				RoleId = RoleId ?? original.RoleId,
				Size = Size ?? original.Size,
			};
		}
	}
}