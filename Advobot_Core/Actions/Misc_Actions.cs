using System;
using System.Text.RegularExpressions;

namespace Advobot
{
	namespace Actions
	{
		public static class MiscActions
		{
			public static bool MakeSureInputIsValidTwitchAccountName(string input)
			{
				//In the bot's case if it's a null name then that just means to not show a stream
				if (String.IsNullOrWhiteSpace(input))
					return true;

				return new Regex("^[a-zA-Z0-9_]{4,25}$", RegexOptions.Compiled).IsMatch(input); //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
			}
		}
	}
}