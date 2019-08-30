using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Models
{
	public class Alias : IReadOnlyAlias
	{
		public Alias()
		{
		}

		public Alias(IReadOnlyCharacter character)
		{
			CharacterId = character.CharacterId;
		}

		public long CharacterId { get; set; }
		public bool IsSpoiler { get; set; }
		public string? Name { get; set; }
	}
}