using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Logging.Database
{
	public sealed class LoggingDatabase : DatabaseBase<SQLiteConnection>
	{
		public LoggingDatabase(ILoggingDatabaseStarter starter) : base(starter)
		{
		}

		public async Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
		{
			const string SQL = @"
				INSERT OR REPLACE INTO IgnoredChannel
				( GuildId, ChannelId )
				VALUES
				( @GuildId, @ChannelId )
			";
			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			return await BulkModifyAsync(SQL, @params).CAF();
		}

		public async Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
		{
			const string SQL = @"
				INSERT OR REPLACE INTO LogAction
				( GuildId, Action )
				VALUES
				( @GuildId, @Action )
			";
			var @params = actions.Select(x => new
			{
				GuildId = guildId.ToString(),
				Action = x.ToString()
			});
			return await BulkModifyAsync(SQL, @params).CAF();
		}

		public async Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
		{
			const string SQL = @"
				DELETE FROM IgnoredChannel
				WHERE GuildId = @GuildId AND ChannelId = @ChannelId
			";
			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			return await BulkModifyAsync(SQL, @params).CAF();
		}

		public async Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
		{
			const string SQL = @"
				DELETE FROM LogAction
				WHERE GuildId = @GuildId AND Action = @Action
			";
			var @params = actions.Select(x => new
			{
				GuildId = guildId.ToString(),
				Action = x.ToString()
			});
			return await BulkModifyAsync(SQL, @params).CAF();
		}

		public async Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var result = await connection.QueryAsync<string>(@"
				SELECT ChannelId
				FROM IgnoredChannel
				WHERE GuildId = @GuildId
			", param).CAF();
			return result.Select(ulong.Parse).ToArray();
		}

		public async Task<IReadOnlyList<LogAction>> GetLogActionsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var result = await connection.QueryAsync<string>(@"
				SELECT Action
				FROM LogAction
				WHERE GuildId = @GuildId
			", param).CAF();
			return result.SelectWhere(x => x != null, Enum.Parse<LogAction>).ToArray();
		}

		public async Task<IReadOnlyLogChannels> GetLogChannelsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			return await connection.QuerySingleOrDefaultAsync<LogChannels>(@"
				SELECT ImageLogId, ModLogId, ServerLogId
				FROM LogChannel
				WHERE GuildId = @GuildId
			", param).CAF() ?? new LogChannels();
		}

		public async Task UpdateLogChannelAsync(Log log, ulong guildId, ulong? channelId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var name = GetLogName(log);
			var param = new { GuildId = guildId.ToString(), ChannelId = channelId?.ToString() };
			await connection.ExecuteAsync($@"
				INSERT OR IGNORE INTO LogChannel
					( GuildId, {name} )
					VALUES
					( @GuildId, @ChannelId );
				UPDATE LogChannel
				SET {name} = @ChannelId
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		private string GetLogName(Log log) => log switch
		{
			Log.Image => "ImageLogId",
			Log.Mod => "ModLogId",
			Log.Server => "ServerLogId",
			_ => throw new ArgumentOutOfRangeException(nameof(log))
		};
	}
}