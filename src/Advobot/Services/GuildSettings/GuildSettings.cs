using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Advobot.Services.GuildSettings
{
	internal sealed class GuildSettingsService<T> : IGuildSettingsService where T : IGuildSettings, new()
	{
		public Type GuildSettingsType => typeof(T);

		private ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();

		public GuildSettingsService(IServiceProvider provider) { }

		/// <inheritdoc />
		public void Remove(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out _))
			{
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", ConsoleColor.Yellow);
			}
		}
		/// <inheritdoc />
		public IGuildSettings GetOrCreate(IGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			var jsonSettings = new JsonSerializerSettings()
			{
				Converters = new[] { new StringEnumConverter() },
				Context = new StreamingContext(StreamingContextStates.Other, guild),
			};
			settings = IOUtils.DeserializeFromFile<IGuildSettings, T>(FileUtils.GetGuildSettingsFile(guild.Id), jsonSettings);

			if (!_GuildSettings.TryAdd(guild.Id, settings))
			{
				ConsoleUtils.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", ConsoleColor.Yellow);
			}
			return settings;
		}
		/// <inheritdoc />
		public IEnumerable<IGuildSettings> GetAll()
		{
			return _GuildSettings.Values;
		}
		/// <inheritdoc />
		public bool TryGet(ulong guildId, out IGuildSettings settings)
		{
			return _GuildSettings.TryGetValue(guildId, out settings);
		}
		/// <inheritdoc />
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
