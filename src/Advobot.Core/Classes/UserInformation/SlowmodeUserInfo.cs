using Discord;
using System;
using System.Threading;

namespace Advobot.Core.Classes.UserInformation
{
	/// <summary>
	/// Holds how many messages a user has left and when to reset them.
	/// </summary>
	public class SlowmodeUserInfo : UserInfo
	{
		private int _MessagesLeft;
		public int MessagesLeft => _MessagesLeft;

		public SlowmodeUserInfo(int interval, IGuildUser user, int baseMessages)
			: this(TimeSpan.FromSeconds(interval), user, baseMessages) { }
		public SlowmodeUserInfo(TimeSpan time, IGuildUser user, int baseMessages) : base(time, user)
		{
			_MessagesLeft = baseMessages;
		}

		public int DecrementMessages()
		{
			return Interlocked.Decrement(ref _MessagesLeft);
		}
	}
}
