using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Levels.Metadata;
using Advobot.Levels.Models;
using Advobot.Levels.ReadOnlyModels;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Levels.Database
{
	public sealed class LevelDatabase
	{
		private readonly IDatabaseStarter _Starter;

		public LevelDatabase(IDatabaseStarter starter)
		{
			_Starter = starter;
		}

		public async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			if (_Starter.IsDatabaseCreated())
			{
				return await GetTablesAsync().CAF();
			}

			using var connection = await GetConnectionAsync().CAF();

			//User
			await connection.ExecuteAsync(@"
			CREATE TABLE User
			(
				GuildId						TEXT NOT NULL,
				ChannelId					TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				Experience					INTEGER NOT NULL,
				MessageCount				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, ChannelId, UserId)
			);
			CREATE INDEX User_GuildId_ChannelId_Index ON User
			(
				GuildId,
				ChannelId
			);
			CREATE INDEX User_GuildId_Index ON User
			(
				GuildId
			);
			CREATE INDEX User_ChannelId_Index ON User
			(
				ChannelId
			);
			").CAF();

			return await GetTablesAsync().CAF();
		}

		public async Task<Rank> GetRankAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var xp = await GetXpAsync(args).CAF();

			var where = new StringBuilder();
			AppendWhereStatement(where, args.GuildId, "GuildId = @GuildId");
			AppendWhereStatement(where, args.ChannelId, "ChannelId = @ChannelId");

			var results = await connection.QueryAsync<int>($@"
				SELECT SUM (Experience)
				FROM User
				{where}
				GROUP BY UserId
			", args).CAF();

			var rank = 1;
			var total = 0;
			foreach (var result in results)
			{
				++total;
				if (result > xp)
				{
					++rank;
				}
			}
			return new Rank(xp, rank, total);
		}

		public async Task<IReadOnlyUser> GetUserAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var result = await connection.QuerySingleOrDefaultAsync<User>($@"
				SELECT *
				FROM User
				WHERE UserId = @UserId AND GuildId = @GuildId AND ChannelId = @ChannelId
			", args).CAF();
			return result ?? new User(args);
		}

		public async Task<int> GetXpAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var where = new StringBuilder();
			AppendWhereStatement(where, args.UserId, "UserId = @UserId");
			AppendWhereStatement(where, args.GuildId, "GuildId = @GuildId");
			AppendWhereStatement(where, args.ChannelId, "ChannelId = @ChannelId");

			var result = await connection.QueryAsync<int>($@"
				SELECT Experience
				FROM User
				{where}
			", args).CAF();
			return result.Sum();
		}

		public async Task UpsertUser(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT OR IGNORE INTO User
					( GuildId, ChannelId, UserId, Experience, MessageCount )
					VALUES
					( @GuildId, @ChannelId, @UserId, @Experience, @MessageCount );
				UPDATE User
				SET
					Experience = @Experience,
					MessageCount = @MessageCount
				WHERE UserId = @UserId AND GuildId = @GuildId AND ChannelId = @ChannelId
			", user).CAF();
		}

		private void AppendWhereStatement(StringBuilder sb, object? value, string where)
		{
			if (value == null)
			{
				return;
			}

			var statement = sb.Length > 0 ? " AND " : "WHERE ";
			sb.Append(statement).Append(where);
		}

		private async Task<SQLiteConnection> GetConnectionAsync()
		{
			var conn = new SQLiteConnection(_Starter.GetConnectionString());
			await conn.OpenAsync().CAF();
			return conn;
		}

		private async Task<IReadOnlyList<string>> GetTablesAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

			var result = await connection.QueryAsync<string>(@"
				SELECT name FROM sqlite_master
				WHERE type='table'
				ORDER BY name;
			").CAF();
			return result.ToArray();
		}
	}
}