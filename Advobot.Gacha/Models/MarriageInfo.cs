using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Gacha.Models
{
	public class DatabaseMetaInfo
	{
		public int SchemaVersion { get; set; }
	}

	public class UserInfo
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }

		public int PrimaryCharacterId { get; set; }
	}

	public class MarriageInfo
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }
		public int CharacterId { get; set; }

		public string? ImageUrl { get; set; }
	}

	public class WishList
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }
		public int CharacterId { get; set; }
	}

	public class CharacterGlobalInfo
	{
		public int CharacterId { get; set; }

		public string Name { get; set; } = "";
		public string Source { get; set; } = "";
		public string GenderIcon { get; set; } = "";
		public RollType RollType { get; set; }
		public int Claims { get; set; }
		public int Likes { get; set; }
		public string? FlavorText { get; set; }
	}

	public class CharacterImageInfo
	{
		public int CharacterId { get; set; }

		public string ImageUrl { get; set; } = "";
	}

	public enum RollType : ulong
	{
		Game = (1U << 0),
		Manga = (1U << 1),
		Anime = (1U << 2),
	}
}
