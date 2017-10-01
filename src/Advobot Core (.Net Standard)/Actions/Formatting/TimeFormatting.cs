using System;
using System.Diagnostics;

namespace Advobot.Actions.Formatting
{
	public static class TimeFormatting
	{
		/// <summary>
		/// Returns a formatted string displaying the bot's current uptime.
		/// </summary>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static string FormatUptime()
		{
			var span = DateTime.UtcNow.Subtract(Process.GetCurrentProcess().StartTime.ToUniversalTime());
			return $"{span.Days}:{span.Hours:00}:{span.Minutes:00}:{span.Seconds:00}";
		}
		/// <summary>
		/// Returns the current time in a year, month, day, hour, minute, second format. E.G: 20170815_053645
		/// </summary>
		/// <returns></returns>
		public static string FormatDateTimeForSaving()
		{
			return DateTime.UtcNow.ToString("yyyyMMdd_hhmmss");
		}
		/// <summary>
		/// Returns the passed in time as a human readable time.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string FormatReadableDateTime(DateTime dt)
		{
			var ndt = dt.ToUniversalTime();
			var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ndt.Month);
			return $"{monthName} {ndt.Day}, {ndt.Year} at {ndt.ToLongTimeString()}";
		}
		/// <summary>
		/// Returns the passed in time as a human readable time and says how many days ago it was.
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string FormatDateTimeForCreatedAtMessage(DateTime? dt)
		{
			return $"**Created:** `{FormatReadableDateTime(dt ?? DateTime.UtcNow)}` (`{DateTime.UtcNow.Subtract(dt ?? DateTime.UtcNow).TotalDays}` days ago)";
		}
	}
}