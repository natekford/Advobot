using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Advobot
{
	namespace Actions
	{
		public static class ConsoleActions
		{
			public static SortedDictionary<string, List<string>> WrittenLines = new SortedDictionary<string, List<string>>();

			public static void WriteLine(string text, [CallerMemberName] string name = "")
			{
				var line = String.Format("[{0}] [{1}]: {2}", DateTime.Now.ToString("HH:mm:ss"), name, FormattingActions.RemoveMarkdownChars(text, true));

				if (!WrittenLines.TryGetValue(name, out List<string> list))
				{
					WrittenLines.Add(name, list = new List<string>());
				}
				list.Add(line);

				Console.WriteLine(line);
			}
			public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
			{
				WriteLine("EXCEPTION: " + e, name);
			}
		}
	}
}