using System;
using System.Collections.Generic;

namespace Advobot.Gacha.Models
{
	public class Character
	{
		public int CharacterId { get; set; }

		public string Name { get; set; } = "";
		public string GenderIcon { get; set; } = "";
		public Gender Gender { get; set; }
		public RollType RollType { get; set; }
		public int Claims { get; set; }
		public int Likes { get; set; }
		public string? FlavorText { get; set; }

		public IList<Image> Images { get; set; } = Array.Empty<Image>();
		public IList<Marriage> Marriages { get; set; } = Array.Empty<Marriage>();
		public IList<Wish> Wishlist { get; set; } = Array.Empty<Wish>();
		public IList<Alias> Aliases { get; set; } = Array.Empty<Alias>();

		public Source Source { get; set; } = new Source();
	}
}
