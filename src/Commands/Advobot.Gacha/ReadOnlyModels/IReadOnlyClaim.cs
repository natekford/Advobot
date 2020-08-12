using Advobot.Gacha.Relationships;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyClaim : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		long ClaimId { get; }
		string? ImageUrl { get; }
		bool IsPrimaryClaim { get; }
	}
}