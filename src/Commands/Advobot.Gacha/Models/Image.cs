using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.Models
{
	public record Image(
		long CharacterId,
		string Url
	) : ICharacterChild
	{
		public Image() : this(default, "")
		{
		}

		public Image(Character character) : this()
		{
			CharacterId = character.CharacterId;
		}
	}
}