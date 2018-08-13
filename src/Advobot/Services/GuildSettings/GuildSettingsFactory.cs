using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Handles guild setting creation and storage.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class GuildSettingsFactory<T> : IGuildSettingsService where T : class, IGuildSettings, new()
	{
		/// <inheritdoc />
		public Type GuildSettingsType => typeof(T);

		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly IBotSettings _Settings;

		/// <summary>
		/// Creates an instance of <see cref="GuildSettingsFactory{T}"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildSettingsFactory(IIterableServiceProvider provider)
		{
			_Settings = provider.GetRequiredService<IBotSettings>();
		}

		/// <inheritdoc />
		public Task RemoveAsync(ulong guildId)
		{
			if (_GuildSettings.ContainsKey(guildId) && !_GuildSettings.TryRemove(guildId, out _))
			{
				ConsoleUtils.WriteLine($"Failed to remove {guildId} from the guild settings holder.", ConsoleColor.Yellow);
			}
			return Task.CompletedTask;
		}
		/// <inheritdoc />
		public async Task<IGuildSettings> GetOrCreateAsync(SocketGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			settings = Classes.GuildSettings.Load(_Settings, guild.Id);
			await settings.PostDeserializeAsync(guild).CAF();
			settings.SaveSettings(_Settings);

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
	}
}
