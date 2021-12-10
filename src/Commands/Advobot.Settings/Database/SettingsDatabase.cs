using Advobot.Settings.Models;
using Advobot.SQLite;

using AdvorangesUtils;

using System.Data.SQLite;

namespace Advobot.Settings.Database;

public sealed class SettingsDatabase : DatabaseBase<SQLiteConnection>, ISettingsDatabase
{
	public SettingsDatabase(IConnectionStringFor<SettingsDatabase> conn) : base(conn)
	{
	}

	public Task<int> DeleteCommandOverridesAsync(IEnumerable<CommandOverride> overrides)
	{
		return BulkModifyAsync(@"
				DELETE FROM CommandOverride
				WHERE GuildId = @GuildId AND CommandId = @CommandId AND TargetId = @TargetId
			", overrides);
	}

	public async Task<IReadOnlyList<CommandOverride>> GetCommandOverridesAsync(
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
				ORDER BY Priority DESC, TargetType ASC
			", param).CAF();
	}

	public async Task<IReadOnlyList<CommandOverride>> GetCommandOverridesAsync(
		ulong guildId)
	{
		var param = new { GuildId = guildId.ToString() };
		return await GetManyAsync<CommandOverride>(@"
				SELECT * FROM CommandOverride
				WHERE GuildId = @GuildId
				ORDER BY CommandId ASC, Priority DESC, TargetType ASC
			", param).CAF();
	}

	public async Task<GuildSettings> GetGuildSettingsAsync(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString() };
		return await GetOneAsync<GuildSettings>(@"
				SELECT * FROM GuildSetting
				WHERE GuildId = @GuildId
			", param).CAF() ?? new GuildSettings { GuildId = guildId };
	}

	public Task<int> UpsertCommandOverridesAsync(IEnumerable<CommandOverride> overrides)
	{
		return BulkModifyAsync(@"
				INSERT OR IGNORE INTO CommandOverride
					( GuildId, CommandId, TargetId, TargetType, Enabled, Priority )
					VALUES
					( @GuildId, @CommandId, @TargetId, @TargetType, @Enabled, @Priority );
				UPDATE CommandOverride
				SET
					Enabled = @Enabled,
					Priority = @Priority
				WHERE GuildId = @GuildId AND CommandId = @CommandId AND TargetId = @TargetId
			", overrides);
	}

	public Task<int> UpsertGuildSettingsAsync(GuildSettings settings)
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