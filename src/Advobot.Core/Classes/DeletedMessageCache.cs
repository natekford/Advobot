using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Discord;
using Discord.WebSocket;

namespace Advobot.Classes
{
	/// <summary>
	/// Handles deleted message collection.
	/// </summary>
	public sealed class DeletedMessageCache : IDisposable
	{
		/// <summary>
		/// The amount of messages currently cached.
		/// </summary>
		public int Count => _Messages.Count;

		private CancellationTokenSource _CancelTokenSource = new CancellationTokenSource();
		private ConcurrentBag<IMessage> _Messages = new ConcurrentBag<IMessage>();

		/// <summary>
		/// Cancels the previous cancellation token and generates a new one from a new source.
		/// </summary>
		/// <returns></returns>
		public CancellationToken GetNewCancellationToken()
		{
			_CancelTokenSource?.Cancel();
			_CancelTokenSource?.Dispose();
			return (_CancelTokenSource = new CancellationTokenSource()).Token;
		}
		/// <summary>
		/// Removes all of the cached messages from the cache and returns them.
		/// </summary>
		public IReadOnlyCollection<IMessage> Empty()
			=> Interlocked.Exchange(ref _Messages, new ConcurrentBag<IMessage>()).ToArray();
		/// <summary>
		/// Adds the message to the cache.
		/// </summary>
		/// <param name="message"></param>
		public void Add(IMessage message)
			=> _Messages.Add(message);
		/// <inheritdoc />
		public void Dispose()
			=> _CancelTokenSource?.Dispose();
	}
}