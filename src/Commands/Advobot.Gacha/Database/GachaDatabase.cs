using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;
using Advobot.Utilities;
using AdvorangesUtils;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Image = Advobot.Gacha.Models.Image;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabase
	{
		private readonly IDatabaseStarter _Starter;

		public GachaDatabase(IServiceProvider provider)
		{
			_Starter = provider.GetRequiredService<IDatabaseStarter>();
		}

		public async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			if (_Starter.IsDatabaseCreated())
			{
				return await GetTablesAsync().CAF();
			}

			using var connection = await GetConnectionAsync().CAF();

			//Source
			await connection.ExecuteAsync(@"
			CREATE TABLE Source
			(
				SourceId					INTEGER NOT NULL PRIMARY KEY,
				Name						TEXT NOT NULL,
				ThumbnailUrl				TEXT,
				TimeCreated					INTEGER NOT NULL
			);
			CREATE UNIQUE INDEX Source_Name_Index ON Source
			(
				Name
			);
			").CAF();

			//Character
			await connection.ExecuteAsync(@"
			CREATE TABLE Character
			(
				CharacterId					INTEGER NOT NULL PRIMARY KEY,
				SourceId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				GenderIcon					TEXT NOT NULL,
				Gender						INTEGER NOT NULL,
				RollType					INTEGER NOT NULL,
				FlavorText					TEXT,
				TimeCreated					INTEGER NOT NULL,
				IsFakeCharacter				INTEGER NOT NULL,
				FOREIGN KEY(SourceId) REFERENCES Source(SourceId) ON DELETE CASCADE
			);
			CREATE INDEX Character_SourceId_Index ON Character
			(
				SourceId
			);
			CREATE INDEX Character_Name_Index ON Character
			(
				Name
			);
			CREATE INDEX Character_Gender_Index ON Character
			(
				Gender
			);
			").CAF();

			//Alias
			await connection.ExecuteAsync(@"
			CREATE TABLE Alias
			(
				CharacterId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				IsSpoiler					INTEGER NOT NULL,
				PRIMARY KEY(CharacterId, Name)
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX Alias_CharacterId_Index ON Alias
			(
				CharacterId
			);
			").CAF();

			//Image
			await connection.ExecuteAsync(@"
			CREATE TABLE Image
			(
				CharacterId					INTEGER NOT NULL,
				Url							TEXT NOT NULL,
				PRIMARY KEY(CharacterId, Url),
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			);
			CREATE INDEX Image_CharacterId_Index ON Image
			(
				CharacterId
			);
			").CAF();

			//User
			await connection.ExecuteAsync(@"
			CREATE TABLE User
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId)
			);
			").CAF();

			//Claim
			await connection.ExecuteAsync(@"
			CREATE TABLE Claim
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				ImageUrl					TEXT,
				IsPrimaryClaim				INTEGER NOT NULL,
				TimeCreated					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, CharacterId)
			);
			CREATE INDEX Claim_GuildId_Index ON Claim
			(
				GuildId
			);
			CREATE INDEX Claim_GuildId_UserId_Index ON Claim
			(
				GuildId,
				UserId
			);
			").CAF();

			//Wish
			await connection.ExecuteAsync(@"
			CREATE TABLE Wish
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				TimeCreated					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, UserId, CharacterId)
			);
			CREATE INDEX Wish_GuildId_Index ON Wish
			(
				GuildId
			);
			CREATE INDEX Wish_GuildId_UserId_Index ON Wish
			(
				GuildId,
				UserId
			);
			CREATE INDEX Wish_GuildId_CharacterId_Index ON Wish
			(
				GuildId,
				CharacterId
			);
			").CAF();

			return await GetTablesAsync().CAF();
		}
		private async Task<IReadOnlyList<string>> GetTablesAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

			var query = await connection.QueryAsync<string>(@"
				SELECT name FROM sqlite_master
				WHERE type='table'
				ORDER BY name;
			").CAF();
			return query.ToArray();
		}
		private async Task<SQLiteConnection> GetConnectionAsync()
		{
			var conn = new SQLiteConnection(_Starter.GetConnectionString());
			await conn.OpenAsync().CAF();
			return conn;
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
		public async Task<long> AddUserAsync(IReadOnlyUser user)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			", user).CAF();
			return connection.LastInsertRowId;
		}
		public async Task<int> AddUsersAsync(IEnumerable<IReadOnlyUser> users)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			", users).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public async Task<IReadOnlySource> GetSourceAsync(long sourceId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { SourceId = sourceId };
			return await connection.QuerySingleOrDefaultAsync<Source>(@"
				SELECT SourceId, Name, ThumbnailUrl, TimeCreated
				FROM Source
				WHERE SourceId = @SourceId
			", param).CAF();
		}
		public async Task<long> AddSourceAsync(IReadOnlySource source)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Source
				( SourceId, Name, ThumbnailUrl, TimeCreated )
				VALUES
				( @SourceId, @Name, @ThumbnailUrl, @TimeCreated )
			", source).CAF();
			return connection.LastInsertRowId;
		}
		public async Task<int> AddSourcesAsync(IEnumerable<IReadOnlySource> sources)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT INTO Source
				( SourceId, Name, ThumbnailUrl, TimeCreated )
				VALUES
				( @SourceId, @Name, @ThumbnailUrl, @TimeCreated )
			", sources).CAF();
			transaction.Commit();
			return affectedRowCount;
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IReadOnlySource source)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { source.SourceId };
			var query = await connection.QueryAsync<Character>(@"
				SELECT SourceId, CharacterId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated
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
				SELECT SourceId, CharacterId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated
				FROM Character
				WHERE CharacterId IN @Ids
			", param).CAF();
			return query.ToArray();
		}
		public async Task<IReadOnlyCharacter> GetCharacterAsync(long characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { CharacterId = characterId };
			return await connection.QuerySingleOrDefaultAsync<Character>(@"
				SELECT SourceId, CharacterId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated
				FROM Character
				WHERE CharacterId = @CharacterId
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
		public async Task<long> AddCharacterAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Character
				( SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated )
				VALUES
				( @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter, @TimeCreated )
			", character).CAF();
			return connection.LastInsertRowId;
		}
		public async Task<int> AddCharactersAsync(IEnumerable<IReadOnlyCharacter> characters)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT INTO Character
				( SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated )
				VALUES
				( @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter, @TimeCreated )
			", characters).CAF();
			transaction.Commit();
			return affectedRowCount;
		}
		public async Task<CharacterMetadata> GetCharacterMetadataAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var id = character.CharacterId;
			var source = await GetSourceAsync(character.SourceId).CAF();
			var claims = await connection.GetRankAsync<Claim>("Claim", id).CAF();
			var likes = new AmountAndRank("Likes", -1, -1, -1, -1);
			var wishes = await connection.GetRankAsync<Wish>("Wish", id).CAF();
			return new CharacterMetadata(source, character, claims, likes, wishes);
		}

		public async Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim, TimeCreated
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
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim, TimeCreated
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<IReadOnlyClaim> GetClaimAsync(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId, character.CharacterId };
			return await connection.QuerySingleOrDefaultAsync<Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim, TimeCreated
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}
		public async Task<long> AddClaimAsync(IReadOnlyClaim claim)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Claim
				( GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryClaim, @TimeCreated )
			", claim).CAF();
			return connection.LastInsertRowId;
		}
		public async Task<int> AddClaimsAsync(IEnumerable<IReadOnlyClaim> claims)
		{
			//Scope is needed to make the bulk adding not take ages
			using var connection = await GetConnectionAsync().CAF();
			using var transaction = connection.BeginTransaction();

			var affectedRowCount = await connection.ExecuteAsync(@"
				INSERT INTO Claim
				( GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryClaim, @TimeCreated )
			", claims).CAF();
			transaction.Commit();
			return affectedRowCount;
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

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString() };
			var query = await connection.QueryAsync<Wish>(@"
				SELECT GuildId, UserId, CharacterId, TimeCreated
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
				SELECT GuildId, UserId, CharacterId, TimeCreated
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
				SELECT GuildId, UserId, CharacterId, TimeCreated
				FROM Wish
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<long> AddWishAsync(IReadOnlyWish wish)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Wish
				( GuildId, UserId, CharacterId, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @TimeCreated )
			", wish).CAF();
			return connection.LastInsertRowId;
		}

		public async Task<IReadOnlyList<IReadOnlyImage>> GetImagesAsync(IReadOnlyCharacter character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { character.CharacterId };
			var query = await connection.QueryAsync<Image>(@"
				SELECT CharacterId, Url
				FROM Image
				WHERE CharacterId = @CharacterId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<long> AddImageAsync(IReadOnlyImage image)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Image
				( CharacterId, Url )
				VALUES
				( @CharacterId, @Url )
			", image).CAF();
			return connection.LastInsertRowId;
		}
	}
}
