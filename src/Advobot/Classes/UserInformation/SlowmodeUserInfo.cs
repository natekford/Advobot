using Discord.WebSocket;
using System;
using System.Threading;

namespace Advobot.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserInfo
	{
		/// <summary>
		/// The amount of messages the user has currently sent.
		/// </summary>
		public int MessagesSent => _MessagesSent;

		private int _MessagesSent;

		/// <summary>
		/// Creates an instance of slowmodeuserinfo.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="user"></param>
		public SlowmodeUserInfo(TimeSpan time, SocketGuildUser user) : base(time, user) { }

		/// <summary>
		/// Increments messages sent.
		/// </summary>
		/// <returns></returns>
		public int Increment()
		{
			return Interlocked.Increment(ref _MessagesSent);
		}
		/// <summary>
		/// Sets the time to utcnow + time.
		/// </summary>
		/// <param name="time"></param>
		public void UpdateTime(TimeSpan time)
		{
			Time = DateTime.UtcNow.Add(time);
		}
		/// <inheritdoc />
		public override void Reset()
		{
			Interlocked.Exchange(ref _MessagesSent, 0);
		}
	}
}
