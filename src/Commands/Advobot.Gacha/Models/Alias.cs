using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Models
{
	public class Alias : IReadOnlyAlias
	{
		public long CharacterId { get; private set; }
		public string Name { get; set; }
		public bool IsSpoiler { get; set; }

		public Alias() { }
		public Alias(Character character)
		{
			CharacterId = character.CharacterId;
		}
	}
}
