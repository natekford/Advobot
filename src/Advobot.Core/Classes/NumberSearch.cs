using System;
using System.Collections.Generic;
using System.Linq;
using Advobot.Enums;

namespace Advobot.Classes
{
	/// <summary>
	/// Specifies how to search with a number.
	/// </summary>
	public sealed class NumberSearch
	{
		/// <summary>
		/// The number to search for.
		/// </summary>
		public uint? Number { get; private set; }
		/// <summary>
		/// How to use that number to search.
		/// </summary>
		public CountTarget Method { get; private set; }

		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public IEnumerable<T> GetFromCount<T>(IEnumerable<T> objects, Func<T, uint?> f)
		{
			switch (Method)
			{
				case CountTarget.Equal:
					objects = objects.Where(x => f(x) == Number);
					break;
				case CountTarget.Below:
					objects = objects.Where(x => f(x) < Number);
					break;
				case CountTarget.Above:
					objects = objects.Where(x => f(x) > Number);
					break;
			}
			return objects;
		}
	}
}
