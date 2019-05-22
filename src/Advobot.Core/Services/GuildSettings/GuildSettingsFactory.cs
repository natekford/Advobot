using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Handles guild setting creation and storage.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class GuildSettingsFactory<T> : IGuildSettingsFactory where T : IGuildSettings
	{
		/// <inheritdoc />
		public Type GuildSettingsType => typeof(T);

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
		public async Task<IGuildSettings> GetOrCreateAsync(IGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			var concrete = GuildSettings.Load(_Settings, guild);
			if (concrete == null)
			{
				concrete = new GuildSettings();
				concrete.Save(_Settings);
			}

			await concrete.PostDeserializeAsync(guild).CAF();
			_GuildSettings.TryAdd(guild.Id, concrete);
			return concrete;
		}
		/// <inheritdoc />
		public Task RemoveAsync(ulong guildId)
		{
			_GuildSettings.TryRemove(guildId, out _);
			return Task.CompletedTask;
		}
		/// <inheritdoc />
		public bool TryGet(ulong guildId, out IGuildSettings settings)
			=> _GuildSettings.TryGetValue(guildId, out settings);
	}
}
