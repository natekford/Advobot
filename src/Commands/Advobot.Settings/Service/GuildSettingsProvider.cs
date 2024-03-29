﻿using Advobot.Services.BotSettings;
using Advobot.Services.GuildSettingsProvider;
using Advobot.Settings.Database;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

using System.Globalization;

namespace Advobot.Settings.Service;

public class GuildSettingsProvider(ISettingsDatabase db, IBotSettings settings) : IGuildSettingsProvider
{
	private const string NAME = "Advobot_Mute";

	private static readonly RequestOptions _RoleCreation = new()
	{
		AuditLogReason = "Role not found or is higher than my highest role.",
	};

	private readonly ISettingsDatabase _Db = db;
	private readonly IBotSettings _Settings = settings;

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

		var newRole = await guild.CreateEmptyRoleAsync(NAME, _RoleCreation);
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