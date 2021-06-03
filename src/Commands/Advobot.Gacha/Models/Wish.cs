using System;

using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utilities;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.Models
{
	public record Wish(
		long CharacterId,
		ulong GuildId,
		ulong UserId,
		long WishId
	) : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		public Wish() : this(default, default, default, WishId: TimeUtils.UtcNowTicks) { }

		public Wish(User user, Character character) : this()
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> WishId.ToTime();
	}
}