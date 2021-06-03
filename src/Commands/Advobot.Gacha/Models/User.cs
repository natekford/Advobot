using Advobot.SQLite.Relationships;

using Discord;

namespace Advobot.Gacha.Models
{
	public record User(
		ulong GuildId,
		ulong UserId
	) : IGuildChild, IUserChild
	{
		public User() : this(default, default)
		{
		}

		public User(IGuildUser user) : this()
		{
			GuildId = user.GuildId;
			UserId = user.Id;
		}
	}
}