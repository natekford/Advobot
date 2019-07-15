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

		private readonly ConcurrentDictionary<ulong, IGuildSettings> _GuildSettings = new ConcurrentDictionary<ulong, IGuildSettings>();
		private readonly IBotSettings _BotSettings;

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

			BsonMapper.Global.Entity<GuildSettings>()
				.Id(x => x.GuildId)
				.Ignore(x => x.SettingNames)
				.Ignore(x => x.EvaluatedRegex)
				.Ignore(x => x.MessageDeletion)
				.Ignore(x => x.CachedInvites)
				.Ignore(x => x.BannedPhraseUsers);

			BsonMapper.Global.Entity<DatabaseMetadata>()
				.Id(x => x.ProgramVersion);

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
		public Task<IGuildSettings> GetOrCreateAsync(IGuild guild)
		{
			var query = DatabaseQuery<GuildSettings>.Get(x => x.GuildId == guild.Id);
			var instance = DatabaseWrapper.ExecuteQuery(query).SingleOrDefault();
			if (instance == null)
			{
				instance = new GuildSettings
				{
					GuildId = guild.Id,
				};
			}

			foreach (var group in instance.SelfAssignableGroups)
			{
				group.RemoveRoles(group.Roles.Where(x => guild.GetRole(x) == null));
			}

			DatabaseWrapper.ExecuteQuery(DatabaseQuery<GuildSettings>.Update(new[] { instance }));
			return Task.FromResult<IGuildSettings>(instance);
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
