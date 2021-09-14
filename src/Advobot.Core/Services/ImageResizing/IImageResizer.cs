namespace Advobot.Services.ImageResizing
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
		/// Adds the arguments to the queue and then eventually gets to processing it.
		/// </summary>
		/// <param name="arguments"></param>
		void Enqueue(IImageContext arguments);

		/// <summary>
		/// All the items which are queued.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IImageContext> GetQueuedArguments();

		/// <summary>
		/// Whether the guild is currently having an image worked on.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		bool IsGuildAlreadyProcessing(ulong guildId);
	}
}