using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyAlias : ICharacterChild
	{
		string? Name { get; }
		bool IsSpoiler { get; }
	}
}
