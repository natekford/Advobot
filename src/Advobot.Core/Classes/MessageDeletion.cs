using System;
using System.Collections.Concurrent;
using System.Threading;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles deleted message collection.
	/// </summary>
	public sealed class MessageDeletion : IDisposable
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
		public ConcurrentBag<SocketMessage> Messages => _Messages;

		private CancellationTokenSource _CancelTokenSource = new CancellationTokenSource();
		private ConcurrentBag<SocketMessage> _Messages = new ConcurrentBag<SocketMessage>();

		/// <summary>
		/// Clears any messages currently held.
		/// </summary>
		public void ClearBag()
			=> Interlocked.Exchange(ref _Messages, new ConcurrentBag<SocketMessage>());
		/// <inheritdoc />
		public void Dispose()
			=> _CancelTokenSource?.Dispose();
	}
}