using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Databases;
using Advobot.Databases.Abstract;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Settings;
using AdvorangesUtils;
using Discord;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
		private readonly IBotDirectoryAccessor _Accessor;

		/// <summary>
		/// Creates an instance of <see cref="GuildSettingsFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public GuildSettingsFactory(IServiceProvider provider) : base(provider)
		{
			_Accessor = provider.GetRequiredService<IBotDirectoryAccessor>();
		}

		protected override void AfterStart(int schema)
		{
			//Before setting this up, guild settings relied on JSON files for every guild
			if (schema < 1) //Relying on LiteDB
			{
				var path = Path.Combine(_Accessor.BaseBotDirectory.FullName, "GuildSettings");
				var settings = new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error,
				};
				var files = Directory.GetFiles(path);
				var instances = files.Select(x =>
				{
					var instance = IOUtils.DeserializeFromFile<GuildSettings>(new FileInfo(x), settings);
					instance.GuildId = ulong.Parse(Path.GetFileNameWithoutExtension(x));
					return instance;
				});

				DatabaseWrapper.ExecuteQuery(DatabaseQuery<GuildSettings>.Upsert(instances));
			}
			if (schema < 2) //Relying on LiteDB
			{
				var db = (LiteDatabase)DatabaseWrapper.UnderlyingDatabase;
				var col = db.GetCollection("GuildSettings");
				foreach (var doc in col.FindAll())
				{
					doc["CommandSettings"] = db.Mapper.ToDocument(new CommandSettings());
					col.Update(doc);
				}
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
			await instance.GetInviteCache().CacheInvitesAsync(guild).CAF();

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
