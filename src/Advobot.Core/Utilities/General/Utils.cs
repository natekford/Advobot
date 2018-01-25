using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Advobot.Core.Enums;
using Advobot.Core.Interfaces;
using Discord;

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
		/// Converts an enum to the names of the values it contains. <typeparamref name="TEnum"/> MUST be an enum.
		/// </summary>
		/// <typeparam name="TEnum">The enum to convert.</typeparam>
		/// <param name="value">The instance value of the enum.</param>
		/// <returns>The names of the values <paramref name="value"/> contains.</returns>
		/// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
		/// <remarks>Roughly 2x slower than any explicit method, but it is generic.</remarks>
		public static IEnumerable<string> GetNamesFromEnum<TEnum>(TEnum value) where TEnum : struct
		{
			//If only generics could be restricted fully to enums, this stupid shit wouldn't have to be done
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException($"Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			//Has to be created from underlying type so Marshal.SizeOf doesn't throw an exception
			//Add one so this value can be used to bitshift in the for loop
			//Can't just use 1 or 1UL in the loop because that might be the incorrect underlying type
			var startVal = (dynamic)Activator.CreateInstance(Enum.GetUnderlyingType(typeof(TEnum))) + 1;
			//Use Marshal.SizeOf to loop through every single bit in the enum
			for (int i = 0; i < Marshal.SizeOf(startVal) * 8; ++i)
			{
				//Cast the bitshifted value so it can & together with the passed in enum value
				var bitVal = (TEnum)(startVal << i);
				//Cast the passed in enum value as dynamic since TEnum can't do bitwise functions on its own
				if (Enum.IsDefined(typeof(TEnum), bitVal) && ((dynamic)value & bitVal) == bitVal)
				{
					yield return Enum.GetName(typeof(TEnum), bitVal);
				}
			}
		}
		public static IEnumerable<string> GetNamesFromEnum2(GuildPermission value)
		{
			for (int i = 0; i < 64; ++i)
			{
				var bitVal = 1UL << i;
				if (Enum.IsDefined(typeof(GuildPermission), bitVal) && ((ulong)value & bitVal) == bitVal)
				{
					yield return Enum.GetName(typeof(GuildPermission), bitVal);
				}
			}
		}
		/// <summary>
		/// Attempts to parse enums from the supplied values. <typeparamref name="TEnum"/> MUST be an enum.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="value">The valid enums.</param>
		/// <param name="invalidInput">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		/// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
		public static bool TryParseEnums<TEnum>(IEnumerable<string> input, out List<TEnum> validInput, out List<string> invalidInput) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException("Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			validInput = new List<TEnum>();
			invalidInput = new List<string>();
			foreach (var enumName in input)
			{
				if (Enum.TryParse<TEnum>(enumName, true, out var result))
				{
					validInput.Add(result);
				}
				else
				{
					invalidInput.Add(enumName);
				}
			}
			return !invalidInput.Any();
		}
		/// <summary>
		/// Attempts to parse all enums then OR them together. <typeparamref name="TEnum"/> MUST be an enum.
		/// </summary>
		/// <typeparam name="TEnum">The enum to parse.</typeparam>
		/// <param name="input">The input names.</param>
		/// <param name="value">The return value of every valid enum ORed together.</param>
		/// <param name="invalidInput">The invalid names.</param>
		/// <returns>A boolean indicating if there were any failed parses.</returns>
		/// <exception cref="ArgumentException">When <typeparamref name="TEnum"/> is not an enum.</exception>
		public static bool TryParseEnums<TEnum>(IEnumerable<string> input, out TEnum value, out List<string> invalidInput) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnum)
			{
				throw new ArgumentException($"Invalid generic parameter type. Must be an enum.", nameof(TEnum));
			}

			//Cast as dynamic so bitwise functions can be done on it
			dynamic temp = Activator.CreateInstance<TEnum>();
			invalidInput = new List<string>();
			foreach (var enumName in input)
			{
				if (Enum.TryParse<TEnum>(enumName, true, out var result))
				{
					temp |= result;
				}
				else
				{
					invalidInput.Add(enumName);
				}
			}
			value = (TEnum)temp;
			return !invalidInput.Any();
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