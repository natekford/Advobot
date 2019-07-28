using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyClaim : ITimeCreated, IUserChild, ICharacterChild
	{
		long ClaimId { get; }
		string? ImageUrl { get; }
		bool IsPrimaryClaim { get; }
	}
}
