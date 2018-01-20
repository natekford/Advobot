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
		private ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();

		public GuildSettingsHolder(IServiceProvider provider) { }

		public Task Remove(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out var value))
			{
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", color: ConsoleColor.Red);
			}
			return Task.FromResult(0);
		}
		public Task<IGuildSettings> GetOrCreate(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}

			if (!_GuildSettings.TryGetValue(guild.Id, out var settings) &&
				!_GuildSettings.TryAdd(guild.Id, settings = ServiceProviderCreationUtils.CreateGuildSettings(guild as SocketGuild)))
			{
				ConsoleUtils.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", color: ConsoleColor.Red);
			}
			return Task.FromResult(settings);
		}
		public IEnumerable<IGuildSettings> GetAll()
		{
			return _GuildSettings.Values;
		}

		public bool TryGet(ulong guildId, out IGuildSettings settings)
		{
			return _GuildSettings.TryGetValue(guildId, out settings);
		}

		public bool Contains(ulong guildId)
		{
			return _GuildSettings.ContainsKey(guildId);
		}
	}
}
