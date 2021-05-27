using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Discord;

namespace Advobot.Logging.Caches
{
	/// <summary>
	/// Handles deleted message collection.
	/// </summary>
	public sealed class DeletedMessageCache : IDisposable
	{
		private CancellationTokenSource _CancelTokenSource = new();

		private ConcurrentBag<IMessage> _Messages = new();

		/// <summary>
		/// The amount of messages currently cached.
		/// </summary>
		public int Count => _Messages.Count;

		/// <summary>
		/// Adds the message to the cache.
		/// </summary>
		/// <param name="message"></param>
		public void Add(IMessage message)
			=> _Messages.Add(message);

		/// <inheritdoc />
		public void Dispose()
			=> _CancelTokenSource?.Dispose();

		/// <summary>
		/// Removes all of the cached messages from the cache and returns them.
		/// </summary>
		public IReadOnlyCollection<IMessage> Empty()
		{
			return Interlocked.Exchange(ref _Messages, new ConcurrentBag<IMessage>())
				.OrderBy(x => x.Id)
				.ToArray();
		}

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
	}
}