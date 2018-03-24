using AdvorangesUtils;
using System.Windows.Controls;

namespace Advobot.UILauncher.Utilities
{
	internal static class CommandHandler
	{
		/// <summary>
		/// Returns the text of the textbox and clears the textbox.
		/// </summary>
		/// <param name="tb"></param>
		/// <returns></returns>
		public static string GatherInput(TextBox tb)
		{
			var text = tb.Text.Trim('\r', '\n');
			tb.Text = null;
			return text;
		}
		/// <summary>
		/// Attemts to invoke a command if one can be found. Otherwise writes that no command could be found.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="prefix"></param>
		public static void HandleCommand(string input, string prefix)
		{
			if (!input.CaseInsStartsWith(prefix))
			{
				return;
			}

			var inputArray = input.Substring(prefix.Length)?.Split(new[] { ' ' }, 2);
			if (!FindCommand(inputArray[0], inputArray.Length > 1 ? inputArray[1] : null))
			{
				ConsoleUtils.WriteLine("No command could be found with that name.");
			}
		}
		/// <summary>
		/// Attempts to find a command with the supplied name and that can accept the supplied args.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool FindCommand(string name, string args)
		{
			return false;
		}
	}
}
