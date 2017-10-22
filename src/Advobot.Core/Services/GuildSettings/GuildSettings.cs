﻿using Advobot.Core.Actions;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Core.Services.GuildSettings
{
	internal sealed class GuildSettingsHolder : IGuildSettingsService
	{
		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();

		public GuildSettingsHolder(IServiceProvider provider) { }

		public Task RemoveGuild(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out var value))
			{
				ConsoleActions.WriteLine($"Failed to remove {guildId} from the guild settings holder.", color: ConsoleColor.Red);
			}
			return Task.FromResult(0);
		}
		public async Task<IGuildSettings> GetOrCreateSettings(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}

			if (!_GuildSettings.TryGetValue(guild.Id, out var settings) &&
				!_GuildSettings.TryAdd(guild.Id, settings = await CreationActions.CreateGuildSettingsAsync(guild).CAF()))
			{
				ConsoleActions.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", color: ConsoleColor.Red);
			}
			return settings;
		}
		public IEnumerable<IGuildSettings> GetAllSettings()
		{
			return _GuildSettings.Values;
		}
		public bool TryGetSettings(ulong guildId, out IGuildSettings settings)
		{
			return _GuildSettings.TryGetValue(guildId, out settings);
		}
		public bool ContainsGuild(ulong guildId)
		{
			return _GuildSettings.ContainsKey(guildId);
		}
	}
}