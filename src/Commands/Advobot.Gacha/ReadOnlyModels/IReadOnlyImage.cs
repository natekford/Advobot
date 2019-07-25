using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyImage : ICharacterChild
	{
		string Url { get; }
	}
}
