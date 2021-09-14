
using Advobot.Interactivity.Criterions;

using Discord;

namespace Advobot.Interactivity
{
	/// <summary>
	/// Options for handling interactivity.
	/// </summary>
	public class InteractivityOptions
	{
		/// <summary>
		/// Criteria for determing the message can be used.
		/// </summary>
		public IEnumerable<ICriterion<IMessage>>? Criteria { get; set; }

		/// <summary>
		/// The timeout before the interactivity should be stopped.
		/// </summary>
		public TimeSpan? Timeout { get; set; }

		/// <summary>
		/// A token for cancelling the interactivity.
		/// </summary>
		public CancellationToken? Token { get; set; }
	}
}