using Advobot.Databases.Relationships;
using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyClaim : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		long ClaimId { get; }
		string? ImageUrl { get; }
		bool IsPrimaryClaim { get; }
	}
}