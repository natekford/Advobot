using System;

using Advobot.Databases.Relationships;
using Advobot.Levels.Database;
using Advobot.Levels.ReadOnlyModels;

using Discord;

namespace Advobot.Levels.Models
{
	public sealed class User : IReadOnlyUser
	{
		public string ChannelId { get; set; }
		public int Experience { get; set; }
		public string GuildId { get; set; }
		public int MessageCount { get; set; }
		public string UserId { get; set; }

		ulong IChannelChild.ChannelId => ulong.Parse(ChannelId);
		ulong IGuildChild.GuildId => ulong.Parse(GuildId);
		ulong IUserChild.UserId => ulong.Parse(UserId);

		public User()
		{
			GuildId = "";
			ChannelId = "";
			UserId = "";
		}

		public User(IGuildUser user, ITextChannel channel, int experience)
		{
			GuildId = channel.GuildId.ToString();
			ChannelId = channel.Id.ToString();
			UserId = user.Id.ToString();
			Experience = experience;
		}

		public User(ISearchArgs args)
		{
			GuildId = args.GuildId ?? throw new ArgumentNullException(nameof(args.GuildId));
			ChannelId = args.ChannelId ?? throw new ArgumentNullException(nameof(args.ChannelId));
			UserId = args.UserId ?? throw new ArgumentNullException(nameof(args.UserId));
		}

		private User(IReadOnlyUser user)
		{
			ChannelId = user.ChannelId.ToString();
			Experience = user.Experience;
			GuildId = user.GuildId.ToString();
			MessageCount = user.MessageCount;
			UserId = user.UserId.ToString();
		}

		public IReadOnlyUser AddXp(int xp)
		{
			var newUser = new User(this);
			newUser.Experience += xp;
			++newUser.MessageCount;
			return newUser;
		}

		public IReadOnlyUser RemoveXp(int xp)
		{
			var newUser = new User(this);
			newUser.Experience -= xp;
			--newUser.MessageCount;

			if (newUser.Experience < 0)
			{
				newUser.Experience = 0;
			}
			if (newUser.MessageCount < 0)
			{
				newUser.MessageCount = 0;
			}
			return newUser;
		}
	}
}