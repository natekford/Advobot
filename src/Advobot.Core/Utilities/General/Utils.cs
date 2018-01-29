using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are extensions to other classes.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if two strings are the same.
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <returns></returns>
		public static bool CaseInsEquals(this string str1, string str2)
		{
			return String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string contains a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this string source, string search)
		{
			return source != null && search != null && source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to return the index of a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		public static bool CaseInsIndexOf(this string source, string search, out int position)
		{
			position = source == null || search == null ? -1 : source.IndexOf(search, StringComparison.OrdinalIgnoreCase);
			return position >= 0;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsStartsWith(this string source, string search)
		{
			return source != null && search != null && source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsEndsWith(this string source, string search)
		{
			return source != null && search != null && source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
		}
		/// <summary>
		/// Returns the string with the oldValue replaced with the newValue case insensitively.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="oldValue"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public static string CaseInsReplace(this string source, string oldValue, string newValue)
		{
			var sb = new StringBuilder();
			var previousIndex = 0;
			var index = source.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				sb.Append(source.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = source.IndexOf(oldValue, index, StringComparison.OrdinalIgnoreCase);
			}
			return sb.Append(source.Substring(previousIndex)).ToString();
		}
		/// <summary>
		/// Utilizes <see cref="CaseInsEquals(string, string)"/> to check if every string is the same.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static bool CaseInsEverythingSame(this IEnumerable<string> enumerable)
		{
			var array = enumerable.ToArray();
			for (var i = 1; i < array.Length; ++i)
			{
				if (!array[i - 1].CaseInsEquals(array[i]))
				{
					return false;
				}
			}
			return true;
		}
		/// <summary>
		/// Utilizes <see cref="StringComparer.OrdinalIgnoreCase"/> to see if the search string is in the enumerable.
		/// </summary>
		/// <param name="enumerable"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
		{
			return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Verifies all characters in the string have a value of a less than the upperlimit.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="limit"></param>
		/// <returns></returns>
		public static bool AllCharsWithinLimit(this string str, int limit = -1)
		{
			return !str.Any(x => x > (limit < 0 ? 1000 : limit));
		}
		/// <summary>
		/// Returns the count of characters equal to \r or \n.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int CountLineBreaks(this string str)
		{
			return str?.Count(x => x == '\r' || x == '\n') ?? 0;
		}
		/// <summary>
		/// Counts how many times something that implements <see cref="ITime"/> has occurred within a given timeframe.
		/// Also modifies the queue by removing instances which are too old to matter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <param name="seconds"></param>
		/// <param name="removeOldInstances"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When <paramref name="queue"/> is not in order.</exception>
		/// <exception cref="InvalidOperationException">When <paramref name="queue"/> has been modified during method run time.</exception>
		public static int CountItemsInTimeFrame<T>(this ConcurrentQueue<T> queue, int seconds = 0, bool removeOldInstances = false) where T : ITime
		{
			var timeList = new List<T>(queue);
			//No timeFrame given means that it's a spam prevention that doesn't check against time, like longmessage or mentions
			var listLength = timeList.Count;
			if (seconds <= 0 || listLength < 2)
			{
				return listLength;
			}

			//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
			var maxCount = 0;
			for (var i = 0; i < listLength; ++i)
			{
				//If the queue is out of order that kinda ruins the method
				if (i > 0 && timeList[i - 1].Time > timeList[i].Time)
				{
					throw new ArgumentException("The queue must be in order from oldest to newest.", nameof(queue));
				}

				var currentIterCount = 1;
				for (var j = i + 1; j < listLength; ++j)
				{
					if ((int)(timeList[j].Time - timeList[i].Time).TotalSeconds < seconds)
					{
						++currentIterCount;
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
					if ((int)(timeList[j].Time - timeList[j - 1].Time).TotalSeconds > seconds)
					{
						i = j + 1;
					}
					break;
				}
				maxCount = Math.Max(maxCount, currentIterCount);
			}

			if (removeOldInstances)
			{
				//Work the way down
				var now = DateTime.UtcNow;
				for (int i = listLength - 1; i >= 0; --i)
				{
					//if the time is recent enough to still be within the timeframe leave it
					if ((int)(now - timeList[i].Time).TotalSeconds < seconds + 1)
					{
						continue;
					}
					//The first object now found within the timeframe is where objects will be removed up to
					for (int j = 0; j < i; ++j)
					{
						//Make sure the queue and the source are looking at the same object
						if (queue.TryPeek(out var peekResult) && peekResult.Time != timeList[j].Time)
						{
							throw new InvalidOperationException($"{nameof(queue)} has had an object dequeued.");
						}

						queue.TryDequeue(out _);
					}
					break;
				}
			}

			return maxCount;
		}

		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task<T> CAF<T>(this Task<T> task)
		{
			return await task.ConfigureAwait(false);
		}
		/// <summary>
		/// Short way to write ConfigureAwait(false).
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task CAF(this Task task)
		{
			await task.ConfigureAwait(false);
		}
	}
}