using System;

using Advobot.Levels.Database;
using Advobot.Levels.ReadOnlyModels;

using Discord;

namespace Advobot.Levels.Models
{
	public sealed class User : IReadOnlyUser
	{
		public ulong ChannelId { get; set; }
		public int Experience { get; set; }
		public ulong GuildId { get; set; }
		public int MessageCount { get; set; }
		public ulong UserId { get; set; }

		public User()
		{
		}

		public User(IGuildUser user, ITextChannel channel, int experience)
		{
			GuildId = channel.GuildId;
			ChannelId = channel.Id;
			UserId = user.Id;
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
			ChannelId = user.ChannelId;
			Experience = user.Experience;
			GuildId = user.GuildId;
			MessageCount = user.MessageCount;
			UserId = user.UserId;
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