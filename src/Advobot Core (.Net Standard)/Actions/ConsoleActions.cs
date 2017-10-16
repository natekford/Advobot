using Advobot.Actions.Formatting;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Advobot.Actions
{
	public static class ConsoleActions
	{
		private static object _MessageLock = new object();
		private static SortedDictionary<string, List<string>> _WrittenLines;

		/// <summary>
		/// Creates a dictionary to log the lines that have been written to the console.
		/// </summary>
		public static void CreateWrittenLines()
		{
			_WrittenLines = _WrittenLines ?? new SortedDictionary<string, List<string>>();
		}
		/// <summary>
		/// Returns a dictionary containing lines that have been written to the console
		/// </summary>
		/// <returns></returns>
		public static SortedDictionary<string, List<string>> GetWrittenLines()
		{
			return _WrittenLines;
		}

		/// <summary>
		/// Writes the given text to the console with a timestamp and the calling method. Writes in gray by default.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="name"></param>
		/// <param name="color"></param>
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
		/// <summary>
		/// Writes the exception in red to the console.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="name"></param>
		public static void ExceptionToConsole(Exception e, [CallerMemberName] string name = "")
		{
			WriteLine($"EXCEPTION: {e}", name, ConsoleColor.Red);
		}
	}
}