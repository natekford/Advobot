using Advobot.Core.Interfaces;
using System;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds a <see cref="DateTime"/> object and implements <see cref="ITime"/> so certain methods can restrict generics easier.
	/// </summary>
	public struct BasicTimeInterface : IHasTime
	{
		private DateTime _Time;

		public BasicTimeInterface(DateTime time)
		{
			_Time = time.ToUniversalTime();
		}

		public DateTime GetTime() => _Time;
	}
}