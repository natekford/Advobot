using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Resizes and uses images.
	/// </summary>
	public interface IImageResizer
	{
		/// <summary>
		/// How many items are currently queued.
		/// </summary>
		int QueueCount { get; }

		/// <summary>
		/// All the items which are queued.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IImageArgs> GetQueuedArguments();
		/// <summary>
		/// Whether the guild is currently having an image worked on.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		bool IsGuildAlreadyProcessing(ulong guildId);
		/// <summary>
		/// Adds the arguments to the queue and then eventually gets to processing it.
		/// </summary>
		/// <param name="arguments"></param>
		void Enqueue(IImageArgs arguments);
	}
}