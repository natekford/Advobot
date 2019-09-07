using System;

using Advobot.Services.Time;

using Discord;

namespace Advobot.Services.GuildSettings.UserInformation
{
	/// <summary>
	/// Holds a user and a time.
	/// </summary>
	public abstract class UserInfo
	{
		/// <summary>
		/// The id of the guild the user is on.
		/// </summary>
		public ulong GuildId { get; }

		/// <summary>
		/// The time to reset the user.
		/// </summary>
		public DateTimeOffset Time { get; protected set; }

		/// <inheritdoc />
		public ulong UserId { get; }

		/// <summary>
		/// Creates an instance of userinfo with the supplied user and time as <see cref="DateTimeOffset.UtcNow"/>.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="user"></param>
		protected UserInfo(ITime time, IGuildUser user) : this(time, user, TimeSpan.Zero) { }

		/// <summary>
		/// Creates an instance of userinfo with the supplied user and time.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="user"></param>
		/// <param name="span"></param>
		protected UserInfo(ITime time, IGuildUser user, TimeSpan span)
		{
			Time = time.UtcNow.Add(span);
			GuildId = user.Guild.Id;
			UserId = user.Id;
		}

		/// <summary>
		/// Sets everything back to default values.
		/// </summary>
		public abstract void Reset();
	}
}