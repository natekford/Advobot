using Discord;
using System.Collections.Concurrent;
using System.Threading;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Handles deleted message collection for <see cref="Modules.Log.MyLogModule.OnMessageDeleted(Cacheable{IMessage, ulong}, ISocketMessageChannel)"/>.
	/// </summary>
	public sealed class MessageDeletion
	{
		private CancellationTokenSource _CancelTokenSource = new CancellationTokenSource();
		/// <summary>
		/// Accessing this cancel token cancels the old token and generates a new one.
		/// </summary>
		public CancellationToken CancelToken
		{
			get
			{
				_CancelTokenSource?.Cancel();
				_CancelTokenSource?.Dispose();
				return (_CancelTokenSource = new CancellationTokenSource()).Token;
			}
		}
		private ConcurrentBag<IMessage> _Messages = new ConcurrentBag<IMessage>();
		public ConcurrentBag<IMessage> Messages => _Messages;

		public void ClearBag()
		{
			Interlocked.Exchange(ref _Messages, new ConcurrentBag<IMessage>());
		}
	}
}