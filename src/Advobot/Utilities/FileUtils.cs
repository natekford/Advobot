﻿using System.IO;

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
		public static DirectoryInfo GetBaseBotDirectory()
		{
			return Directory.CreateDirectory(Path.Combine(LowLevelConfig.Config.SavePath, $"Discord_Servers_{LowLevelConfig.Config.BotId}"));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(string fileName)
		{
			return new FileInfo(Path.Combine(GetBaseBotDirectory().FullName, fileName));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\GuildSettings\GuildId.json
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static FileInfo GetGuildSettingsFile(ulong id)
		{
			return GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{id}.json"));
		}
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\BotSettings.json
		/// </summary>
		/// <returns></returns>
		public static FileInfo GetBotSettingsFile()
		{
			return GetBaseBotDirectoryFile("BotSettings.json");
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