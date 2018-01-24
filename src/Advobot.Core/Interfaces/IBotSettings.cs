using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Holds bot settings.
	/// </summary>
	public interface IBotSettings
	{
		//Saved settings
		IReadOnlyList<ulong> TrustedUsers { get; set; }
		IReadOnlyList<ulong> UsersUnableToDmOwner { get; set; }
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
		bool Pause { get; set; }

		/// <summary>
		/// Returns a string of all the bot's settings in a human readable format.
		/// </summary>
		/// <param name="client"></param>
		/// <returns></returns>
		Task<string> FormatAsync(IDiscordClient client);
		/// <summary>
		/// Returns a string of a bot setting in human readable format.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		Task<string> FormatAsync(IDiscordClient client, PropertyInfo property);
		/// <summary>
		/// Saves the settings to a json file.
		/// </summary>
		void SaveSettings();
	}
}
