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
		public bool IsPrimaryMarriage { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();

		public Claim() { }
		public Claim(User user, Character character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}
	}
}
