using Advobot.Logging.Database.Models;
using Advobot.SQLite;

using Dapper;

using System.Data;
using System.Data.SQLite;

namespace Advobot.Logging.Database;

public sealed class LoggingDatabase(IConnectionString<LoggingDatabase> connection)
	: DatabaseBase<SQLiteConnection>(connection)
{
	public Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
	{
		var @params = channels.Select(x => new
		{
			GuildId = guildId.ToString(),
			ChannelId = x.ToString()
		});
		return BulkModifyAsync(@"
			INSERT OR IGNORE INTO IgnoredChannel
			( GuildId, ChannelId )
			VALUES
			( @GuildId, @ChannelId )
		", @params);
	}

	public Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
	{
		var @params = actions.Select(x => new
		{
			GuildId = guildId.ToString(),
			Action = x.ToString()
		});
		return BulkModifyAsync(@"
			INSERT OR IGNORE INTO LogAction
			( GuildId, Action )
			VALUES
			( @GuildId, @Action )
		", @params);
	}

	public Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
	{
		var @params = channels.Select(x => new
		{
			GuildId = guildId.ToString(),
			ChannelId = x.ToString()
		});
		return BulkModifyAsync(@"
			DELETE FROM IgnoredChannel
			WHERE GuildId = @GuildId AND ChannelId = @ChannelId
		", @params);
	}

	public Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
	{
		var @params = actions.Select(x => new
		{
			GuildId = guildId.ToString(),
			Action = x.ToString()
		});
		return BulkModifyAsync(@"
			DELETE FROM LogAction
			WHERE GuildId = @GuildId AND Action = @Action
		", @params);
	}

	public async Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId)
	{
		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

		var param = new { GuildId = guildId.ToString() };
		var result = await connection.QueryAsync<string>(@"
			SELECT ChannelId
			FROM IgnoredChannel
			WHERE GuildId = @GuildId
		", param).ConfigureAwait(false);
		return [.. result.Select(ulong.Parse)];
	}

	public async Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId)
	{
		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

		var param = new { GuildId = guildId.ToString() };
		var result = await connection.QueryAsync<string>(@"
			SELECT Action
			FROM LogAction
			WHERE GuildId = @GuildId
		", param).ConfigureAwait(false);
		return [.. result.Where(x => x != null).Select(Enum.Parse<LogAction>)];
	}

	public async Task<LogChannels> GetLogChannelsAsync(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString() };
		return await GetOneAsync<LogChannels>(@"
			SELECT ImageLogId, ModLogId, ServerLogId
			FROM LogChannel
			WHERE GuildId = @GuildId
		", param).ConfigureAwait(false) ?? new LogChannels();
	}

	public Task<int> UpsertLogChannelAsync(Log log, ulong guildId, ulong? channelId)
	{
		var name = GetLogName(log);
		var param = new { GuildId = guildId.ToString(), ChannelId = channelId?.ToString() };
		return ModifyAsync($@"
			INSERT OR IGNORE INTO LogChannel
				( GuildId, {name} )
				VALUES
				( @GuildId, @ChannelId );
			UPDATE LogChannel
			SET
				{name} = @ChannelId
			WHERE GuildId = @GuildId
		", param);
	}

	private string GetLogName(Log log) => log switch
	{
		Log.Image => "ImageLogId",
		Log.Mod => "ModLogId",
		Log.Server => "ServerLogId",
		_ => throw new ArgumentOutOfRangeException(nameof(log))
	};
}