using System;
using System.Threading;

namespace Advobot.Interactivity
{
	/// <summary>
	/// Options for handling interactivity.
	/// </summary>
	public class InteractivityOptions
	{
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