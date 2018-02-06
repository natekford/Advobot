using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for basic things, such as escaping characters or removing new lines.
	/// </summary>
	public static class GeneralFormatting
	{
		/// <summary>
		/// Joins all strings which are not null with the given string.
		/// </summary>
		/// <param name="seperator"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static string JoinNonNullStrings(this IEnumerable<string> values, string seperator)
		{
			return String.Join(seperator, values.Where(x => !String.IsNullOrWhiteSpace(x)));
		}
		/// <summary>
		/// Returns a string which is a numbered list of the passed in objects. The format is for the passed in arguments; the counter is added by default.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="values"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static string FormatNumberedList<T>(this IEnumerable<T> values, Func<T, string> func)
		{
			var list = values.ToList();
			var maxLen = list.Count.ToString().Length;
			return String.Join("\n", list.Select((x, index) => $"`{(index + 1).ToString().PadLeft(maxLen, '0')}.` {func(x)}"));
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
			return input?.Replace("\\", "")?.Replace("*", "")?.Replace("_", "")?.Replace("~", "")?.Replace("`", "");
		}
		/// <summary>
		/// Returns the input string with no duplicate new lines. Also changes any carriage returns to new lines.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string RemoveDuplicateNewLines(this string input)
		{
			var str = input.Replace("\r", "\n");
			var len = str.Length;
			do
			{
				len = str.Length;
				str = str.Replace("\n\n", "\n");
			} while (len != str.Length);
			return str;
		}
		/// <summary>
		/// Adds in spaces between each capital letter and capitalizes every letter after a space.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public static string FormatTitle(this string title)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < title.Length; ++i)
			{
				var c = title[i];
				if (Char.IsUpper(c) && (i > 0 && !Char.IsWhiteSpace(title[i - 1])))
				{
					sb.Append(' ');
				}
				var makeCapital = i == 0 || i > 0 && Char.IsWhiteSpace(title[i - 1]);
				sb.Append(makeCapital ? Char.ToUpper(c) : c);
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
			//In case of weird floating point math
			return i > 0 && Math.Abs(i - 1) <= 0 ? "" : "s";
		}
		/// <summary>
		/// Only appends a \n after the value. On Windows <see cref="StringBuilder.AppendLine(string)"/> appends \r\n (which isn't
		/// necessarily wanted).
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static StringBuilder AppendLineFeed(this StringBuilder sb, string value = "")
		{
			return sb.Append(value + "\n");
		}
	}
}
