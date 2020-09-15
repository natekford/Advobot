using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.Settings.Models;
using Advobot.Settings.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.Settings.Database
{
	public sealed class SettingsDatabase : DatabaseBase<SQLiteConnection>, ISettingsDatabase
	{
		public SettingsDatabase(IConnectionStringFor<SettingsDatabase> conn) : base(conn)
		{
		}

		public async Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(
			ulong guildId,
			string commandId)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				CommandId = commandId
			};
			return await GetManyAsync<CommandOverride>(@"
				SELECT * FROM CommandOverride
				WHERE GuildId = @GuildId AND CommandId = @CommandId
				ORDER BY Position DESC
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyCommandOverride>> GetCommandOverridesAsync(
			ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<CommandOverride>(@"
				SELECT * FROM CommandOverride
				WHERE GuildId = @GuildId
				ORDER BY CommandId ASC, Position DESC
			", param).CAF();
		}

		public async Task<IReadOnlyGuildSettings> GetGuildSettingsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetOneAsync<GuildSettings>(@"
				SELECT * FROM GuildSetting
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public Task<int> UpsertGuildSettingsAsync(IReadOnlyGuildSettings settings)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO GuildSetting
					( GuildId )
					VALUES
					( @GuildId );
				UPDATE GuildSetting
				SET
					MuteRoleId = @MuteRoleId,
					Prefix = @Prefix,
					Culture = @Culture
				WHERE GuildId = @GuildId
			", settings);
		}
	}
}