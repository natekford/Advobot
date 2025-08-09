using Advobot.Services.BotSettings;
using Advobot.Utilities;

using Discord;

using System.Globalization;

namespace Advobot.Services.GuildSettings;

[Replacable]
internal sealed class NaiveGuildSettingsService(IRuntimeConfig settings) : IGuildSettingsService
{
	private const string NAME = "Advobot_Mute";
	private static readonly RequestOptions RoleCreation = new()
	{
		AuditLogReason = "Role not found or is higher than my highest role.",
	};
	private readonly IRuntimeConfig _Settings = settings;

	public Task<CultureInfo> GetCultureAsync(IGuild guild)
		=> Task.FromResult(guild.PreferredCulture);

	public async Task<IRole> GetMuteRoleAsync(IGuild guild)
	{
		var bot = await guild.GetCurrentUserAsync().ConfigureAwait(false);
		foreach (var role in guild.Roles)
		{
			if (role.Name == NAME && bot.CanModify(role))
			{
				return role;
			}
		}
		return await guild.CreateEmptyRoleAsync(NAME, RoleCreation).ConfigureAwait(false);
	}

	public Task<string> GetPrefixAsync(IGuild guild)
		=> Task.FromResult(_Settings.Prefix);
}