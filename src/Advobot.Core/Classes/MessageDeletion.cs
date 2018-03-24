using System.Collections.Concurrent;
using System.Threading;
using Discord;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Handles deleted message collection.
	/// </summary>
	public sealed class MessageDeletion
	{
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
		/// <summary>
		/// The messages which have been deleted but not printed yet.
		/// </summary>
		public ConcurrentBag<IMessage> Messages => _Messages;

		private CancellationTokenSource _CancelTokenSource = new CancellationTokenSource();
		private ConcurrentBag<IMessage> _Messages = new ConcurrentBag<IMessage>();

		/// <summary>
		/// Clears any messages currently held.
		/// </summary>
		public void ClearBag()
		{
			Interlocked.Exchange(ref _Messages, new ConcurrentBag<IMessage>());
		}
	}
}