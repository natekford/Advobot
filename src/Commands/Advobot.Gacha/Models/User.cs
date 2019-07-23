using System;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class User
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }

		public int PrimaryCharacterId { get; set; }

		public IList<Marriage> Marriages { get; set; } = Array.Empty<Marriage>();
		public IList<Wish> Wishlist { get; set; } = Array.Empty<Wish>();
	}
}
