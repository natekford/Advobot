using Advobot.Actions;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot
{
	public static class ExtendedActions
	{
		public static void ThreadSafeAdd<T>(this List<T> list, T obj)
		{
			lock (list)
			{
				list.Add(obj);
			}
		}
		public static void ThreadSafeAddRange<T>(this List<T> list, IEnumerable<T> objs)
		{
			lock (list)
			{
				list.AddRange(objs);
			}
		}
		public static void ThreadSafeRemove<T>(this List<T> list, T obj)
		{
			lock (list)
			{
				list.Remove(obj);
			}
		}
		public static void ThreadSafeRemoveAll<T>(this List<T> list, Predicate<T> match)
		{
			lock (list)
			{
				list.RemoveAll(match);
			}
		}

		public static bool CaseInsEquals(this string str1, string str2)
		{
			//null == null
			if (str1 == null)
			{
				return str2 == null;
			}
			//x != null
			else if (str2 == null)
			{
				return false;
			}
			//x ?= x
			else
			{
				return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
			}
		}
		public static bool CaseInsContains(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
			}
		}
		public static bool CaseInsIndexOf(this string source, string search, out int position)
		{
			position = -1;
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return (position = source.IndexOf(search, StringComparison.OrdinalIgnoreCase)) >= 0;
			}
		}
		public static bool CaseInsStartsWith(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.StartsWith(search, StringComparison.OrdinalIgnoreCase);
			}
		}
		public static bool CaseInsEndsWith(this string source, string search)
		{
			if (source == null || search == null)
			{
				return false;
			}
			else
			{
				return source.EndsWith(search, StringComparison.OrdinalIgnoreCase);
			}
		}
		public static string CaseInsReplace(this string source, string oldValue, string newValue)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

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
			sb.Append(source.Substring(previousIndex));

			return sb.ToString();
		}
		public static bool CaseInsEverythingSame(this IEnumerable<string> enumerable)
		{
			var array = enumerable.ToArray();
			for (int i = 1; i < array.Length; ++i)
			{
				if (!array[i - 1].CaseInsEquals(array[i]))
					return false;
			}
			return true;
		}
		public static bool CaseInsContains(this IEnumerable<string> enumerable, string search)
		{
			if (enumerable.Any())
			{
				return enumerable.Contains(search, StringComparer.OrdinalIgnoreCase);
			}
			return false;
		}

		public static string EscapeAllMarkdown(this string input)
		{
			return input.Replace("`", "\\`").Replace("*", "\\*").Replace("_", "\\_");
		}
		public static string EscapeBackTicks(this string input)
		{
			return input.Replace("`", "\\`");
		}
		public static string RemoveAllMarkdown(this string input)
		{
			return input.Replace("`", "").Replace("*", "").Replace("_", "");
		}
		public static string RemoveDuplicateNewLines(this string input)
		{
			while (input.Contains("\n\n"))
			{
				input = input.Replace("\n\n", "\n");
			}
			return input;
		}
		public static string RemoveAllNewLines(this string input)
		{
			return input.Replace(Environment.NewLine, "").Replace("\r", "").Replace("\n", "");
		}
		public static string EnumName(this Enum e)
		{
			return Enum.GetName(e.GetType(), e);
		}

		public static bool AllCharactersAreWithinUpperLimit(this string str, int upperLimit)
		{
			if (String.IsNullOrWhiteSpace(str))
				return false;

			foreach (var c in str)
			{
				if (c > upperLimit)
					return false;
			}
			return true;
		}
		public static int GetLineBreaks(this string str)
		{
			if (str == null)
				return 0;

			return str.Count(x => x == '\r' || x == '\n');
		}

		public static List<T> GetUpToAndIncludingMinNum<T>(this List<T> list, params int[] x)
		{
			return list.GetRange(0, Math.Max(0, Math.Min(list.Count, x.Min())));
		}
		public static List<T> GetOutTimedObjects<T>(this List<T> inputList) where T : ITimeInterface
		{
			if (inputList == null)
			{
				return null;
			}

			var eligibleToBeGotten = inputList.Where(x => x.GetTime() < DateTime.UtcNow).ToList();
			inputList.ThreadSafeRemoveAll(x => eligibleToBeGotten.Contains(x));
			return eligibleToBeGotten;
		}
		public static Dictionary<TKey, TValue> GetOutTimedObjects<TKey, TValue>(this Dictionary<TKey, TValue> inputDict) where TValue : ITimeInterface
		{
			if (inputDict == null)
			{
				return null;
			}

			var elligibleToBeGotten = inputDict.Where(x => x.Value.GetTime() < DateTime.UtcNow).ToList();
			foreach (var value in elligibleToBeGotten)
			{
				inputDict.Remove(value.Key);
			}
			return elligibleToBeGotten.ToDictionary(x => x.Key, x => x.Value);
		}
		public static int GetCountOfItemsInTimeFrame<T>(this List<T> timeList, int timeFrame = 0) where T : ITimeInterface
		{
			lock (timeList)
			{
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
						if ((int)timeList[j].GetTime().Subtract(timeList[i].GetTime()).TotalSeconds >= timeFrame)
						{
							//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
							if ((int)timeList[j].GetTime().Subtract(timeList[j - 1].GetTime()).TotalSeconds > timeFrame)
							{
								i = j;
							}
							break;
						}
					}
				}

				//Remove all that are older than the given timeframe (with an added 1 second margin)
				var nowTime = DateTime.UtcNow;
				for (int i = listLength - 1; i >= 0; --i)
				{
					if ((int)nowTime.Subtract(timeList[i].GetTime()).TotalSeconds > timeFrame + 1)
					{
						timeList.RemoveRange(0, i + 1);
						break;
					}
				}

				return count;
			}
		}

		public static string[] SplitByCharExceptInQuotes(this string inputString, char inputChar)
		{
			if (inputString == null)
			{
				return null;
			}

			return inputString.Split('"').Select((element, index) =>
			{
				if (index % 2 == 0)
				{
					return element.Split(new[] { inputChar }, StringSplitOptions.RemoveEmptyEntries);
				}
				else
				{
					return new[] { element };
				}
			}).SelectMany(x => x).Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();
		}

		public static T GetService<T>(this IServiceProvider provider)
		{
			return (T)provider.GetService(typeof(T));
		}
	}
}
