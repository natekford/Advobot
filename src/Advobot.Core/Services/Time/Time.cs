using System;

namespace Advobot.Services.Time
{
	/// <summary>
	/// Implementation for the default <see cref="DateTimeOffset.UtcNow"/>.
	/// </summary>
	public sealed class DefaultTime : ITime
	{
		/// <summary>
		/// Returns <see cref="DateTimeOffset.UtcNow"/>.
		/// </summary>
		public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
	}
}