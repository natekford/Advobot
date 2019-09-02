using Advobot.Gacha.ReadOnlyModels;

using Discord;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }

		public User()
		{
			GuildId = "";
			UserId = "";
		}

		public User(IGuildUser user)
		{
			GuildId = user.GuildId.ToString();
			UserId = user.Id.ToString();
		}
	}
}