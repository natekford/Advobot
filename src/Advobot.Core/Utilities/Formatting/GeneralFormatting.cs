using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for basic things, such as escaping characters or removing new lines.
	/// </summary>
	public static class GeneralFormatting
	{
		private static readonly Regex _RemoveDuplicateLines = new Regex(@"[\r\n]+", RegexOptions.Compiled);

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
			var str1 = obj1.ToString().PadRight(Math.Max(right, 0));
			var str2 = obj2.ToString().PadLeft(Math.Max(left, 0));
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
		/// Returns a string which is a numbered list of the passed in objects. The format is for the passed in arguments; the counter is added by default.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string FormatNumberedList<T>(this IEnumerable<T> list, string format, params Func<T, object>[] args)
		{
			var maxLen = list.Count().GetLength();
			//.ToArray() must be used or else String.Format tries to use an overload accepting object as a parameter instead of object[] thus causing an exception
			return String.Join("\n", list.Select((x, index) => $"`{(index + 1).ToString().PadLeft(maxLen, '0')}.` {String.Format(@format, args.Select(f => f(x)).ToArray())}"));
		}

		/// <summary>
		/// Returns the input string with `, *, and _, escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeAllMarkdown(this string input)
		{
			return input?.Replace("`", "\\`")?.Replace("*", "\\*")?.Replace("_", "\\_");
		}

		/// <summary>
		/// Returns the input string with ` escaped.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string EscapeBackTicks(this string input)
		{
			return input?.Replace("`", "\\`");
		}

		/// <summary>
		/// Returns the input string without `, *, and _.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllMarkdown(this string input)
		{
			return input?.Replace("`", "")?.Replace("*", "")?.Replace("_", "");
		}

		/// <summary>
		/// Returns the input string with no duplicate new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			return _RemoveDuplicateLines.Replace(input, "\n");
		}

		/// <summary>
		/// Returns the input string with no new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveAllNewLines(this string input)
		{
			return input?.Replace("\r", "")?.Replace("\n", "");
		}

		/// <summary>
		/// Adds in spaces between each capital letter.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string FormatTitle(this string title)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < title.Length; ++i)
			{
				var c = title[i];
				if (Char.IsUpper(c) && (i > 0 && !Char.IsWhiteSpace(title[i - 1])))
				{
					sb.Append(' ');
				}
				sb.Append(c);
			}
			return sb.ToString();
		}
		/// <summary>
		/// Returns nothing if equal to 1. Returns "s" if not. Double allows most, if not all, number types in: https://stackoverflow.com/a/828963.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string FormatPlural(double i)
		{
			return i == 1 ? "" : "s";
		}

		/// <summary>
		/// Only appends a \n after the value. On Windows <see cref="StringBuilder.AppendLine(string)"/> appends \r\n (which isn't
		/// necessarily wanted).
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public static StringBuilder AppendLineFeed(this StringBuilder sb, string value = "")
		{
			return sb.Append(value + "\n");
		}
	}
}
