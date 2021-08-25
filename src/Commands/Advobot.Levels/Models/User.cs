
using Advobot.Levels.Database;
using Advobot.SQLite.Relationships;

namespace Advobot.Levels.Models
{
	public sealed record User(
		ulong ChannelId,
		int Experience,
		ulong GuildId,
		int MessageCount,
		ulong UserId
	) : IChannelChild, IUserChild
	{
		public User() : this(default, default, default, default, default) { }

		public User(SearchArgs args) : this()
		{
			GuildId = args.GuildId ?? throw new ArgumentException("null guild id.", nameof(args));
			ChannelId = args.ChannelId ?? throw new ArgumentException("null channel id.", nameof(args));
			UserId = args.UserId ?? throw new ArgumentException("null user id.", nameof(args));
		}
	}
}