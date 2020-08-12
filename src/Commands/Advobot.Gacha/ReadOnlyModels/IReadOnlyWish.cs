using Advobot.Gacha.Relationships;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyWish : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		long WishId { get; }
	}
}