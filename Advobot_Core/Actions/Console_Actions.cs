using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Advobot
{
	namespace Actions
	{
		public static class ConsoleActions
		{
			private static SortedDictionary<string, List<string>> _WrittenLines;

			public static void CreateWrittenLines()
			{
				_WrittenLines = _WrittenLines ?? new SortedDictionary<string, List<string>>();
			}
			public static SortedDictionary<string, List<string>> GetWrittenLines()
			{
				return _WrittenLines;
			}

			public static void WriteLine(string text, [CallerMemberName] string name = "")
			{
				var line = String.Format("[{0}] [{1}]: {2}", DateTime.Now.ToString("HH:mm:ss"), name, FormattingActions.RemoveMarkdownChars(text, true));

				//WrittenLines gets set 
				if (_WrittenLines != null)
				{
					if (!_WrittenLines.TryGetValue(name, out List<string> list))
					{
						_WrittenLines.Add(name, list = new List<string>());
					}
					list.Add(line);
				}

				Console.WriteLine(line);
			}
			public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
			{
				WriteLine("EXCEPTION: " + e, name);
			}
		}
	}
}