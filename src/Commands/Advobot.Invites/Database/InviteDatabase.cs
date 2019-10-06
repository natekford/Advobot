using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases.AbstractSQL;
using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Invites.Database
{
	public sealed class InviteDatabase : IDatabase
	{
		private readonly IInviteDatabaseStarter _Starter;

		public InviteDatabase(IInviteDatabaseStarter starter)
		{
			_Starter = starter;
		}

		public async Task AddInviteAsync(IReadOnlyListedInvite invite)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
				INSERT INTO Invite
				( GuildId, Code, Name, HasGlobalEmotes, LastBumped, MemberCount )
				VALUES
				( @GuildId, @Code, @Name, @HasGlobalEmotes, @LastBumped, @MemberCount )
			", invite).CAF();
		}

		public async Task AddKeywordAsync(IReadOnlyKeyword keyword)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
				INSERT INTO Keyword
				( GuildId, Word )
				VALUES
				( @GuildId, @Word )
			", keyword).CAF();
		}

		public async Task<int> AddKeywordsAsync(IEnumerable<IReadOnlyKeyword> keywords)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT INTO Keyword
				( GuildId, Word )
				VALUES
				( @GuildId, @Word )
			", keywords).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await _Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//Invite
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Invite
			(
				GuildId						TEXT NOT NULL,
				Code						TEXT NOT NULL,
				Name						TEXT NOT NULL,
				HasGlobalEmotes				INTEGER NOT NULL,
				LastBumped					INTEGER NOT NULL,
				MemberCount					INTEGER NOT NULL,
				PRIMARY KEY(GuildId)
			);
			").CAF();

			//Keyword
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Keyword
			(
				GuildId						TEXT NOT NULL,
				Word						TEXT NOT NULL COLLATE NOCASE,
				PRIMARY KEY(GuildId, Word)
			);
			CREATE INDEX IF NOT EXISTS Keyword_GuildId ON Keyword
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Keyword_Word ON Keyword
			(
				Word
			);
			").CAF();

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public async Task DeleteInviteAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			await connection.ExecuteAsync(@"
				DELETE FROM Invite
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task DeleteKeywordAsync(ulong guildId, string word)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), Word = word };
			await connection.ExecuteAsync(@"
				DELETE FROM Keyword
				WHERE GuildId = @GuildId AND Word = @Word
			", param).CAF();
		}

		public async Task<IReadOnlyListedInvite?> GetInviteAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			return await connection.QuerySingleOrDefaultAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyListedInvite>> GetInvitesAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

			var query = await connection.QueryAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
			").CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyListedInvite>> GetInvitesAsync(
			IEnumerable<string> keywords)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { Words = keywords };
			var query = await connection.QueryAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
				INNER JOIN Keyword
				ON Keyword.GuildId = Invite.GuildId
				WHERE Keyword.Word IN @Words
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyKeyword>> GetKeywords(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var query = await connection.QueryAsync<Keyword>(@"
				SELECT *
				FROM Keyword
				WHERE GuildId = @GuildId
			", param).CAF();
			return query.ToArray();
		}

		public async Task UpdateInviteAsync(IReadOnlyListedInvite invite)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
				UPDATE Invite
				SET
					Name = @Name,
					HasGlobalEmotes = @HasGlobalEmotes,
					LastBumped = @LastBumped,
					MemberCount = @MemberCount
				WHERE GuildId = @GuildId
			", invite).CAF();
		}

		private Task<SQLiteConnection> GetConnectionAsync()
			=> _Starter.GetConnectionAsync<SQLiteConnection>();
	}
}