using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Advobot.Attributes.ParameterPreconditions
{
	/// <summary>
	/// Holds a collection of valid numbers.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class NumberCollection<T> where T : IComparable<T>
	{
		private readonly ImmutableSortedSet<T> _Values;
		private readonly bool _IsRange;

		/// <summary>
		/// Creates an instance of <see cref="NumberCollection{T}"/> with the specified valid values.
		/// </summary>
		/// <param name="values"></param>
		public NumberCollection(IEnumerable<T> values)
		{
			_Values = values.ToImmutableSortedSet();
			_IsRange = false;
		}
		/// <summary>
		/// Creates an instance of <see cref="NumberCollection{T}"/> with the specified inclusive range.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public NumberCollection(T start, T end)
		{
			if (start.CompareTo(end) > 0)
			{
				throw new ArgumentException(nameof(start));
			}

			_Values = new[] { start, end }.ToImmutableSortedSet();
			_IsRange = true;
		}

		/// <summary>
		/// Whether the value is contained in the valid numbers.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool Contains(T value)
		{
			if (!_IsRange)
			{
				return _Values.Contains(value);
			}

			var start = _Values[0];
			var end = _Values[^1];
			return start.CompareTo(value) <= 0 && end.CompareTo(value) >= 0;
		}
		/// <summary>
		/// Returns the valid numbers.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
			=> _IsRange ? $"{_Values[0]} to {_Values[^1]}" : $"{string.Join(", ", _Values)}";
	}
}
