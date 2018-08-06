using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for low level configuration in the bot.
	/// </summary>
	public interface ILowLevelConfig
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
		/// Attempts to set the save path with the given input. Returns a boolean signifying whether the save path is valid or not.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="startup"></param>
		/// <returns></returns>
		bool ValidatePath(string input, bool startup);
		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="client">The client to login.</param>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		Task<bool> ValidateBotKey(DiscordShardedClient client, string input, bool startup);
		/// <summary>
		/// Sets the bot key to null.
		/// </summary>
		void ResetBotKey();
		/// <summary>
		/// Writes the current config to file.
		/// </summary>
		void Save();
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