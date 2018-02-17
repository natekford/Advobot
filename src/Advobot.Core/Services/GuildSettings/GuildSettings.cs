using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Core.Services.GuildSettings
{
	internal sealed class GuildSettingsService : IGuildSettingsService
	{
		private ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		public Type GuildSettingsType { get; }

		public GuildSettingsService(Type guildSettingsType, IServiceProvider provider)
		{
			if (!typeof(IGuildSettings).IsAssignableFrom(guildSettingsType))
			{
				throw new ArgumentException($"Must inherit {nameof(IGuildSettings)}.", nameof(guildSettingsType));
			}

			GuildSettingsType = guildSettingsType;
		}

		public void Remove(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out _))
			{
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", ConsoleColor.Yellow);
			}
		}
		public IGuildSettings GetOrCreate(IGuild guild)
		{
			if (guild == null)
			{
				return null;
			}
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			if (!_GuildSettings.TryAdd(guild.Id, settings = CreationUtils.CreateGuildSettings(GuildSettingsType, guild)))
			{
				ConsoleUtils.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", ConsoleColor.Yellow);
			}
			return settings;
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

		Task IGuildSettingsService.RemoveAsync(ulong guildId)
		{
			Remove(guildId);
			return Task.FromResult(0);
		}
		Task<IGuildSettings> IGuildSettingsService.GetOrCreateAsync(IGuild guild)
		{
			return Task.FromResult(GetOrCreate(guild));
		}
	}
}
