using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases.AbstractSQL;
using Advobot.Logging.Models;
using Advobot.Logging.ReadOnlyModels;
using Advobot.Services.GuildSettings.Settings;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Logging.Database
{
	public sealed class LoggingDatabase : IDatabase
	{
		private readonly ILoggingDatabaseStarter _Starter;

		public LoggingDatabase(ILoggingDatabaseStarter starter)
		{
			_Starter = starter;
		}

		public async Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT OR REPLACE INTO IgnoredChannel
				( GuildId, ChannelId )
				VALUES
				( @GuildId, @ChannelId )
			", @params).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public async Task<int> AddLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var @params = actions.Select(x => new
			{
				GuildId = guildId.ToString(),
				Action = x.ToString()
			});
			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT OR REPLACE INTO LogAction
				( GuildId, Action )
				VALUES
				( @GuildId, @Action )
			", @params).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await _Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//Log channels
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS LogChannel
			(
				GuildId						TEXT NOT NULL,
				ImageLogId					TEXT,
				ModLogId					TEXT,
				ServerLogId					TEXT,
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
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			var affectedRowCount = await connection.ExecuteAsync(@"
				DELETE FROM IgnoredChannel
				WHERE GuildId = @GuildId AND ChannelId = @ChannelId
			", @params).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public Task DeleteImageLogChannelAsync(ulong guildId)
			=> DeleteLogChannelAsync(guildId, "ImageLogId");

		public async Task<int> DeleteLogActionsAsync(ulong guildId, IEnumerable<LogAction> actions)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var @params = actions.Select(x => new
			{
				GuildId = guildId.ToString(),
				Action = x.ToString()
			});
			var affectedRowCount = await connection.ExecuteAsync(@"
				DELETE FROM LogAction
				WHERE GuildId = @GuildId AND Action = @Action
			", @params).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public Task DeleteModLogChannelAsync(ulong guildId)
			=> DeleteLogChannelAsync(guildId, "ModLogId");

		public Task DeleteServerLogChannelAsync(ulong guildId)
			=> DeleteLogChannelAsync(guildId, "ServerLogId");

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

		public Task UpdateImageLogChannelAsync(ulong guildId, ulong channelId)
			=> UpdateLogChannelAsync(guildId, channelId, "ImageLogId");

		public Task UpdateModLogChannelAsync(ulong guildId, ulong channelId)
			=> UpdateLogChannelAsync(guildId, channelId, "ModLogId");

		public Task UpdateServerLogChannelAsync(ulong guildId, ulong channelId)
			=> UpdateLogChannelAsync(guildId, channelId, "ServerLogId");

		private async Task DeleteLogChannelAsync(ulong guildId, string name)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			await connection.ExecuteAsync($@"
				UPDATE LogChannel
				SET {name} = NULL
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		private Task<SQLiteConnection> GetConnectionAsync()
			=> _Starter.GetConnectionAsync<SQLiteConnection>();

		private async Task UpdateLogChannelAsync(ulong guildId, ulong channelId, string name)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), ChannelId = channelId.ToString() };
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
	}
}