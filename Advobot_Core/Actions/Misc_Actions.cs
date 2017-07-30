using System;
using System.Text.RegularExpressions;

namespace Advobot
{
	namespace Actions
	{
		public static class MiscActions
		{
			public static void ResetSettings()
			{
				Properties.Settings.Default.BotKey = null;
				Properties.Settings.Default.Path = null;
				Properties.Settings.Default.BotName = null;
				Properties.Settings.Default.BotID = 0;
				Properties.Settings.Default.Save();
			}
			public static void RestartBot()
			{
				try
				{
					//Create a new instance of the bot and close the old one
					System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
					Environment.Exit(0);
				}
				catch (Exception e)
				{
					ConsoleActions.ExceptionToConsole(e);
				}
			}
			public static void DisconnectBot()
			{
				Environment.Exit(0);
			}

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