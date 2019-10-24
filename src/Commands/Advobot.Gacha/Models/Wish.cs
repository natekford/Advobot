﻿using System;

using Advobot.Databases.Relationships;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Wish : IReadOnlyWish
	{
		public long CharacterId { get; set; }
		public string GuildId { get; set; }
		public string UserId { get; set; }
		public long WishId { get; set; } = TimeUtils.UtcNowTicks;

		ulong IGuildChild.GuildId => ulong.Parse(GuildId);
		ulong IUserChild.UserId => ulong.Parse(UserId);

		public Wish()
		{
			GuildId = "";
			UserId = "";
		}

		public Wish(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId.ToString();
			UserId = user.UserId.ToString();
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> WishId.ToTime();
	}
}