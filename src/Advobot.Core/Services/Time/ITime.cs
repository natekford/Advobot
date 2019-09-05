using System;
using System.Collections.Generic;
using System.Text;

namespace Advobot.Services.Time
{
	/// <summary>
	/// Abstraction for time.
	/// </summary>
	public interface ITime
	{
		/// <summary>
		/// The current time.
		/// </summary>
		DateTimeOffset UtcNow { get; }
	}
}