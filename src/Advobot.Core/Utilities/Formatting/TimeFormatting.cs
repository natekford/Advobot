using System;
using System.Diagnostics;
using System.Globalization;

namespace Advobot.Core.Utilities.Formatting
{
	/// <summary>
	/// Formatting for time.
	/// </summary>
	public static class TimeFormatting
	{
		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime.
		/// </summary>
		/// <returns></returns>
		public static string GetUptime()
		{
			var span = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
			return $"{span.TotalDays}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
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