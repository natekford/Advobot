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
	internal sealed class GuildSettingsFactory : IGuildSettingsFactory
	{
		/// <inheritdoc />
		public Type GuildSettingsType => typeof(GuildSettings);

		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly IBotSettings _BotSettings;

		/// <summary>
		/// Creates an instance of <see cref="GuildSettingsFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildSettingsFactory(IServiceProvider provider)
		{
			_BotSettings = provider.GetRequiredService<IBotSettings>();
		}

		/// <inheritdoc />
		public async Task<IGuildSettings> GetOrCreateAsync(IGuild guild)
		{
			if (_GuildSettings.TryGetValue(guild.Id, out var settings))
			{
				return settings;
			}

			var concrete = GuildSettings.Load(_BotSettings, guild);
			if (concrete == null)
			{
				concrete = new GuildSettings();
				concrete.Save(_BotSettings);
			}

			await concrete.PostDeserializeAsync(_BotSettings, guild).CAF();
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
