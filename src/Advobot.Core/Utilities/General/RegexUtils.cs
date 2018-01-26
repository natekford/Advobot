using System;
using System.Text.RegularExpressions;
using Advobot.Core.Classes;

namespace Advobot.Core.Utilities
{
	/// <summary>
	/// Actions which are done on a <see cref="Regex"/>.
	/// </summary>
	public static class RegexUtils
	{
		private static Regex _TwitchRegex = new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled);
		private static TimeSpan _Timespan = new TimeSpan(Constants.TICKS_REGEX_TIMEOUT);

		/// <summary>
		/// Tries to create a <see cref="Regex"/>. Returns false if unable to created a <see cref="Regex"/> with the given input.
		/// </summary>
		/// <param name="pattern"></param>
		/// <param name="regexOutput"></param>
		/// <param name="errorReason"></param>
		/// <returns></returns>
		public static bool TryCreateRegex(string pattern, out Regex regexOutput, out Error errorReason)
		{
			regexOutput = null;
			errorReason = default;
			if (String.IsNullOrEmpty(pattern))
			{
				errorReason = new Error("The pattern cannot be null or empty.");
				return false;
			}

			try
			{
				regexOutput = new Regex(pattern);
				return true;
			}
			catch (ArgumentException e)
			{
				errorReason = new Error(e.Message);
				return false;
			}
		}
		/// <summary>
		/// Returns true if the pattern is found within the input. Has a timeout of 1,000,000 ticks.
		/// Will only check up to the first 2,000 characters since this should only be applied to message content.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool IsMatch(string input, string pattern)
		{
			var content = input.Substring(0, Math.Min(input.Length, Constants.MAX_MESSAGE_LENGTH));

			try
			{
				return Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase, _Timespan);
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}
		/// <summary>
		/// Returns true if the name is null, empty, or matches the <see cref="Regex"/> from https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool IsValidTwitchName(string input)
		{
			return String.IsNullOrEmpty(input) ? true : _TwitchRegex.IsMatch(input);
		}
	}
}
