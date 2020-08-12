using Advobot.Gacha.ReadOnlyModels;

using Discord;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }

		public User()
		{
		}

		public User(IGuildUser user)
		{
			GuildId = user.GuildId;
			UserId = user.Id;
		}
	}
}