using Advobot.Invites.Models;
using Advobot.SQLite;

using AdvorangesUtils;

using System.Data.SQLite;

namespace Advobot.Invites.Database;

public sealed class InviteDatabase(IConnectionString<InviteDatabase> conn) : DatabaseBase<SQLiteConnection>(conn)
{
	public Task AddInviteAsync(ListedInvite invite)
	{
		return ModifyAsync(@"
				INSERT INTO Invite
				( GuildId, Code, Name, HasGlobalEmotes, LastBumped, MemberCount )
				VALUES
				( @GuildId, @Code, @Name, @HasGlobalEmotes, @LastBumped, @MemberCount )
			", invite);
	}

	public Task AddKeywordAsync(Keyword keyword)
	{
		return ModifyAsync(@"
				INSERT INTO Keyword
				( GuildId, Word )
				VALUES
				( @GuildId, @Word )
			", keyword);
	}

	public Task<int> AddKeywordsAsync(IEnumerable<Keyword> keywords)
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

	public async Task<ListedInvite?> GetInviteAsync(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString() };
		return await GetOneAsync<ListedInvite?>(@"
				SELECT *
				FROM Invite
				WHERE GuildId = @GuildId
			", param).CAF();
	}

	public async Task<IReadOnlyList<ListedInvite>> GetInvitesAsync()
	{
		return await GetManyAsync<ListedInvite>(@"
				SELECT *
				FROM Invite
			", null).CAF();
	}

	public async Task<IReadOnlyList<ListedInvite>> GetInvitesAsync(
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

	public async Task<IReadOnlyList<Keyword>> GetKeywords(ulong guildId)
	{
		var param = new { GuildId = guildId.ToString() };
		return await GetManyAsync<Keyword>(@"
				SELECT *
				FROM Keyword
				WHERE GuildId = @GuildId
			", param).CAF();
	}

	public Task UpdateInviteAsync(ListedInvite invite)
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