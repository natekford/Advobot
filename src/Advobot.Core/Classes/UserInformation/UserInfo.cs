using Advobot.Core.Interfaces;
using Discord.WebSocket;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo : DatabaseEntry
	{
		public ulong GuildId { get; set; }
		public ulong UserId { get; set; }

		protected UserInfo() : base(default) { }
		public UserInfo(SocketGuildUser user) : base(default)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
		public UserInfo(TimeSpan time, SocketGuildUser user) : base(time)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
	}
}
