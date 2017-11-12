using Discord;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Handles deleted message collection for <see cref="Modules.Log.MyLogModule.OnMessageDeleted(Cacheable{IMessage, ulong}, ISocketMessageChannel)"/>.
	/// </summary>
	public class MessageDeletion
	{
		private CancellationTokenSource _CancelToken;
		/// <summary>
		/// Accessing this cancel token cancels the old token and generates a new one.
		/// </summary>
		public CancellationTokenSource CancelToken
		{
			get
			{
				_CancelToken?.Cancel();
				return _CancelToken = new CancellationTokenSource();
			}
		}
		private ConcurrentBag<IMessage> _Messages = new ConcurrentBag<IMessage>();
		public ConcurrentBag<IMessage> Messages => _Messages;

		public void ClearBag() => Interlocked.Exchange(ref _Messages, new ConcurrentBag<IMessage>());
	}
}