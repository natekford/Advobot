using Discord;

using System.Globalization;

namespace Advobot.Services.GuildSettings;

/// <summary>
/// Provides a role to mute a user.
/// </summary>
public interface IGuildSettingsService
{
	/// <summary>
	/// Gets a culture for the guild.
	/// </summary>
	/// <param name="guild"></param>
	/// <returns></returns>
	Task<CultureInfo> GetCultureAsync(IGuild guild);

	/// <summary>
	/// Gets a role to mute a user with.
	/// </summary>
	/// <param name="guild"></param>
	/// <returns></returns>
	Task<IRole> GetMuteRoleAsync(IGuild guild);

	/// <summary>
	/// Gets a prefix for the guild.
	/// </summary>
	/// <param name="guild"></param>
	/// <returns></returns>
	Task<string> GetPrefixAsync(IGuild guild);
}