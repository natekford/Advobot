using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Settings.Database;

using AdvorangesUtils;

using Discord;

using System.Globalization;

namespace Advobot.Settings.Service;

public class GuildSettingsProvider : IGuildSettingsProvider
{
	private const string NAME = "Advobot_Mute";
	private static readonly RequestOptions RoleCreation = new()
	{
		AuditLogReason = "Role not found or is higher than my highest role.",
	};
	private readonly ISettingsDatabase _Db;
	private readonly IBotSettings _Settings;

	public GuildSettingsProvider(ISettingsDatabase db, IBotSettings settings)
	{
		_Db = db;
		_Settings = settings;
	}

	public async Task<CultureInfo> GetCultureAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).CAF();
		if (settings.Culture != null)
		{
			return CultureInfo.GetCultureInfo(settings.Culture);
		}
		return guild.PreferredCulture;
	}

	public async Task<IRole> GetMuteRoleAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).CAF();
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

		var newRole = await guild.CreateRoleAsync(
			name: NAME,
			permissions: GuildPermissions.None,
			color: null,
			isHoisted: false,
			isMentionable: false,
			options: RoleCreation
		).CAF();
		await _Db.UpsertGuildSettingsAsync(settings with
		{
			MuteRoleId = newRole.Id,
		}).CAF();
		return newRole;
	}

	public async Task<string> GetPrefixAsync(IGuild guild)
	{
		var settings = await _Db.GetGuildSettingsAsync(guild.Id).CAF();
		return settings.Prefix ?? _Settings.Prefix;
	}
}