﻿using System;
using System.Text.RegularExpressions;

namespace Advobot.Actions
{
	public static class RegexActions
	{
		private static readonly Regex _TwitchRegex = new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled);

		/// <summary>
		/// Tries to create a <see cref="Regex"/>. Returns false if unable to created a <see cref="Regex"/> with the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="regexOutput"></param>
		/// <param name="stringOutput"></param>
		/// <returns></returns>
		public static bool TryCreateRegex(string input, out Regex regexOutput, out string errorOutput)
		{
			regexOutput = null;
			errorOutput = null;
			try
			{
				regexOutput = new Regex(input);
				return true;
			}
			catch (Exception e)
			{
				errorOutput = e.Message;
				return false;
			}
		}
		/// <summary>
		/// Returns true if the pattern is found within the input. Has a timeout of 1,000,000 ticks.
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		public static bool CheckIfRegexMatch(string input, string pattern)
		{
			return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase, new TimeSpan(Constants.TICKS_REGEX_TIMEOUT));
		}
		/// <summary>
		/// Returns true if the name is null, empty, or matches the <see cref="Regex"/> from https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static bool CheckIfInputIsAValidTwitchName(string input)
		{
			//In the bot's case if it's a null name then that just means to not show a stream
			if (String.IsNullOrEmpty(input))
			{
				return true;
			}

			return _TwitchRegex.IsMatch(input); 
		}
	}
}