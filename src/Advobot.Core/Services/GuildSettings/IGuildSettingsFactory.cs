using System;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Abstraction for a guild settings module. Handles containing guild settings and adding/removing them.
	/// </summary>
	public interface IGuildSettingsFactory
	{
		/// <summary>
		/// The type of settings this factory is making.
		/// </summary>
		Type GuildSettingsType { get; }

		/// <summary>
		/// If the given guild is already in the module this will return its settings.
		/// Otherwise it will create them, add them to the module, then return them.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		Task<IGuildSettings> GetOrCreateAsync(IGuild guild);

		/// <summary>
		/// Removes the given guild's settings from the module.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		Task RemoveAsync(ulong guildId);

		/// <summary>
		/// Attempts to get the guild settings with the passed in id.
		/// </summary>
		/// <param name="guildId"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		bool TryGet(ulong guildId, out IGuildSettings settings);
	}
}