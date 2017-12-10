using Advobot.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Actions
{
	/// <summary>
	/// Actions which are extensions to other classes.
	/// </summary>
	public static class ExtendedActions
	{
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if two strings are the same.
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <returns></returns>
		public static bool CaseInsEquals(this string str1, string str2)
			=> String.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string contains a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsContains(this string source, string search)
			=> source != null && search != null && source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
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
			=> source != null && search != null && source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
		/// <summary>
		/// Utilizes <see cref="StringComparison.OrdinalIgnoreCase"/> to check if a string ends with a search string.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="search"></param>
		/// <returns></returns>
		public static bool CaseInsEndsWith(this string source, string search)
			=> source != null && search != null && source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
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
			for (int i = 1; i < array.Length; ++i)
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
			=> enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Verifies all characters in the string have a value of a less than the upperlimit.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="upperLimit"></param>
		/// <returns></returns>
		public static bool AllCharactersAreWithinUpperLimit(this string str, int upperLimit = -1)
			=> !str.Any(x => x > (upperLimit < 0 ? Constants.MAX_UTF16_VAL_FOR_NAMES : upperLimit));
		/// <summary>
		/// Returns the enum's name as a string.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static string EnumName(this Enum e)
			=> Enum.GetName(e.GetType(), e);
		/// <summary>
		/// Returns the count of characters equal to \r or \n.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static int CountLineBreaks(this string str)
			=> str?.Count(x => x == '\r' || x == '\n') ?? 0;
		/// <summary>
		/// Counts how many times something that implements <see cref="ITime"/> has occurred within a given timeframe.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="queue"></param>
		/// <param name="timeFrame"></param>
		/// <returns></returns>
		public static int CountItemsInTimeFrame<T>(this ConcurrentQueue<T> queue, int timeFrame = 0) where T : IHasTime
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
			for (int i = 0; i < listLength; ++i)
			{
				for (int j = i + 1; j < listLength; ++j)
				{
					if ((int)(timeList[j].GetTime() - timeList[i].GetTime()).TotalSeconds < timeFrame)
					{
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
					else if ((int)(timeList[j].GetTime() - timeList[j - 1].GetTime()).TotalSeconds > timeFrame)
					{
						i = j;
					}
					break;
				}
			}

			//Remove all that are older than the given timeframe (with an added 1 second margin)
			//Do this because if they're too old then they cannot affect any spam prevention that relies on a timeframe
			var nowTime = DateTime.UtcNow;
			for (int i = listLength - 1; i >= 0; --i)
			{
				if ((int)(nowTime - timeList[i].GetTime()).TotalSeconds < timeFrame + 1)
				{
					//Make sure the queue and the list are looking at the same object
					if (queue.TryPeek(out var peekResult) && peekResult.GetTime() == timeList[i].GetTime())
					{
						queue.TryDequeue(out var dequeueResult);
					}
					continue;
				}
				break;
			}

			return count;
		}
		/// <summary>
		/// Returns the length of a number.
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static int GetLengthOfNumber(this int num)
			=> num == 0 ? 1 : (int)Math.Log10(Math.Abs(num)) + 1;
		/// <summary>
		/// Takes a variable number of integers and cuts the list the smallest one (including the list's length).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="x"></param>
		/// <returns></returns>
		public static List<T> GetUpToAndIncludingMinNum<T>(this List<T> list, params int[] x)
			=> list.GetRange(0, Math.Max(0, Math.Min(list.Count, x.Min())));

		/// <summary>
		/// Short way to write <see cref="Task.ConfigureAwait(false)"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task<T> CAF<T>(this Task<T> task) => await task.ConfigureAwait(false);
		/// <summary>
		/// Short way to write <see cref="Task.ConfigureAwait(false)"/>.
		/// </summary>
		/// <param name="task"></param>
		/// <returns></returns>
		public static async Task CAF(this Task task) => await task.ConfigureAwait(false);
	}
}