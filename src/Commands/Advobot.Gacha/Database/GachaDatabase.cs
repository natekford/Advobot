using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Databases.AbstractSQL;
using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;
using Advobot.Gacha.Utilities;
using Advobot.Services.Time;

using AdvorangesUtils;

using Dapper;

using Image = Advobot.Gacha.Models.Image;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabase : DatabaseBase<SQLiteConnection>
	{
		private readonly ITime _Time;

		public GachaDatabase(ITime time, IGachaDatabaseStarter starter) : base(starter)
		{
			_Time = time;
		}

		public async Task AddCharacterAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Character
				( CharacterId, SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter )
				VALUES
				( @CharacterId, @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter )
			", character).CAF();
		}

		public async Task<int> AddCharactersAsync(IEnumerable<IReadOnlyCharacter> characters)
		{
			const string SQL = @"
				INSERT INTO Character
				( CharacterId, SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter )
				VALUES
				( @CharacterId, @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter )
			";
			return await BulkModify(SQL, characters).CAF();
		}

		public async Task AddClaimAsync(IReadOnlyClaim claim)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Claim
				( ClaimId, GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim )
				VALUES
				( @ClaimId, @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryClaim )
			", claim).CAF();
		}

		public async Task<int> AddClaimsAsync(IEnumerable<IReadOnlyClaim> claims)
		{
			const string SQL = @"
				INSERT INTO Claim
				( ClaimId, GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim )
				VALUES
				( @ClaimId, @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryClaim )
			";
			return await BulkModify(SQL, claims).CAF();
		}

		public async Task AddImageAsync(IReadOnlyImage image)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Image
				( CharacterId, Url )
				VALUES
				( @CharacterId, @Url )
			", image).CAF();
		}

		public async Task AddSourceAsync(IReadOnlySource source)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Source
				( SourceId, Name, ThumbnailUrl )
				VALUES
				( @SourceId, @Name, @ThumbnailUrl )
			", source).CAF();
		}

		public async Task<int> AddSourcesAsync(IEnumerable<IReadOnlySource> sources)
		{
			const string SQL = @"
				INSERT INTO Source
				( SourceId, Name, ThumbnailUrl )
				VALUES
				( @SourceId, @Name, @ThumbnailUrl )
			";
			return await BulkModify(SQL, sources).CAF();
		}

		public async Task AddUserAsync(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			", user).CAF();
		}

		public async Task<int> AddUsersAsync(IEnumerable<IReadOnlyUser> users)
		{
			const string SQL = @"
				INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			";
			return await BulkModify(SQL, users).CAF();
		}

		public async Task AddWishAsync(IReadOnlyWish wish)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Wish
				( WishId, GuildId, UserId, CharacterId )
				VALUES
				( @WishId, @GuildId, @UserId, @CharacterId )
			", wish).CAF();
		}

		public override async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//Source
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Source
			(
				SourceId					INTEGER NOT NULL PRIMARY KEY,
				Name						TEXT NOT NULL,
				ThumbnailUrl				TEXT
			);
			CREATE UNIQUE INDEX IF NOT EXISTS Source_Name_Index ON Source
			(
				Name
			);
			").CAF();

			//Character
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Character
			(
				CharacterId					INTEGER NOT NULL PRIMARY KEY,
				SourceId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				GenderIcon					TEXT NOT NULL,
				Gender						INTEGER NOT NULL,
				RollType					INTEGER NOT NULL,
				FlavorText					TEXT,
				IsFakeCharacter				INTEGER NOT NULL,
				FOREIGN KEY(SourceId) REFERENCES Source(SourceId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Character_SourceId_Index ON Character
			(
				SourceId
			);
			CREATE INDEX IF NOT EXISTS Character_Name_Index ON Character
			(
				Name
			);
			CREATE INDEX IF NOT EXISTS Character_Gender_Index ON Character
			(
				Gender
			);
			").CAF();

			//Alias
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Alias
			(
				CharacterId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				IsSpoiler					INTEGER NOT NULL,
				PRIMARY KEY(CharacterId, Name)
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Alias_CharacterId_Index ON Alias
			(
				CharacterId
			);
			").CAF();

			//Image
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Image
			(
				CharacterId					INTEGER NOT NULL,
				Url							TEXT NOT NULL,
				PRIMARY KEY(CharacterId, Url),
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX IF NOT EXISTS Image_CharacterId_Index ON Image
			(
				CharacterId
			);
			").CAF();

			//User
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS User
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId)
			);
			").CAF();

			//Claim
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Claim
			(
				ClaimId						INTEGER NOT NULL,
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				ImageUrl					TEXT,
				IsPrimaryClaim				INTEGER NOT NULL,
				PRIMARY KEY(GuildId, CharacterId)
			);
			CREATE INDEX IF NOT EXISTS Claim_GuildId_Index ON Claim
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Claim_GuildId_UserId_Index ON Claim
			(
				GuildId,
				UserId
			);
			").CAF();

			//Wish
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Wish
			(
				WishId						INTEGER NOT NULL,
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, UserId, CharacterId)
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_Index ON Wish
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_UserId_Index ON Wish
			(
				GuildId,
				UserId
			);
			CREATE INDEX IF NOT EXISTS Wish_GuildId_CharacterId_Index ON Wish
			(
				GuildId,
				CharacterId
			);
			").CAF();

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public async Task<IReadOnlyCharacter> GetCharacterAsync(long characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { CharacterId = characterId };
			return await connection.QuerySingleOrDefaultAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<CharacterMetadata> GetCharacterMetadataAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var id = character.CharacterId;
			var source = await GetSourceAsync(character.SourceId).CAF();
			var claims = await connection.GetRankAsync<Claim>("Claim", id, _Time.UtcNow).CAF();
			//TODO: implement likes
			var likes = new AmountAndRank("Likes", -1, -1, -1, -1);
			var wishes = await connection.GetRankAsync<Wish>("Wish", id, _Time.UtcNow).CAF();
			return new CharacterMetadata(source, character, claims, likes, wishes);
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

			var query = await connection.QueryAsync<Character>(@"
				SELECT *
				FROM Character
			").CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IReadOnlySource source)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { source.SourceId };
			var query = await connection.QueryAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE SourceId = @SourceId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IEnumerable<long> ids)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { Ids = ids };
			var query = await connection.QueryAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE CharacterId IN @Ids
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyClaim> GetClaimAsync(ulong guildId, IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			return await connection.QuerySingleOrDefaultAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyClaim> GetClaimAsync(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId, character.CharacterId };
			return await connection.QuerySingleOrDefaultAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyImage>> GetImagesAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { character.CharacterId };
			var query = await connection.QueryAsync<Image>(@"
				SELECT *
				FROM Image
				WHERE CharacterId = @CharacterId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlySource> GetSourceAsync(long sourceId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { SourceId = sourceId };
			return await connection.QuerySingleOrDefaultAsync<Source>(@"
				SELECT *
				FROM Source
				WHERE SourceId = @SourceId
			", param).CAF();
		}

		public async Task<IReadOnlyCharacter> GetUnclaimedCharacter(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			//Time for 500,000 records in both Character and Claim:
			//NOT IN = 3796ms
			//NOT EXISTS = 3946ms
			//LEFT JOIN = 3636ms
			//LEFT JOIN makes the least sense to read, but it's the fastest by a fair bit

			var param = new { GuildId = guildId.ToString() };
			return await connection.QuerySingleOrDefaultAsync<Character>(@"
				SELECT l.*
				From Character l
				LEFT JOIN Claim r
				ON r.GuildId = @GuildId AND r.CharacterId = l.CharacterId
				WHERE r.CharacterId IS NULL
				ORDER BY RANDOM() LIMIT 1
			", param).CAF();
		}

		public async Task<IReadOnlyUser> GetUserAsync(ulong guildId, ulong userId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), UserId = userId.ToString() };
			return await connection.QuerySingleOrDefaultAsync<User>(@"
				SELECT GuildId, UserId
				FROM User
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var query = await connection.QueryAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId, IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			var query = await connection.QueryAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId };
			var query = await connection.QueryAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.ToArray();
		}

		public async Task<int> TradeAsync(IEnumerable<ITrade> trades)
		{
			const string SQL = @"
				UPDATE Claim
				SET UserId = @ReceiverId
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			";
			return await BulkModify(SQL, trades).CAF();
		}

		public async Task UpdateClaimImageUrlAsync(IReadOnlyClaim claim, string? url)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { claim.GuildId, claim.UserId, claim.CharacterId, Url = url };
			await connection.QueryAsync(@"
				UPDATE Claim
				SET ImageUrl = @Url
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}

		protected override Task<int> ExecuteAsync(
			IDbConnection connection,
			string sql,
			object @params,
			IDbTransaction transaction)
			=> connection.ExecuteAsync(sql, @params, transaction);
	}
}