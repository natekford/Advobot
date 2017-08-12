using Advobot.Attributes;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Advobot.RemovablePunishments;
using Advobot.SavedClasses;
using Advobot.Structs;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot
{
	namespace Actions
	{
		public static class ExtendedMethods
		{
			public static async Task ForEachAsync<T>(this List<T> list, Func<T, Task> func)
			{
				foreach (var value in list)
				{
					await func(value);
				}
			}
			public static async void Forget(this Task task)
			{
				try
				{
					await task.ConfigureAwait(false);
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}

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

			public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
			{
				var count = 0;
				var maxLen = list.Count().ToString().Length;
				//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
				return String.Join("\n", list.Select(x => String.Format("`{0}.` ", (++count).ToString().PadLeft(maxLen, '0')) + String.Format(@format, args.Select(y => y(x)).ToArray())));
			}
			public static string FormatUser(this IUser user, ulong? userID = 0)
			{
				if (user != null)
				{
					return String.Format("'{0}#{1}' ({2})",
						FormattingActions.EscapeMarkdown(user.Username, true).CaseInsReplace("discord.gg", Constants.FAKE_DISCORD_LINK),
						user.Discriminator,
						user.Id);
				}
				else
				{
					return String.Format("Irretrievable User ({0})", userID);
				}
			}
			public static string FormatRole(this IRole role)
			{
				if (role != null)
				{
					return String.Format("'{0}' ({1})", FormattingActions.EscapeMarkdown(role.Name, true), role.Id);
				}
				else
				{
					return "Irretrievable Role";
				}
			}
			public static string FormatChannel(this IChannel channel)
			{
				if (channel != null)
				{
					return String.Format("'{0}' ({1}) ({2})", FormattingActions.EscapeMarkdown(channel.Name, true), (channel is IMessageChannel ? "text" : "voice"), channel.Id);
				}
				else
				{
					return "Irretrievable Channel";
				}
			}
			public static string FormatGuild(this IGuild guild, ulong? guildID = 0)
			{
				if (guild != null)
				{
					return String.Format("'{0}' ({1})", FormattingActions.EscapeMarkdown(guild.Name, true), guild.Id);
				}
				else
				{
					return String.Format("Irretrievable Guild ({0})", guildID);
				}
			}

			public static bool CaseInsEquals(this string str1, string str2)
			{
				if (str1 == null)
				{
					return str2 == null;
				}
				else if (str2 == null)
				{
					return false;
				}
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
			public static bool HasGlobalEmotes(this IGuild guild)
			{
				return guild.Emotes.Any(x => x.IsManaged && x.RequireColons);
			}

			public static int GetLineBreaks(this string str)
			{
				if (str == null)
					return 0;

				return str.Count(x => x == '\r' || x == '\n');
			}

			public static IGuild GetGuild(this IMessage message)
			{
				return (message?.Channel as IGuildChannel)?.Guild;
			}
			public static IGuild GetGuild(this IUser user)
			{
				return (user as IGuildUser)?.Guild;
			}
			public static IGuild GetGuild(this IChannel channel)
			{
				return (channel as IGuildChannel)?.Guild;
			}
			public static IGuild GetGuild(this IRole role)
			{
				return role?.Guild;
			}

			public static List<T> GetUpToAndIncludingMinNum<T>(this List<T> list, params int[] x)
			{
				return list.GetRange(0, Math.Max(0, Math.Min(list.Count, x.Min())));
			}
			public static List<T> GetOutTimedObjects<T>(this List<T> inputList) where T : ITimeInterface
			{
				if (inputList == null)
					return null;

				var eligibleToBeGotten = inputList.Where(x => x.GetTime() <= DateTime.UtcNow).ToList();
				inputList.ThreadSafeRemoveAll(x => eligibleToBeGotten.Contains(x));
				return eligibleToBeGotten;
			}
			public static int GetCountOfItemsInTimeFrame<T>(this List<T> timeList, int timeFrame = 0) where T : ITimeInterface
			{
				lock (timeList)
				{
					//No timeFrame given means that it's a spam prevention that doesn't check against time, like longmessage or mentions
					var listLength = timeList.Count;
					if (timeFrame <= 0 || listLength < 2)
						return listLength;

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
									i = j;
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
				if (String.IsNullOrWhiteSpace(inputString))
					return null;

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

			public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> readOnlyDick)
			{
				return readOnlyDick.ToDictionary(nixon => nixon.Key, cheney => cheney.Value);
			}
		}
	}
}
