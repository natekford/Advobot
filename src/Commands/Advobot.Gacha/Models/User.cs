using Advobot.Gacha.ReadOnlyModels;

using Discord;

namespace Advobot.Gacha.Models
{
	public class User : IReadOnlyUser
	{
		public string GuildId { get; set; }
		public string UserId { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized.

		public User()
		{
		}

#pragma warning restore CS8618 // Non-nullable field is uninitialized.

		public User(IGuildUser user)
		{
			GuildId = user.GuildId.ToString();
			UserId = user.Id.ToString();
		}
	}
}