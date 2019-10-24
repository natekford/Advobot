﻿using System;

using Advobot.Databases.Relationships;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Claim : IReadOnlyClaim
	{
		public long CharacterId { get; set; }
		public long ClaimId { get; set; } = TimeUtils.UtcNowTicks;
		public string GuildId { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsPrimaryClaim { get; set; }
		public string UserId { get; set; }

		ulong IGuildChild.GuildId => ulong.Parse(GuildId);
		ulong IUserChild.UserId => ulong.Parse(UserId);

		public Claim()
		{
			GuildId = "";
			UserId = "";
		}

		public Claim(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId.ToString();
			UserId = user.UserId.ToString();
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> ClaimId.ToTime();
	}
}