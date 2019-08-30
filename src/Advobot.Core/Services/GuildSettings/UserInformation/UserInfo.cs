using System;

using Discord;

namespace Advobot.Services.GuildSettings.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo
	{
		/// <summary>
		/// Creates an instance of userinfo with the supplied user and time as datetime.utcnow.
		/// </summary>
		/// <param name="user"></param>
		protected UserInfo(IGuildUser user) : this(TimeSpan.Zero, user) { }

		/// <summary>
		/// Creates an instance of userinfo with the supplied user and time.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="user"></param>
		protected UserInfo(TimeSpan time, IGuildUser user)
		{
			Time = DateTime.UtcNow.Add(time);
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}

		/// <summary>
		/// The id of the guild the user is on.
		/// </summary>
		public ulong GuildId { get; }

		/// <summary>
		/// The time to reset the user.
		/// </summary>
		public DateTime Time { get; protected set; }

		/// <inheritdoc />
		public ulong UserId { get; }

		/// <summary>
		/// Sets everything back to default values.
		/// </summary>
		public abstract void Reset();
	}
}