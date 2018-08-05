using System.IO;
using Advobot.Classes;

namespace Advobot.Utilities
{
	/// <summary>
	/// Actions for getting files related to the bot.
	/// </summary>
	public static class FileUtils
	{
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId
		/// </summary>
		/// <returns></returns>
		public static DirectoryInfo GetBaseBotDirectory(LowLevelConfig config)
		{
			return Directory.CreateDirectory(Path.Combine(config.SavePath, $"Discord_Servers_{config.BotId}"));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="config"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(LowLevelConfig config, string fileName)
		{
			return new FileInfo(Path.Combine(GetBaseBotDirectory(config).FullName, fileName));
		}
		/// <summary>
		/// Makes sure the directory and file exists then writes the text to the file.
		/// </summary>
		/// <param name="file"></param>
		/// <param name="text"></param>
		public static void SafeWriteAllText(FileInfo file, string text)
		{
			//Don't use file.Exists because the property sometimes isn't updated.
			if (!File.Exists(file.FullName))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(file.FullName));
				using (var fs = file.Create())
				{
					fs.Close();
				}
			}
			File.WriteAllText(file.FullName, text);
		}
	}
}
