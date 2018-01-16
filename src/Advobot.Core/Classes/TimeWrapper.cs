using Advobot.Core.Interfaces;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds a <see cref="DateTime"/> object and implements <see cref="ITime"/> so certain methods can restrict generics easier.
	/// </summary>
	public struct TimeWrapper : ITime
	{
		public DateTime Time { get; }

		public TimeWrapper(DateTime time)
		{
			Time = time.ToUniversalTime();
		}
	}
}