﻿using System;

namespace Advobot.Classes
{
	/// <summary>
	/// Provides information about what log counter to increment.
	/// </summary>
	public sealed class LogCounterIncrementEventArgs : EventArgs
	{
		/// <summary>
		/// The name of the log count to increment.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The amount to increment. Can be negative, in which case this would be a decrement.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// Creates an instance of <see cref="LogCounterIncrementEventArgs"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="count"></param>
		public LogCounterIncrementEventArgs(string name, int count)
		{
			Name = name;
			Count = count;
		}
	}
}