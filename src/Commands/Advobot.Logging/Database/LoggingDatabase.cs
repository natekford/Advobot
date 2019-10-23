using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases.AbstractSQL;
using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Logging.Database
{
	public sealed class LoggingDatabase : DatabaseBase<SQLiteConnection>
	{
		private const string ImageLogId = "ImageLogId";
		private const string ModLogId = "ModLogId";
		private const string ServerLogId = "ServerLogId";

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
			return await BulkModify(SQL, @params).CAF();
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
			return await BulkModify(SQL, @params).CAF();
		}

		public override async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//Log channels
			await connection.ExecuteAsync($@"
			CREATE TABLE IF NOT EXISTS LogChannel
			(
				GuildId						TEXT NOT NULL,
				{ImageLogId}				TEXT,
				{ModLogId}					TEXT,
				{ServerLogId}				TEXT,
				PRIMARY KEY(GuildId)
			);
			").CAF();

			//Log action
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS LogAction
			(
				GuildId						TEXT NOT NULL,
				Action						TEXT NOT NULL,
				PRIMARY KEY(GuildId, Action)
			);
			CREATE INDEX IF NOT EXISTS LogAction_GuildId_Index ON LogAction
			(
				GuildId
			);
			").CAF();

			//Ignored channel
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS IgnoredChannel
			(
				GuildId						TEXT NOT NULL,
				ChannelId					TEXT NOT NULL,
				PRIMARY KEY(GuildId, ChannelId)
			);
			CREATE INDEX IF NOT EXISTS IgnoredChannel_GuildId_Index ON IgnoredChannel
			(
				GuildId
			);
			").CAF();

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
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
			return await BulkModify(SQL, @params).CAF();
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
			return await BulkModify(SQL, @params).CAF();
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

		protected override Task<int> BulkModify<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction)
			=> connection.ExecuteAsync(sql, @params, transaction);

		private string GetLogName(Log log) => log switch
		{
			Log.Image => ImageLogId,
			Log.Mod => ModLogId,
			Log.Server => ServerLogId,
			_ => throw new ArgumentOutOfRangeException(nameof(log))
		};
	}
}