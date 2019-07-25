using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyMarriage : ITimeCreated, IUserChild, ICharacterChild
	{
		IReadOnlyImage? Image { get; }
	}
}
