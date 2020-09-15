using System.Globalization;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Services.GuildSettingsProvider
{
	/// <summary>
	/// Provides a role to mute a user.
	/// </summary>
	public interface IGuildSettingsProvider
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
}