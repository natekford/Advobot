using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Advobot.Enums;
using Advobot.Interfaces;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class AdvobotUtils
	{
		/// <summary>
		/// Gets the prefix to use for this guild. Prioritizes the guild prefix over the global prefix.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static string GetPrefix(this IGuildSettings settings, IBotSettings botSettings)
			=> settings.Prefix ?? botSettings.Prefix ?? throw new InvalidOperationException("Invalid prefix.");
		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName">The name of the file without the bot directory.</param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
		/// <summary>
		/// Ensures the extension of the file is '.db'
		/// </summary>
		/// <param name="dbType"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string EnsureDb(string dbType, string fileName)
		{
			const string EXT = ".db";

			string ExtensionValidation()
			{
				if (!Path.HasExtension(fileName))
				{
					return fileName + EXT;
				}
				else if (Path.GetExtension(fileName) == EXT)
				{
					return fileName;
				}
				return Path.GetFileNameWithoutExtension(fileName) + EXT;
			}

			return Path.Combine(dbType, ExtensionValidation());
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="method"></param>
		/// <param name="number"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetFromCount<T>(this IEnumerable<T> objects, CountTarget method, int? number, Func<T, int?> f) => method switch
		{
			CountTarget.Equal => objects.Where(x => f(x) == number),
			CountTarget.Below => objects.Where(x => f(x) < number),
			CountTarget.Above => objects.Where(x => f(x) > number),
			_ => throw new ArgumentException(nameof(method)),
		};
	}
}