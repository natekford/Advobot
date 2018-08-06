using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Advobot.Services.GuildSettings
{
	internal sealed class GuildSettingsService<T> : IGuildSettingsService where T : IGuildSettings, new()
	{
		private static readonly JsonSerializerSettings _JsonSettings = new JsonSerializerSettings() { Converters = new[] { new StringEnumConverter() }, };

		/// <inheritdoc />
		public Type GuildSettingsType => typeof(T);

		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly ILowLevelConfig _Config;

		public GuildSettingsService(IServiceProvider provider)
		{
			_Config = provider.GetRequiredService<ILowLevelConfig>();
		}

		/// <inheritdoc />
		public void Remove(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out _))
			{
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", ConsoleColor.Yellow);
			}
		}
		/// <inheritdoc />
		public async Task<IGuildSettings> GetOrCreateAsync(SocketGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			var path = _Config.GetBaseBotDirectoryFile(Path.Combine("GuildSettings", $"{guild.Id}.json"));
			settings = IOUtils.DeserializeFromFile<IGuildSettings, T>(path, _JsonSettings);
			await settings.PostDeserializeAsync(guild).CAF();

			if (!path.Exists)
			{
				settings.SaveSettings(_Config);
			}
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
			return Task.CompletedTask;
		}
	}
}
