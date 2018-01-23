using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;

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
		/// <param name="upperLimit"></param>
		/// <returns></returns>
		public static bool AllCharactersAreWithinUpperLimit(this string str, int upperLimit = -1)
		{
			return !str.Any(x => x > (upperLimit < 0 ? Constants.MAX_UTF16_VAL_FOR_NAMES : upperLimit));
		}
		/// <summary>
		/// Returns the enum's name as a string.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string EnumName(this Enum e)
		{
			return Enum.GetName(e.GetType(), e);
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
		/// <param name="timeFrame"></param>
		/// <param name="removeOldInstances"></param>
		/// <returns></returns>
		public static int CountItemsInTimeFrame<T>(this ConcurrentQueue<T> queue, int timeFrame = 0, bool removeOldInstances = false) where T : ITime
		{
			var timeList = new List<T>(queue);
			//No timeFrame given means that it's a spam prevention that doesn't check against time, like longmessage or mentions
			var listLength = timeList.Count;
			if (timeFrame <= 0 || listLength < 2)
			{
				return listLength;
			}

			//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
			var count = 0;
			for (var i = 0; i < listLength; ++i)
			{
				for (var j = i + 1; j < listLength; ++j)
				{
					if ((int)(timeList[j].Time - timeList[i].Time).TotalSeconds < timeFrame)
					{
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1

					if ((int)(timeList[j].Time - timeList[j - 1].Time).TotalSeconds > timeFrame)
					{
						i = j;
					}
					break;
				}
			}

			if (removeOldInstances)
			{
				//Remove all that are older than the given timeframe (with an added 1 second margin)
				//Do this because if they're too old then they cannot affect any spam prevention that relies on a timeframe
				var now = DateTime.UtcNow;
				for (var i = listLength - 1; i >= 0; --i)
				{
					if ((int)(now - timeList[i].Time).TotalSeconds >= timeFrame + 1)
					{
						break;
					}

					//Make sure the queue and the source are looking at the same object
					if (queue.TryPeek(out var peekResult) && peekResult.Time != timeList[i].Time)
					{
						throw new InvalidOperationException($"{nameof(queue)} has had an object dequeued.");
					}

					queue.TryDequeue(out _);
				}
			}

			return count;
		}
		/// <summary>
		/// Gets and removes items older than <paramref name="time"/>.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="dictionary"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		public static IEnumerable<TValue> GetItemsByTime<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dictionary, DateTime time) where TValue : ITime
		{
			//Loop through every value in the dictionary, remove if too old
			foreach (var kvp in dictionary)
			{
				if (kvp.Value.Time.Ticks < time.Ticks && dictionary.TryRemove(kvp.Key, out var value))
				{
					yield return value;
				}
			}
		}
		/// <summary>
		/// Returns the length of a number.
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static int GetLength(this int num)
		{
			return num.ToString().Length;
		}
		/// <summary>
		/// Takes a variable number of integers and cuts the source the smallest one (including the source's length).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static IEnumerable<T> TakeMin<T>(this IEnumerable<T> source, params int[] x)
		{
			var list = source.ToList();
			return list.Take(Math.Max(0, Math.Min(list.Count, x.Min()))).ToList();
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="target"></param>
		/// <param name="count"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetObjectsInListBasedOffOfCount<T>(this IEnumerable<T> objects, CountTarget target, uint? count, Func<T, int?> f)
		{
			switch (target)
			{
				case CountTarget.Equal:
				{
					objects = objects.Where(x => { var val = f(x); return val != null && val == count; });
					break;
				}
				case CountTarget.Below:
				{
					objects = objects.Where(x => { var val = f(x); return val != null && val < count; });
					break;
				}
				case CountTarget.Above:
				{
					objects = objects.Where(x => { var val = f(x); return val != null && val > count; });
					break;
				}
			}
			return objects;
		}
		/// <summary>
		/// Returns all public properties that have a set method.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetSettings(Type t)
		{
			return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite && x.GetSetMethod(true).IsPublic).ToArray();
		}
		/// <summary>
		/// Returns all public properties that have a set method and are not <see cref="String"/> or <see cref="IEnumerable{T}"/>.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static PropertyInfo[] GetNonEnumerableSettings(Type t)
		{
			return GetSettings(t).Where(p =>
			{
				var pt = p.PropertyType;
				return pt != typeof(string)
					&& pt != typeof(IEnumerable)
					&& !pt.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
			}).ToArray();
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