
using Advobot.Gacha.Relationships;
using Advobot.Gacha.Utilities;
using Advobot.SQLite.Relationships;

namespace Advobot.Gacha.Models
{
	public record Claim(
		long CharacterId,
		long ClaimId,
		ulong GuildId,
		string? ImageUrl,
		bool IsPrimaryClaim,
		ulong UserId
	) : ITimeCreated, IGuildChild, IUserChild, ICharacterChild
	{
		public Claim() : this(default, ClaimId: TimeUtils.UtcNowTicks, default, default, default, default)
		{
		}

		public Claim(User user, Character character) : this()
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}

		public DateTimeOffset GetTimeCreated()
			=> ClaimId.ToTime();
	}
}