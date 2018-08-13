using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for low level configuration in the bot.
	/// </summary>
	public interface ILowLevelConfig : ISettingsBase
	{
		/// <summary>
		/// The path the bot's files are in.
		/// </summary>
		string SavePath { get; set; }
		/// <summary>
		/// The id of the bot.
		/// </summary>
		ulong BotId { get; set; }
		/// <summary>
		/// Whether or not to always download users when joining a guild.
		/// </summary>
		bool AlwaysDownloadUsers { get; set; }
		/// <summary>
		/// How many messages to cache.
		/// </summary>
		int MessageCacheSize { get; set; }
		/// <summary>
		/// What level to log messages at in the console.
		/// </summary>
		LogSeverity LogLevel { get; set; }
		/// <summary>
		/// Whether the path is validated or not.
		/// </summary>
		bool ValidatedPath { get; }
		/// <summary>
		/// Whether the bot key is validated or not.
		/// </summary>
		bool ValidatedKey { get; }

		/// <summary>
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		bool ValidatePath(string input, bool startup);
		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <param name="restartCallback"></param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		Task<bool> ValidateBotKey(string input, bool startup, Func<ILowLevelConfig, BaseSocketClient, Task> restartCallback);
		/// <summary>
		/// Logs in and starts the client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		Task StartAsync(BaseSocketClient client);
		/// <summary>
		/// Sets the bot key to null.
		/// </summary>
		void ResetBotKey();
		/// <summary>
		/// Writes the current config to file.
		/// </summary>
		void SaveSettings();
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId
		/// </summary>
		/// <returns></returns>
		DirectoryInfo GetBaseBotDirectory();
		/// <summary>
		/// Assuming the save path is C:\Users\User\AppData\Roaming, returns C:\Users\User\AppData\Roaming\Discord_Servers_BotId\File
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		FileInfo GetBaseBotDirectoryFile(string fileName);
	}
}