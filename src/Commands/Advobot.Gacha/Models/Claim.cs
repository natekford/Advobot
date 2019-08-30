using System;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Claim : IReadOnlyClaim
	{
		public long ClaimId { get; set; } = TimeUtils.UtcNowTicks;
		public string GuildId { get; set; }
		public string UserId { get; set; }
		public long CharacterId { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsPrimaryClaim { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.

		public Claim()
		{
		}

#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		public Claim(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}

		public DateTime GetTimeCreated()
			=> ClaimId.ToTime();
	}
}