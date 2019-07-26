using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;

namespace Advobot.Gacha.Models
{
	public class Claim : IReadOnlyClaim
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }
		public long CharacterId { get; set; }
		public string? ImageUrl { get; set; }
		public bool IsPrimaryClaim { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public Claim() { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		public Claim(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}
	}
}
