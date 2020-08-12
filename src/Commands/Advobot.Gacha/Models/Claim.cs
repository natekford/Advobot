using System;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Claim : IReadOnlyClaim
	{
		public long CharacterId { get; set; }
		public long ClaimId { get; set; } = TimeUtils.UtcNowTicks;
		public ulong GuildId { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsPrimaryClaim { get; set; }
		public ulong UserId { get; set; }

		public Claim()
		{
		}

		public Claim(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> ClaimId.ToTime();
	}
}