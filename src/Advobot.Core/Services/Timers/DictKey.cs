using System;
using Advobot.Core.Classes.UserInformation;
using Discord;

namespace Advobot.Core.Services.Timers
{
	internal class DictKey
	{
		public ulong GuildId { get; }
		public ulong ObjectId { get; }
		public long Ticks { get; }

		public DictKey(UserInfo info) : this(info.User, info.Time) { }
		public DictKey(IGuildUser user, DateTime time) : this(user.GuildId, user.Id, time.Ticks) { }
		public DictKey(IGuildChannel channel, DateTime time) : this(channel.GuildId, channel.Id, time.Ticks) { }
		public DictKey(IRole role, DateTime time) : this(role.Guild.Id, role.Id, time.Ticks) { }
		public DictKey(IGuild guild, ISnowflakeEntity obj, DateTime time) : this(guild.Id, obj.Id, time.Ticks) { }
		private DictKey(ulong guildId, ulong objectId, long ticks)
		{
			GuildId = guildId;
			ObjectId = objectId;
			Ticks = ticks;
		}

		public override string ToString()
		{
			return $"{GuildId}:{ObjectId}:{Ticks}";
		}
	}
}
