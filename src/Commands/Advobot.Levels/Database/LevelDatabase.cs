using Advobot.Levels.Database.Models;
using Advobot.SQLite;

using Dapper;

using System.Data;
using System.Data.SQLite;
using System.Text;

namespace Advobot.Levels.Database;

public sealed class LevelDatabase(IConnectionString<LevelDatabase> connection)
	: DatabaseBase<SQLiteConnection>(connection)
{
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
		", @params).ConfigureAwait(false);
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
		", @params).ConfigureAwait(false);
	}

	public async Task<int> GetDistinctUserCountAsync(SearchArgs args)
	{
		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

		return await connection.QuerySingleAsync<int>($@"
			SELECT COUNT(DISTINCT UserId)
			FROM User
			{GenerateWhereStatement(args)}
		", args).ConfigureAwait(false);
	}

	public async Task<IReadOnlyList<ulong>> GetIgnoredChannelsAsync(ulong guildId)
	{
		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

		var param = new { GuildId = guildId.ToString() };
		var result = await connection.QueryAsync<ulong>(@"
			SELECT ChannelId
			FROM IgnoredChannel
			WHERE GuildId = @GuildId
		", param).ConfigureAwait(false);
		return [.. result];
	}

	public async Task<IRank> GetRankAsync(SearchArgs args)
	{
		if (args.UserId is null)
		{
			throw new ArgumentException("UserId cannot be null", nameof(args));
		}

		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

		var xp = await GetXpAsync(args).ConfigureAwait(false);
		var results = await connection.QueryAsync<int>($@"
			SELECT SUM(Experience)
			FROM User
			{GenerateWhereStatement(args)}
			GROUP BY UserId
		", args).ConfigureAwait(false);

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

	public async Task<IReadOnlyList<IRank>> GetRanksAsync(SearchArgs args, int offset, int limit)
	{
		await using var connection = await GetConnectionAsync().ConfigureAwait(false);

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
		", param).ConfigureAwait(false);
		var count = await GetDistinctUserCountAsync(args).ConfigureAwait(false);
		return [.. results.Select((x, i) => new Rank(x.UserId, x.Xp, offset + i, count))];
	}

	public async Task<User> GetUserAsync(SearchArgs args)
	{
		return await GetOneAsync<User?>($@"
			SELECT *
			FROM User
			{GenerateSingleUserWhereStatement(args)}
		", args).ConfigureAwait(false) ?? new User(args);
	}

	public async Task<int> GetXpAsync(SearchArgs args)
	{
		return await GetOneAsync<int?>($@"
			SELECT SUM(Experience)
			FROM User
			{GenerateSingleUserWhereStatement(args)}
		", args).ConfigureAwait(false) ?? 0;
	}

	public Task<int> UpsertUserAsync(User user)
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
		if (value is null)
		{
			return;
		}

		var statement = sb.Length > 0 ? " AND " : "WHERE ";
		sb.Append(statement).Append(name).Append(" = @").Append(name);
	}

	private string GenerateSingleUserWhereStatement(SearchArgs args)
	{
		var where = new StringBuilder();
		AppendWhereStatement(where, args.UserId, nameof(args.UserId));
		AppendWhereStatement(where, args.GuildId, nameof(args.GuildId));
		AppendWhereStatement(where, args.ChannelId, nameof(args.ChannelId));
		return where.ToString();
	}

	private string GenerateWhereStatement(SearchArgs args)
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