using Advobot.Gacha.ReadOnlyModels;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }
		public IList<Marriage> Marriages { get; set; } = new List<Marriage>();
		public IList<Wish> Wishlist { get; set; } = new List<Wish>();

		IReadOnlyList<IReadOnlyMarriage> IReadOnlyUser.Marriages => (IReadOnlyList<IReadOnlyMarriage>)Marriages;
		IReadOnlyList<IReadOnlyWish> IReadOnlyUser.Wishlist => (IReadOnlyList<IReadOnlyWish>)Wishlist;
	}
}
