using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyClaim : ITimeCreated, IUserChild, ICharacterChild
	{
		string? ImageUrl { get; }
	}
}
