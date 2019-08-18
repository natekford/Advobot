using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Settings;

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
		/// Ensures the extension of the file is '.db' and that the directory exists.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileNameParts"></param>
		/// <returns></returns>
		public static FileInfo ValidateDbPath(IBotDirectoryAccessor accessor, params string[] fileNameParts)
		{
			static void ExtensionValidation(ref string fileName)
			{
				const string EXT = ".db";
				if (!Path.HasExtension(fileName))
				{
					fileName += EXT;
				}
				else if (Path.GetExtension(fileName) != EXT)
				{
					fileName = Path.GetFileNameWithoutExtension(fileName) + EXT;
				}
			}
			ExtensionValidation(ref fileNameParts[^1]);

			var relativePath = Path.Combine(fileNameParts);
			var absolutePath = accessor.GetBaseBotDirectoryFile(relativePath).FullName;
			//Make sure the directory the db will be created in exists
			Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
			return new FileInfo(absolutePath);
		}
		/// <summary>
		/// Sets <see cref="ValueTask{TResult}.ConfigureAwait(bool)"/> to false.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="valueTask"></param>
		/// <returns></returns>
		public static ConfiguredValueTaskAwaitable<T> CAF<T>(this ValueTask<T> valueTask)
			=> valueTask.ConfigureAwait(false);
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
			_ => throw new ArgumentOutOfRangeException(nameof(method)),
		};
	}
}