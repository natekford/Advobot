using Advobot.Enums;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Advobot.Formatting
{
	public static class GeneralFormatting
	{
		private static readonly Regex _RemoveDuplicateSpaces = new Regex(@"[\r\n]+", RegexOptions.Compiled);

		/// <summary>
		/// Returns a string with a zero length character and the error message added to the front of the input.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static string ERROR(string message)
		{
			return Constants.ZERO_LENGTH_CHAR + Constants.ERROR_MESSAGE + message;
		}
		/// <summary>
		/// Returns a string indicating why modifying something involving the passed in object failed.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="failureReason"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string FormatErrorString(IGuild guild, FailureReason failureReason, object obj)
		{
			string objType;
			if (obj is IUser)
			{
				objType = "user";
			}
			else if (obj is IChannel)
			{
				objType = "channel";
			}
			else if (obj is IRole)
			{
				objType = "role";
			}
			else if (obj is IGuild)
			{
				objType = "guild";
			}
			else
			{
				objType = obj.GetType().Name;
			}

			switch (failureReason)
			{
				case FailureReason.TooFew:
				{
					return $"Unable to find the {objType}.";
				}
				case FailureReason.UserInability:
				{
					return $"You are unable to make the given changes to the {objType}: `{DiscordObjectFormatting.FormatDiscordObject(obj)}`.";
				}
				case FailureReason.BotInability:
				{
					return $"I am unable to make the given changes to the {objType}: `{DiscordObjectFormatting.FormatDiscordObject(obj)}`.";
				}
				case FailureReason.TooMany:
				{
					return $"There are too many {objType}s with the same name.";
				}
				case FailureReason.EveryoneRole:
				{
					return "The everyone role cannot be modified in that way.";
				}
				case FailureReason.ManagedRole:
				{
					return "Managed roles cannot be modified in that way.";
				}
				case FailureReason.InvalidEnum:
				{
					return $"The option `{(obj as Enum).EnumName()}` is not accepted in this instance.";
				}
				default:
				{
					return "This shouldn't be seen. - Advobot";
				}
			}
		}
		/// <summary>
		/// Returns a string saying who did an action with the bot and possibly why they did it.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static string FormatUserReason(IUser user, string reason = null)
		{
			return $"Action by {user.FormatUser()}.{(reason == null ? "" : $" Reason: {reason.TrimEnd('.')}.")}";
		}
		/// <summary>
		/// Returns a string saying the bot did an action and possibly why it did it.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static string FormatBotReason(string reason)
		{
			if (!String.IsNullOrWhiteSpace(reason))
			{
				reason = $"Automated action. User triggered {reason.TrimEnd('.')}.";
				reason = reason.Substring(0, Math.Min(reason.Length, Constants.MAX_LENGTH_FOR_REASON));
			}
			else
			{
				reason = "Automated action. User triggered something.";
			}

			return reason;
		}

		/// <summary>
		/// Returns a string with the given number of spaces minus the length of the second object padded onto the right side of the first object.
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		public static string FormatStringsWithLength(object obj1, object obj2, int len)
		{
			var str1 = obj1.ToString();
			var str2 = obj2.ToString();
			return $"{str1.PadRight(Math.Min(len - str2.Length, 0))}{str2}";
		}
		/// <summary>
		/// Returns the first object padded right with the right argument and the second string padded left with the left argument.
		/// </summary>
		/// <param name="obj1"></param>
		/// <param name="obj2"></param>
		/// <param name="right"></param>
		/// <param name="left"></param>
		/// <returns></returns>
		public static string FormatStringsWithLength(object obj1, object obj2, int right, int left)
		{
			var str1 = obj1.ToString().PadRight(Math.Min(right, 0));
			var str2 = obj2.ToString().PadLeft(Math.Min(left, 0));
			return $"{str1}{str2}";
		}
		/// <summary>
		/// Joins all strings which are not null with the given string.
		/// </summary>
		/// <param name="joining"></param>
		/// <param name="toJoin"></param>
		/// <returns></returns>
		public static string JoinNonNullStrings(string joining, params string[] toJoin)
		{
			return String.Join(joining, toJoin.Where(x => !String.IsNullOrWhiteSpace(x)));
		}
		/// <summary>
		/// Returns a string which is a numbered list of the passed in objects.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
		{
			var count = 0;
			var maxLen = list.Count().ToString().Length;
			//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
			return String.Join("\n", list.Select(x => $"`{(++count).ToString().PadLeft(maxLen, '0')}.` " + String.Format(@format, args.Select(y => y(x)).ToArray())));
		}

		/// <summary>
		/// Returns the input string with `, *, and _, escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeAllMarkdown(this string input)
		{
			return input.Replace("`", "\\`").Replace("*", "\\*").Replace("_", "\\_");
		}
		/// <summary>
		/// Returns the input string with ` escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeBackTicks(this string input)
		{
			return input.Replace("`", "\\`");
		}
		/// <summary>
		/// Returns the input string without `, *, and _.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllMarkdown(this string input)
		{
			return input.Replace("`", "").Replace("*", "").Replace("_", "");
		}
		/// <summary>
		/// Returns the input string with no duplicate new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			return _RemoveDuplicateSpaces.Replace(input, "\n");
		}
		/// <summary>
		/// Returns the input string with no new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllNewLines(this string input)
		{
			return input.Replace("\r", "").Replace("\n", "");
		}

		/// <summary>
		/// Only appends a \n after the value. On Windows <see cref="StringBuilder.AppendLine(string)"/> appends \r\n (which isn't
		/// necessarily wanted).
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static StringBuilder AppendLineFeed(this StringBuilder sb, string value)
		{
			return sb.Append(value + "\n");
		}
	}
}
