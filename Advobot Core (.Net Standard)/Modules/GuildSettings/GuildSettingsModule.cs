using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Modules.GuildSettings
{
	public sealed class MyGuildSettingsModule : IGuildSettingsModule
	{
		private readonly Dictionary<ulong, IGuildSettings> _GuildSettings = new Dictionary<ulong, IGuildSettings>();

		public async Task AddGuild(IGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out IGuildSettings guildSettings))
			{
				return;
			}

			_GuildSettings.Add(guild.Id, await CreationActions.CreateGuildSettings(guild));
		}
		public Task RemoveGuild(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId))
			{
				_GuildSettings.Remove(guildId);
			}
			return Task.FromResult(0);
		}
		public IGuildSettings GetSettings(ulong guildId)
		{
			return _GuildSettings[guildId];
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
