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
		public static string Uptime()
		{
			var span = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime());
			return $"{span.Days}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns></returns>
		public static string Saving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		/// <summary>
		/// Returns the passed in time as a human readable time.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string Readable(this DateTime dt)
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
		public static string CreatedAt(this DateTime dt)
		{
			var time = Readable(dt);
			var diff = DateTime.UtcNow.Subtract(dt).Days;
			return $"**Created:** `{time}` (`{diff}` days ago)";
		}
	}
}