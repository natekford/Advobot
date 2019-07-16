using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Services.GuildSettings
{
	/// <summary>
	/// Handles guild setting creation and storage.
	/// </summary>
	internal sealed class GuildSettingsFactory : DatabaseWrapperConsumer, IGuildSettingsFactory
	{
		/// <inheritdoc />
		public Type GuildSettingsType => typeof(GuildSettings);
		/// <inheritdoc />
		public override string DatabaseName => "GuildSettings";

		private readonly ConcurrentDictionary<ulong, IGuildSettings> _Cache = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly IBotSettings _BotSettings;

		static GuildSettingsFactory()
		{
			BsonMapper.Global.Entity<GuildSettings>()
				.Id(x => x.GuildId)
				.Ignore(x => x.SettingNames)
				.Ignore(x => x.EvaluatedRegex)
				.Ignore(x => x.MessageDeletion)
				.Ignore(x => x.CachedInvites)
				.Ignore(x => x.BannedPhraseUsers);
			BsonMapper.Global.Entity<DatabaseMetadata>()
				.Id(x => x.ProgramVersion);
		}

		/// <summary>
		/// Creates an instance of <see cref="GuildSettingsFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildSettingsFactory(IServiceProvider provider) : base(provider)
		{
			_BotSettings = provider.GetRequiredService<IBotSettings>();
		}

		protected override void AfterStart(int schema)
		{
			//Before setting this up, guild settings relied on JSON files for every guild
			if (schema < 1) //Relying on LiteDB
			{
				var path = Path.Combine(_BotSettings.BaseBotDirectory.FullName, "GuildSettings");
				var files = Directory.GetFiles(path);
				var instances = files.Select(x =>
				{
					var instance = IOUtils.DeserializeFromFile<GuildSettings>(new FileInfo(x));
					instance.GuildId = ulong.Parse(Path.GetFileNameWithoutExtension(x));
					return instance;
				});

				DatabaseWrapper.ExecuteQuery(DatabaseQuery<GuildSettings>.Upsert(instances));
			}
		}

		/// <inheritdoc />
		public async Task<IGuildSettings> GetOrCreateAsync(IGuild guild)
		{
			if (_Cache.TryGetValue(guild.Id, out var cached))
			{
				return cached;
			}

			var query = DatabaseQuery<GuildSettings>.Get(x => x.GuildId == guild.Id);
			var instance = DatabaseWrapper.ExecuteQuery(query).SingleOrDefault();
			if (instance == null)
			{
				instance = new GuildSettings
				{
					GuildId = guild.Id,
				};
				DatabaseWrapper.ExecuteQuery(DatabaseQuery<GuildSettings>.Update(new[] { instance }));
			}
			instance.StoreGuildSettingsFactory(this);

			foreach (var invite in await guild.SafeGetInvitesAsync().CAF())
			{
				instance.CachedInvites.Add(new CachedInvite(invite));
			}

			_Cache.TryAdd(guild.Id, instance);
			return instance;
		}
		/// <inheritdoc />
		public Task RemoveAsync(ulong guildId)
		{
			_Cache.TryRemove(guildId, out _);

			var query = DatabaseQuery<GuildSettings>.Delete(x => x.GuildId == guildId);
			DatabaseWrapper.ExecuteQuery(query);

			return Task.CompletedTask;
		}
		/// <inheritdoc />
		public bool TryGet(ulong guildId, out IGuildSettings settings)
			=> _Cache.TryGetValue(guildId, out settings);
		/// <summary>
		/// Saves any changes done to <paramref name="settings"/> to the backing database.
		/// </summary>
		/// <param name="settings"></param>
		public void Save(GuildSettings settings)
		{
			var query = DatabaseQuery<GuildSettings>.Update(new[] { settings });
			DatabaseWrapper.ExecuteQuery(query);
		}
	}
}
