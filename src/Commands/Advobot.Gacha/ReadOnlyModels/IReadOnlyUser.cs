using System.Collections.Generic;

namespace Advobot.Gacha.ReadOnlyModels
{
	public interface IReadOnlyUser
	{
		ulong GuildId { get; }
		ulong UserId { get; }
		IReadOnlyList<IReadOnlyMarriage> Marriages { get; }
		IReadOnlyList<IReadOnlyWish> Wishlist { get; }
	}
}
