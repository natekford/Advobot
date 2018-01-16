using Discord;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Holds bot settings.
	/// </summary>
	public interface IBotSettings
	{
		//Saved settings
		IReadOnlyList<ulong> TrustedUsers { get; set; }
		IReadOnlyList<ulong> UsersUnableToDMOwner { get; set; }
		IReadOnlyList<ulong> UsersIgnoredFromCommands { get; set; }
		int ShardCount { get; set; }
		int MessageCacheCount { get; set; }
		int MaxUserGatherCount { get; set; }
		int MaxMessageGatherSize { get; set; }
		string Prefix { get; set; }
		string Game { get; set; }
		string Stream { get; set; }
		bool AlwaysDownloadUsers { get; set; }
		LogSeverity LogLevel { get; set; }

		//Non-saved settings
		bool Pause { get; }

		/// <summary>
		/// Saves the settings to a JSON file.
		/// </summary>
		void SaveSettings();
		/// <summary>
		/// Switches the value of <see cref="Pause"/>.
		/// </summary>
		void TogglePause();
		/// <summary>
		/// Returns <see cref="Int32.MaxValue"/> is bypass is true, otherwise returns whatever botSettings has for MaxUserGatherCount.
		/// </summary>
		/// <param name="bypass"></param>
		/// <returns></returns>
		int GetMaxAmountOfUsersToGather(bool bypass);

		/// <summary>
		/// Returns a string of all the bot's settings in a human readable format.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		Task<string> Format(IDiscordClient client);
		/// <summary>
		/// Returns a string of a bot setting in human readable format.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		Task<string> Format(IDiscordClient client, PropertyInfo property);
	}
}
