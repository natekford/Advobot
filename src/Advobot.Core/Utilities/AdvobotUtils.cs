using System;
using System.IO;

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
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName">The name of the file without the bot directory.</param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
			=> new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));

		/// <summary>
		/// Gets the prefix to use for this guild. Prioritizes the guild prefix over the global prefix.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="botSettings"></param>
		/// <returns></returns>
		public static string GetPrefix(this IGuildSettings settings, IBotSettings botSettings)
			=> settings.Prefix ?? botSettings.Prefix ?? throw new InvalidOperationException("Invalid prefix.");

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
	}
}