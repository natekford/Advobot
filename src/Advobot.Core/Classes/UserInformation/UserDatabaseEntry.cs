using Discord.WebSocket;
using System;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserDatabaseEntry : DatabaseEntry
	{
		/// <summary>
		/// The id of the guild the user is on.
		/// </summary>
		public ulong GuildId { get; set; }
		/// <summary>
		/// The id of the user.
		/// </summary>
		public ulong UserId { get; set; }

		protected UserDatabaseEntry() : base(default) { }
		public UserDatabaseEntry(SocketGuildUser user) : base(default)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
		public UserDatabaseEntry(TimeSpan time, SocketGuildUser user) : base(time)
		{
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}
	}
}
