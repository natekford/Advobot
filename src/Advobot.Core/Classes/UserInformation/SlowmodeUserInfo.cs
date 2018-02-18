using Discord.WebSocket;
using System;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserInfo
	{
		private int _MessagesSent;

		/// <summary>
		/// The amount of messages the user has currently sent.
		/// </summary>
		public int MessagesSent => _MessagesSent;

		public SlowmodeUserInfo(TimeSpan time, SocketGuildUser user) : base(time, user) { }

		public int Increment()
		{
			return Interlocked.Increment(ref _MessagesSent);
		}
		public void UpdateTime(TimeSpan time)
		{
			Time = DateTime.UtcNow.Add(time);
		}
		public override void Reset()
		{
			Interlocked.Exchange(ref _MessagesSent, 0);
		}
	}
}
