using System;

namespace Advobot.Services.Time
{
	/// <summary>
	/// Implementation for the default <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public sealed class DefaultTime : ITime
	{
		/// <summary>
		/// Returns <see cref="DateTime.UtcNow"/>.
		/// </summary>
		public DateTime UtcNow => DateTime.UtcNow;
	}
}