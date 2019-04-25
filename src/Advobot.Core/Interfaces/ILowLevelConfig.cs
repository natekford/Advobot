using System;
using System.Threading.Tasks;
using Advobot.Classes.DatabaseWrappers;
using Discord.WebSocket;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for low level configuration in the bot.
	/// </summary>
	public interface ILowLevelConfig : IRestartArgumentProvider, IBotDirectoryAccessor
	{
		/// <summary>
		/// The id of the bot.
		/// </summary>
		ulong BotId { get; }
		/// <summary>
		/// The previous process id of the application. This is used with the .Net Framework version to make sure the old one is killed first when restarting.
		/// </summary>
		int PreviousProcessId { get; }
		/// <summary>
		/// The instance number of the bot at launch. This is used to find the correct config.
		/// </summary>
		int CurrentInstance { get; }
		/// <summary>
		/// The type of database to use for services which require a database.
		/// </summary>
		DatabaseType DatabaseType { get; }
		/// <summary>
		/// The string to connect to the database with.
		/// </summary>
		string DatabaseConnectionString { get; }
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
		bool ValidatePath(string? input, bool startup);
		/// <summary>
		/// Attempts to login with the given input. Returns a boolean signifying whether the login was successful or not.
		/// </summary>
		/// <param name="input">The bot key.</param>
		/// <param name="startup">Whether or not this should be treated as the first attempt at logging in.</param>
		/// <param name="restartCallback"></param>
		/// <returns>A boolean signifying whether the login was successful or not.</returns>
		Task<bool> ValidateBotKey(string? input, bool startup, Func<BaseSocketClient, IRestartArgumentProvider, Task> restartCallback);
		/// <summary>
		/// Logs in and starts the client.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		Task StartAsync(BaseSocketClient client);
	}
}