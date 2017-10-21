using Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// Abstraction for a guild settings module. Handles containing guild settings and adding/removing them.
	/// </summary>
	public interface IGuildSettingsService
	{
		/// <summary>
		/// If the given guild is already in the module this will return its settings.
		/// Otherwise it will create them, add them to the module, then return them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task<IGuildSettings> GetOrCreateSettings(IGuild guild);
		/// <summary>
		/// Removes the given guild's settings from the module.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task RemoveGuild(ulong guildId);
		/// <summary>
		/// Returns all of the settings in the module.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IGuildSettings> GetAllSettings();
		/// <summary>
		/// Attempts to get the guild settings with the passed in id.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		bool TryGetSettings(ulong guildId, out IGuildSettings settings);
		/// <summary>
		/// Checks if a guild has settings.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		bool ContainsGuild(ulong guildId);
	}
}
