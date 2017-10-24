using Advobot.Core;
using Advobot.Core.Actions;
using System.Windows.Controls;

namespace Advobot.UILauncher.Actions
{
	internal static class UICommandHandler
	{
		public static string GatherInput(TextBox tb, Button b)
		{
			var text = tb.Text.Trim(new[] { '\r', '\n' });
			if (text.Contains("﷽"))
			{
				text += "This program really doesn't like that long Arabic character for some reason. Whenever there are a lot of them it crashes the program completely.";
			}

			tb.Text = "";
			b.IsEnabled = false;
			ConsoleActions.WriteLine(text);
			return text;
		}
		public static void HandleCommand(string input, string prefix)
		{
			if (input.CaseInsStartsWith(prefix))
			{
				var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
				if (!FindCommand(inputArray[0], inputArray.Length > 1 ? inputArray[1] : null))
				{
					ConsoleActions.WriteLine("No command could be found with that name.");
				}
			}
		}
		public static bool FindCommand(string cmd, string args)
		{
			return false;
		}
	}
}
