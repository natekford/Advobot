using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;

namespace Advobot.Gacha.Models
{
	public class Wish : IReadOnlyWish
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }
		public long CharacterId { get; set; }
		public long TimeCreated { get; set; } = TimeUtils.Now();

		public Wish() { }
		public Wish(User user, Character character)
		{
			GuildId = user.GuildId;
			UserId = user.UserId;
			CharacterId = character.CharacterId;
		}
	}
}
