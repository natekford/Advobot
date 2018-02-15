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
		private int _MessagesLeft;
		public int MessagesLeft
		{
			get => _MessagesLeft;
			private set => _MessagesLeft = value;
		}

		public SlowmodeUserInfo(TimeSpan time, SocketGuildUser user, int baseMessages) : base(time, user)
		{
			_MessagesLeft = baseMessages;
		}

		public int DecrementMessages()
		{
			return Interlocked.Decrement(ref _MessagesLeft);
		}
	}
}
