using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Advobot.Levels.Metadata;
using Advobot.Levels.Models;
using Advobot.Levels.ReadOnlyModels;
using Advobot.Levels.Utilities;
using Advobot.SQLite;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Levels.Database
{
	public sealed class LevelDatabase : DatabaseBase<SQLiteConnection>
	{
		public LevelDatabase(IConnectionFor<LevelDatabase> conn) : base(conn)
		{
		}

		public async Task<int> AddIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
		{
			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			return await BulkModifyAsync(@"
				INSERT OR IGNORE INTO IgnoredChannel
				( GuildId, ChannelId )
				VALUES
				( @GuildId, @ChannelId )
			", @params).CAF();
		}

		public async Task<int> DeleteIgnoredChannelsAsync(ulong guildId, IEnumerable<ulong> channels)
		{
			var @params = channels.Select(x => new
			{
				GuildId = guildId.ToString(),
				ChannelId = x.ToString()
			});
			return await BulkModifyAsync(@"
				DELETE FROM IgnoredChannel
				WHERE GuildId = @GuildId AND ChannelId = @ChannelId
			", @params).CAF();
		}

		public async Task<int> GetDistinctUserCountAsync(ISearchArgs args)
		{
			using var connection = await GetConnectionAsync().CAF();

			return await connection.QuerySingleAsync<int>($@"
				SELECT COUNT(DISTINCT UserId)
				FROM User
				{GenerateWhereStatement(args)}
			", args).CAF();
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

		public async Task<IRank> GetRankAsync(ISearchArgs args)
		{
			if (args.UserId == null)
			{
				throw new ArgumentNullException(nameof(args.UserId));
			}

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
			return new Rank(args.UserId.Value, xp, rank, total);
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
			var results = await connection.QueryAsync<TempRankInfo>($@"
			    SELECT SUM(Experience) as Xp, UserId
				FROM User
				{GenerateWhereStatement(args)}
				GROUP BY UserId
				ORDER BY Xp DESC
				Limit @Limit OFFSET @Offset
			", param).CAF();
			var count = await GetDistinctUserCountAsync(args).CAF();
			return results.Select((x, i) => new Rank(x.UserId, x.Xp, offset + i, count)).ToArray();
		}

		public async Task<IReadOnlyUser> GetUserAsync(ISearchArgs args)
		{
			return await GetOneAsync<User?>($@"
				SELECT *
				FROM User
				{GenerateSingleUserWhereStatement(args)}
			", args).CAF() ?? new User(args);
		}

		public async Task<int> GetXpAsync(ISearchArgs args)
		{
			return await GetOneAsync<int?>($@"
				SELECT SUM(Experience)
				FROM User
				{GenerateSingleUserWhereStatement(args)}
			", args).CAF() ?? 0;
		}

		public Task UpsertUserAsync(IReadOnlyUser user)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO User
					( GuildId, ChannelId, UserId, Experience, MessageCount )
					VALUES
					( @GuildId, @ChannelId, @UserId, @Experience, @MessageCount );
				UPDATE User
				SET
					Experience = @Experience,
					MessageCount = @MessageCount
				WHERE UserId = @UserId AND GuildId = @GuildId AND ChannelId = @ChannelId
			", user);
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

		private sealed class TempRankInfo
		{
			public ulong UserId { get; set; }
			public int Xp { get; set; }
		}
	}
}