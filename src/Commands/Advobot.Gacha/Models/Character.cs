using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;
using System;

namespace Advobot.Gacha.Models
{
	public class Character : IReadOnlyCharacter
	{
		public long SourceId { get; set; }
		public long CharacterId { get; set; } = TimeUtils.UtcNowTicks;
		public string? Name { get; set; }
		public string? GenderIcon { get; set; }
		public Gender Gender { get; set; }
		public RollType RollType { get; set; }
		public string? FlavorText { get; set; }
		public bool IsFakeCharacter { get; set; }

		public Character() { }
		public Character(IReadOnlySource source)
		{
			SourceId = source.SourceId;
		}

		public DateTime GetTimeCreated()
			=> CharacterId.ToTime();
	}
}
