using System;

using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utilities;

namespace Advobot.Gacha.Models
{
	public class Wish : IReadOnlyWish
	{
		public long CharacterId { get; set; }
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }
		public long WishId { get; set; } = TimeUtils.UtcNowTicks;

		public Wish()
		{
		}

		public Wish(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> WishId.ToTime();
	}
}