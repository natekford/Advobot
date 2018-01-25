using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Services.Timers
{
	/// <summary>
	/// Wrapper for a concurrent dictionary inside a concurrent dictionary.
	/// </summary>
	/// <typeparam name="TFirstKey">The first key type.</typeparam>
	/// <typeparam name="TSecondKey">The second key type, has to implement <see cref="ITime"/>.</typeparam>
	/// <typeparam name="TValue">The value to store.</typeparam>
	internal sealed class ConcurrentDoubleKeyDictionary<TFirstKey, TSecondKey, TValue> where TValue : ITime
	{
		private ConcurrentDictionary<TFirstKey, ConcurrentDictionary<TSecondKey, TValue>> _Values = new ConcurrentDictionary<TFirstKey, ConcurrentDictionary<TSecondKey, TValue>>();

		/// <summary>
		/// Removes every value held.
		/// </summary>
		public void Clear()
		{
			_Values.Clear();
		}
		/// <summary>
		/// Removes every value held by the key.
		/// </summary>
		/// <param name="firstKey">The key to remove all values from.</param>
		public void Clear(TFirstKey firstKey)
		{
			if (_Values.TryGetValue(firstKey, out var value))
			{
				value.Clear();
			}
		}
		/// <summary>
		/// Sets the value of the keys to <paramref name="value"/>.
		/// </summary>
		/// <param name="firstKey">The first key.</param>
		/// <param name="secondKey">The second key.</param>
		/// <param name="value">The value to set.</param>
		public void AddOrUpdate(TFirstKey firstKey, TSecondKey secondKey, TValue value)
		{
			_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).AddOrUpdate(secondKey, value, (k, v) => value);
		}
		/// <summary>
		/// Attempst to get the object at the keys. Returns false if there is nothing with the keys.
		/// </summary>
		/// <param name="firstKey">The first key.</param>
		/// <param name="secondKey">The second key.</param>
		/// <param name="value">The value gotten.</param>
		/// <returns>A boolean indicating whether or not the object exists.</returns>
		public bool TryGetValue(TFirstKey firstKey, TSecondKey secondKey, out TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryGetValue(secondKey, out value);
		}
		/// <summary>
		/// Attempts to add the object at the keys. Returns false If there is already something with the keys.
		/// </summary>
		/// <param name="firstKey">The first key.</param>
		/// <param name="secondKey">The second key.</param>
		/// <param name="value">The value to add.</param>
		/// <returns>A boolean indicating whether or not the object was added.</returns>
		public bool TryAdd(TFirstKey firstKey, TSecondKey secondKey, TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryAdd(secondKey, value);
		}
		/// <summary>
		/// Attempts to remove the object at the keys. Returns false if there is nothing with the keys.
		/// </summary>
		/// <param name="firstKey">The first key.</param>
		/// <param name="secondKey">The second key.</param>
		/// <param name="value">The value removed.</param>
		/// <returns>A boolean indicating whether or not the object was removed.</returns>
		public bool TryRemove(TFirstKey firstKey, TSecondKey secondKey, out TValue value)
		{
			return !_Values.GetOrAdd(firstKey, new ConcurrentDictionary<TSecondKey, TValue>()).TryRemove(secondKey, out value);
		}
		/// <summary>
		/// Returns the values of the first key.
		/// </summary>
		/// <param name="firstKey">The key to search for.</param>
		/// <returns>All of the values of the first key.</returns>
		public IEnumerable<TValue> GetValues(TFirstKey firstKey)
		{
			return !_Values.TryGetValue(firstKey, out var innerDict) ? innerDict.Values : Enumerable.Empty<TValue>();
		}
		/// <summary>
		/// Removes and returns values where <paramref name="time"/> is greater than their held time.
		/// </summary>
		/// <param name="time">The time to check against.</param>
		/// <returns>An enumerable of objects which are older than the passed in time.</returns>
		public IEnumerable<TValue> RemoveValues(DateTime time)
		{
			//Loop through each inner dictionary inside the outer dictionary
			foreach (var outerKvp in _Values)
			{
				foreach (var value in TimersService.RemoveItemsByTime(outerKvp.Value, time))
				{
					yield return value;
				}
			}
		}
	}
}
