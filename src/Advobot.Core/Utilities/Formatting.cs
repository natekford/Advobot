using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Formatting for basic things, such as escaping characters or removing new lines.
	/// </summary>
	public static class Formatting
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

		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime.
		/// </summary>
		/// <returns></returns>
		public static string GetUptime()
		{
			var span = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
			return $"{(int)span.TotalDays}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns></returns>
		public static string ToSaving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		/// <summary>
		/// Returns the passed in time as a human readable time.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToReadable(this DateTime dt)
		{
			var utc = dt.ToUniversalTime();
			var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(utc.Month);
			return $"{monthName} {utc.Day}, {utc.Year} at {utc.ToLongTimeString()}";
		}
		/// <summary>
		/// Returns the passed in time as a human readable time and says how many days ago it was.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string ToCreatedAt(this DateTime dt)
		{
			var diff = DateTime.UtcNow.Subtract(dt).TotalDays;
			return $"**Created:** `{ToReadable(dt)}` (`{diff:0.00}` days ago)";
		}
	}
}
