using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Modules.GuildSettings
{
	public sealed class GuildSettingsHolder : IGuildSettingsModule
	{
		private readonly Dictionary<ulong, IGuildSettings> _GuildSettings = new Dictionary<ulong, IGuildSettings>();

		public GuildSettingsHolder(IServiceProvider provider)
		{

		}

		public Task RemoveGuild(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId))
			{
				_GuildSettings.Remove(guildId);
			}
			return Task.FromResult(0);
		}
		public async Task<IGuildSettings> GetOrCreateSettings(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}

			if (!_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				_GuildSettings.Add(guild.Id, settings = await CreationActions.CreateGuildSettings(guild));
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
