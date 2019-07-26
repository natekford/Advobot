using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;

namespace Advobot.Gacha.Models
{
	public class Character : IReadOnlyCharacter
	{
		public long SourceId { get; set; }
		public long CharacterId { get; set; }
		public string? Name { get; set; }
		public string? GenderIcon { get; set; }
		public Gender Gender { get; set; }
		public RollType RollType { get; set; }
		public string? FlavorText { get; set; }
		public bool IsFakeCharacter { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();

		public Character() { }
		public Character(IReadOnlySource source)
		{
			SourceId = source.SourceId;
		}
	}
}
