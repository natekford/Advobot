using System;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Character : IReadOnlyCharacter
	{
		public long CharacterId { get; set; } = TimeUtils.UtcNowTicks;
		public string? FlavorText { get; set; }
		public Gender Gender { get; set; }
		public string? GenderIcon { get; set; }
		public bool IsFakeCharacter { get; set; }
		public string Name { get; set; }
		public RollType RollType { get; set; }
		public long SourceId { get; set; }

		public Character()
		{
			Name = "";
		}

		public Character(IReadOnlySource source) : this()
		{
			SourceId = source.SourceId;
		}

		public DateTimeOffset GetTimeCreated()
			=> CharacterId.ToTime();
	}
}