using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettings;
using Advobot.Settings.Database;
using Advobot.Utilities;

using Discord;

using System.Globalization;

namespace Advobot.Settings.Service;

public class GuildSettingsProvider(ISettingsDatabase db, IRuntimeConfig settings) : IGuildSettingsService
{
	private const string NAME = "Advobot_Mute";

	private static readonly RequestOptions _RoleCreation = new()
	{
		AuditLogReason = "Role not found or is higher than my highest role.",
	};

	private readonly ISettingsDatabase _Db = db;
	private readonly IRuntimeConfig _Settings = settings;

	public async Task<CultureInfo> GetCultureAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).ConfigureAwait(false);
		if (settings.Culture != null)
		{
			return CultureInfo.GetCultureInfo(settings.Culture);
		}
		return guild.PreferredCulture;
	}

	public async Task<IRole> GetMuteRoleAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).ConfigureAwait(false);
		if (settings.MuteRoleId != 0)
		{
			foreach (var role in guild.Roles)
			{
				if (role.Id == settings.MuteRoleId)
				{
					return role;
				}
			}
		}

		var newRole = await guild.CreateEmptyRoleAsync(NAME, _RoleCreation);
		await _Db.UpsertGuildSettingsAsync(settings with
		{
			MuteRoleId = newRole.Id,
		}).ConfigureAwait(false);
		return newRole;
	}

	public async Task<string> GetPrefixAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).ConfigureAwait(false);
		return settings.Prefix ?? _Settings.Prefix;
	}
}