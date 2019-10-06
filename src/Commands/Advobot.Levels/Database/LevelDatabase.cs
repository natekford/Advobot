using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advobot.Databases.AbstractSQL;
using Advobot.Levels.Metadata;
using Advobot.Levels.Models;
using Advobot.Levels.ReadOnlyModels;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Levels.Database
{
	public sealed class LevelDatabase : IDatabase
	{
		private readonly ILevelDatabaseStarter _Starter;

		public LevelDatabase(ILevelDatabaseStarter starter)
		{
			_Starter = starter;
		}

		public async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await _Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//User
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS User
			(
				GuildId						TEXT NOT NULL,
				ChannelId					TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				Experience					INTEGER NOT NULL,
				MessageCount				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, ChannelId, UserId)
			);
			CREATE INDEX IF NOT EXISTS User_GuildId_ChannelId_Index ON User
			(
				GuildId,
				ChannelId
			);
			CREATE INDEX IF NOT EXISTS User_GuildId_Index ON User
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS User_ChannelId_Index ON User
			(
				ChannelId
			);
			").CAF();

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public async Task<int> GetDistinctUserCountAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var where = GenerateWhereStatement(args);
			return await connection.QuerySingleAsync<int>($@"
				SELECT COUNT(DISTINCT UserId)
				FROM User
				{where}
			", args).CAF();
		}

		public async Task<IRank> GetRankAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var xp = await GetXpAsync(args).CAF();

			var where = GenerateWhereStatement(args);
			var results = await connection.QueryAsync<int>($@"
				SELECT SUM(Experience)
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
			return new Rank(args.UserIdValue ?? 0, xp, rank, total);
		}

		public async Task<IReadOnlyList<IRank>> GetRanksAsync(ISearchArgs args, int offset, int limit)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new
			{
				args.ChannelId,
				args.GuildId,
				args.UserId,
				Offset = offset,
				Limit = limit,
			};
			var where = GenerateWhereStatement(args);
			var results = await connection.QueryAsync<TempRankInfo>($@"
			    SELECT SUM(Experience) as Xp, UserId
				FROM User
				{where}
				GROUP BY UserId
				ORDER BY Xp DESC
				Limit @Limit OFFSET @Offset
			", param).CAF();
			var count = await GetDistinctUserCountAsync(args).CAF();
			return results.Select((x, i) => new Rank(x.UserIdValue, x.Xp, offset + i, count)).ToArray();
		}

		public async Task<IReadOnlyUser> GetUserAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var where = GenerateSingleUserWhereStatement(args);
			var result = await connection.QuerySingleOrDefaultAsync<User?>($@"
				SELECT *
				FROM User
				{where}
			", args).CAF();
			return result ?? new User(args);
		}

		public async Task<int> GetXpAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			var where = GenerateSingleUserWhereStatement(args);
			var result = await connection.QuerySingleOrDefaultAsync<int?>($@"
				SELECT SUM(Experience)
				FROM User
				{where}
			", args).CAF();
			return result ?? 0;
		}

		public async Task UpsertUser(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
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

		private void AppendWhereStatement(StringBuilder sb, object? value, string name)
		{
			if (value == null)
			{
				return;
			}

			var statement = sb.Length > 0 ? " AND " : "WHERE ";
			sb.Append(statement).Append(name).Append(" = @").Append(name);
		}

		private string GenerateSingleUserWhereStatement(ISearchArgs args)
		{
			var where = new StringBuilder();
			AppendWhereStatement(where, args.UserId, nameof(args.UserId));
			AppendWhereStatement(where, args.GuildId, nameof(args.GuildId));
			AppendWhereStatement(where, args.ChannelId, nameof(args.ChannelId));
			return where.ToString();
		}

		private string GenerateWhereStatement(ISearchArgs args)
		{
			var where = new StringBuilder();
			AppendWhereStatement(where, args.GuildId, nameof(args.GuildId));
			AppendWhereStatement(where, args.ChannelId, nameof(args.ChannelId));
			return where.ToString();
		}

		private Task<SQLiteConnection> GetConnectionAsync()
			=> _Starter.GetConnectionAsync<SQLiteConnection>();

		private sealed class TempRankInfo
		{
			public string UserId { get; set; } = null!;
			public ulong UserIdValue => ulong.Parse(UserId);
			public int Xp { get; set; }
		}
	}
}