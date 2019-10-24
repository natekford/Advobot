using Advobot.Databases.Relationships;
using Advobot.Gacha.Relationships;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyWish : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		long WishId { get; }
	}
}