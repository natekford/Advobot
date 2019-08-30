using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyAlias : ICharacterChild
	{
		bool IsSpoiler { get; }
		string? Name { get; }
	}
}