using Advobot.Core.Utilities;
using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;

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
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", color: ConsoleColor.Red);
			}
			return Task.FromResult(0);
		}
		public Task<IGuildSettings> GetOrCreateSettings(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}

			if (!_GuildSettings.TryGetValue(guild.Id, out var settings) &&
				!_GuildSettings.TryAdd(guild.Id, settings = ServiceProviderCreationUtils.CreateGuildSettingsAsync(guild as SocketGuild)))
			{
				ConsoleUtils.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", color: ConsoleColor.Red);
			}
			return Task.FromResult(settings);
		}
		public IEnumerable<IGuildSettings> GetAllSettings() => _GuildSettings.Values;
		public bool TryGetSettings(ulong guildId, out IGuildSettings settings) => _GuildSettings.TryGetValue(guildId, out settings);
		public bool ContainsGuild(ulong guildId) => _GuildSettings.ContainsKey(guildId);
	}
}
