using Advobot.Core.Utilities;
using Advobot.Core.Classes.UserInformation;
using Discord;

namespace Advobot.Core.Services.Timers
{
	internal class DictKey
	{
		public readonly long Ticks;

		public DictKey(long ticks)
		{
			Ticks = ticks;
		}
	}

	internal sealed class UserKey : DictKey
	{
		public readonly ulong GuildId;
		public readonly ulong UserId;

		public UserKey(IGuildUser user, long ticks) : base(ticks)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
		public UserKey(IGuild guild, IUser user, long ticks) : base(ticks)
		{
			GuildId = guild.Id;
			UserId = user.Id;
		}
		public UserKey(UserInfo info) : this(info.User, info.GetTime().Ticks) { }

		public override string ToString() => $"{GuildId}:{UserId}:{Ticks}";
	}

	internal sealed class ChannelKey : DictKey
	{
		public readonly ulong GuildId;
		public readonly ulong ChannelId;

		public ChannelKey(IChannel channel, long ticks) : base(ticks)
		{
			GuildId = channel.GetGuild().Id;
			ChannelId = channel.Id;
		}

		public override string ToString() => $"{GuildId}:{ChannelId}:{Ticks}";
	}
}
