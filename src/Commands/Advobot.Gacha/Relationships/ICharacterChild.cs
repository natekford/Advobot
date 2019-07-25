using Advobot.Gacha.ReadOnlyModels;

namespace Advobot.Gacha.Relationships
{
	public interface ICharacterChild
	{
		int CharacterId { get; }

		IReadOnlyCharacter Character { get; }
	}
}
