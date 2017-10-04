using Advobot.Actions;
using Discord;

namespace Advobot.Services.Timers
{
	public class DictKey
	{
		public readonly long Ticks;

		public DictKey(long ticks)
		{
			Ticks = ticks;
		}
	}

	public class UserKey : DictKey
	{
		public readonly ulong GuildId;
		public readonly ulong UserId;

		public UserKey(IGuildUser user, long ticks) : base(ticks)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}

		public override string ToString()
		{
			return $"{GuildId}:{UserId}:{Ticks}";
		}
	}

	public class ChannelKey : DictKey
	{
		public readonly ulong GuildId;
		public readonly ulong ChannelId;

		public ChannelKey(IChannel channel, long ticks) : base(ticks)
		{
			GuildId = channel.GetGuild().Id;
			ChannelId = channel.Id;
		}

		public override string ToString()
		{
			return $"{GuildId}:{ChannelId}:{Ticks}";
		}
	}
}
