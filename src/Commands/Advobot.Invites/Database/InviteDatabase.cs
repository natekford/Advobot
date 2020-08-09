using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Invites.Models;
using Advobot.Invites.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

using Dapper;

namespace Advobot.Invites.Database
{
	public sealed class InviteDatabase : DatabaseBase<SQLiteConnection>
	{
		public InviteDatabase(IConnectionFor<InviteDatabase> conn) : base(conn)
		{
		}

		public Task AddInviteAsync(IReadOnlyListedInvite invite)
		{
			return ModifyAsync(@"
				INSERT INTO Invite
				( GuildId, Code, Name, HasGlobalEmotes, LastBumped, MemberCount )
				VALUES
				( @GuildId, @Code, @Name, @HasGlobalEmotes, @LastBumped, @MemberCount )
			", invite);
		}

		public Task AddKeywordAsync(IReadOnlyKeyword keyword)
		{
			return ModifyAsync(@"
				INSERT INTO Keyword
				( GuildId, Word )
				VALUES
				( @GuildId, @Word )
			", keyword);
		}

		public Task<int> AddKeywordsAsync(IEnumerable<IReadOnlyKeyword> keywords)
		{
			const string SQL = @"
				INSERT INTO Keyword
				( GuildId, Word )
				VALUES
				( @GuildId, @Word )
			";
			return BulkModifyAsync(SQL, keywords);
		}

		public Task DeleteInviteAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return ModifyAsync(@"
				DELETE FROM Invite
				WHERE GuildId = @GuildId
			", param);
		}

		public Task DeleteKeywordAsync(ulong guildId, string word)
		{
			var param = new { GuildId = guildId.ToString(), Word = word };
			return ModifyAsync(@"
				DELETE FROM Keyword
				WHERE GuildId = @GuildId AND Word = @Word
			", param);
		}

		public async Task<IReadOnlyListedInvite?> GetInviteAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetOneAsync<ListedInvite?>(@"
				SELECT *
				FROM Invite
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyListedInvite>> GetInvitesAsync()
		{
			return await GetManyAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
			", null).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyListedInvite>> GetInvitesAsync(
			IEnumerable<string> keywords)
		{
			var param = new { Words = keywords };
			return await GetManyAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
				INNER JOIN Keyword
				ON Keyword.GuildId = Invite.GuildId
				WHERE Keyword.Word IN @Words
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyKeyword>> GetKeywords(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<Keyword>(@"
				SELECT *
				FROM Keyword
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public Task UpdateInviteAsync(IReadOnlyListedInvite invite)
		{
			return ModifyAsync(@"
				UPDATE Invite
				SET
					Name = @Name,
					HasGlobalEmotes = @HasGlobalEmotes,
					LastBumped = @LastBumped,
					MemberCount = @MemberCount
				WHERE GuildId = @GuildId
			", invite);
		}
	}
}