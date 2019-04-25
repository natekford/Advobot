using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
	internal sealed class GuildSettingsFactory<T> : IGuildSettingsFactory where T : class, IGuildSettings, new()
	{
		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly IBotSettings _Settings;

		/// <summary>
		/// Creates an instance of <see cref="GuildSettingsFactory{T}"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildSettingsFactory(IServiceProvider provider)
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

			var concrete = GuildSettings.Load(_Settings, guild.Id);
			await concrete.PostDeserializeAsync(guild).CAF();

			settings = concrete;
			settings.Save(_Settings);

			if (!_GuildSettings.TryAdd(guild.Id, settings))
			{
				ConsoleUtils.WriteLine($"Failed to add {guild.Id} to the guild settings holder.", ConsoleColor.Yellow);
			}
			return settings;
		}
		/// <inheritdoc />
		public IEnumerable<IGuildSettings> GetAll()
			=> _GuildSettings.Values;
		/// <inheritdoc />
		public bool TryGet(ulong guildId, out IGuildSettings settings)
			=> _GuildSettings.TryGetValue(guildId, out settings);
		/// <inheritdoc />
		public bool Contains(ulong guildId)
			=> _GuildSettings.ContainsKey(guildId);
		/// <inheritdoc />
		public DirectoryInfo GetDirectory(IBotDirectoryAccessor accessor)
			=> new DirectoryInfo(Path.Combine(accessor.BaseBotDirectory.FullName, "GuildSettings"));
	}
}
