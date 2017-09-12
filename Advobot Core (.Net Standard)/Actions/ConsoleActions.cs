using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Advobot.Actions
{
	public static class ConsoleActions
	{
		private static object _MessageLock = new object();
		private static SortedDictionary<string, List<string>> _WrittenLines;

		public static void CreateWrittenLines()
		{
			_WrittenLines = _WrittenLines ?? new SortedDictionary<string, List<string>>();
		}
		public static SortedDictionary<string, List<string>> GetWrittenLines()
		{
			return _WrittenLines;
		}

		public static void WriteLine(string text, [CallerMemberName] string name = "", ConsoleColor color = ConsoleColor.Gray)
		{
			var line = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{name}]: {text.RemoveAllMarkdown().RemoveDuplicateNewLines()}";

			lock (_MessageLock)
			{
				if (_WrittenLines != null)
				{
					if (!_WrittenLines.TryGetValue(name, out List<string> list))
					{
						_WrittenLines.Add(name, list = new List<string>());
					}
					list.Add(line);
				}

				Console.ForegroundColor = color;
				Console.WriteLine(line);
			}
		}
		public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
		{
			WriteLine("EXCEPTION: " + e, name, ConsoleColor.Red);
		}
	}
}