﻿using System.Collections.Generic;
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
	public sealed class GachaDatabase : DatabaseBase<SQLiteConnection>, IGachaDatabase
	{
		private const string INSERT_CHAR = @"
			INSERT INTO Character
			( CharacterId, SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter )
			VALUES
			( @CharacterId, @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter )
		";

		private const string INSERT_CLAIM = @"
			INSERT INTO Claim
			( ClaimId, GuildId, UserId, CharacterId, ImageUrl, IsPrimaryClaim )
			VALUES
			( @ClaimId, @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryClaim )
		";

		private const string INSERT_IMG = @"
			INSERT INTO Image
			( CharacterId, Url )
			VALUES
			( @CharacterId, @Url )
		";

		private const string INSERT_SRC = @"
			INSERT INTO Source
			( SourceId, Name, ThumbnailUrl )
			VALUES
			( @SourceId, @Name, @ThumbnailUrl )
		";

		private const string INSERT_USER = @"
			INSERT INTO User
			( GuildId, UserId )
			VALUES
			( @GuildId, @UserId )
		";

		private const string INSERT_WISH = @"
			INSERT INTO Wish
			( WishId, GuildId, UserId, CharacterId )
			VALUES
			( @WishId, @GuildId, @UserId, @CharacterId )
		";

		private readonly ITime _Time;

		public CloseIds CharacterIds { get; } = new CloseIds
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};

		public CloseIds SourceIds { get; } = new CloseIds
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};

		public GachaDatabase(ITime time, IGachaDatabaseStarter starter) : base(starter)
		{
			_Time = time;
		}

		public Task AddCharacterAsync(IReadOnlyCharacter character)
		{
			CharacterIds.Add(character.CharacterId, character.Name);
			return InsertAsync(INSERT_CHAR, character);
		}

		public Task<int> AddCharactersAsync(IEnumerable<IReadOnlyCharacter> characters)
		{
			foreach (var character in characters)
			{
				CharacterIds.Add(character.CharacterId, character.Name);
			}
			return BulkModifyAsync(INSERT_CHAR, characters);
		}

		public Task AddClaimAsync(IReadOnlyClaim claim)
			=> InsertAsync(INSERT_CLAIM, claim);

		public Task<int> AddClaimsAsync(IEnumerable<IReadOnlyClaim> claims)
			=> BulkModifyAsync(INSERT_CLAIM, claims);

		public Task AddImageAsync(IReadOnlyImage image)
			=> InsertAsync(INSERT_IMG, image);

		public Task AddSourceAsync(IReadOnlySource source)
		{
			SourceIds.Add(source.SourceId, source.Name);
			return InsertAsync(INSERT_SRC, source);
		}

		public Task<int> AddSourcesAsync(IEnumerable<IReadOnlySource> sources)
		{
			foreach (var source in sources)
			{
				SourceIds.Add(source.SourceId, source.Name);
			}
			return BulkModifyAsync(INSERT_SRC, sources);
		}

		public Task AddUserAsync(IReadOnlyUser user)
			=> InsertAsync(INSERT_USER, user);

		public Task<int> AddUsersAsync(IEnumerable<IReadOnlyUser> users)
			=> BulkModifyAsync(INSERT_USER, users);

		public Task AddWishAsync(IReadOnlyWish wish)
			=> InsertAsync(INSERT_WISH, wish);

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

			//Cache sources/characters for similar name checking
			foreach (var source in await connection.QueryAsync<Source>(@"
				SELECT SourceId, Name
				FROM Character
			").CAF())
			{
				SourceIds.Add(source.SourceId, source.Name);
			}
			foreach (var character in await connection.QueryAsync<Character>(@"
				SELECT CharacterId, Name
				FROM Character
			").CAF())
			{
				CharacterIds.Add(character.CharacterId, character.Name);
			}

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public async Task<IReadOnlyCharacter> GetCharacterAsync(long id)
		{
			var param = new { CharacterId = id };
			return await GetOneAsync<Character>(@"
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
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
			", null).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IReadOnlySource source)
		{
			var param = new { source.SourceId };
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE SourceId = @SourceId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyCharacter>> GetCharactersAsync(IEnumerable<long> ids)
		{
			var param = new { Ids = ids };
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE CharacterId IN @Ids
			", param).CAF();
		}

		public async Task<IReadOnlyClaim> GetClaimAsync(ulong guildId, IReadOnlyCharacter character)
		{
			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			return await GetOneAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyClaim> GetClaimAsync(IReadOnlyUser user, IReadOnlyCharacter character)
		{
			var param = new
			{
				GuildId = user.GuildId.ToString(),
				UserId = user.UserId.ToString(),
				character.CharacterId
			};
			return await GetOneAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyClaim>> GetClaimsAsync(IReadOnlyUser user)
		{
			var param = new
			{
				GuildId = user.GuildId.ToString(),
				UserId = user.UserId.ToString()
			};
			return await GetManyAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyImage>> GetImagesAsync(IReadOnlyCharacter character)
		{
			var param = new { character.CharacterId };
			return await GetManyAsync<Image>(@"
				SELECT *
				FROM Image
				WHERE CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlySource> GetSourceAsync(long sourceId)
		{
			var param = new { SourceId = sourceId };
			return await GetOneAsync<Source>(@"
				SELECT *
				FROM Source
				WHERE SourceId = @SourceId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlySource>> GetSourcesAsync(IEnumerable<long> ids)
		{
			var param = new { Ids = ids };
			return await GetManyAsync<Source>(@"
				SELECT *
				FROM Source
				WHERE SourceId IN @Ids
			", param).CAF();
		}

		public async Task<IReadOnlyCharacter> GetUnclaimedCharacter(ulong guildId)
		{
			//Time for 500,000 records in both Character and Claim:
			//NOT IN = 3796ms
			//NOT EXISTS = 3946ms
			//LEFT JOIN = 3636ms
			//LEFT JOIN makes the least sense to read, but it's the fastest by a fair bit

			var param = new { GuildId = guildId.ToString() };
			return await GetOneAsync<Character>(@"
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
			var param = new { GuildId = guildId.ToString(), UserId = userId.ToString() };
			return await GetOneAsync<User>(@"
				SELECT GuildId, UserId
				FROM User
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(ulong guildId, IReadOnlyCharacter character)
		{
			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			return await GetManyAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyWish>> GetWishesAsync(IReadOnlyUser user)
		{
			var param = new
			{
				GuildId = user.GuildId.ToString(),
				UserId = user.UserId.ToString()
			};
			return await GetManyAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<int> TradeAsync(IEnumerable<ITrade> trades)
		{
			var @params = trades.Select(x => new
			{
				GuildId = x.GuildId.ToString(),
				ReceiverId = x.ReceiverId.ToString(),
				x.CharacterId
			});
			return await BulkModifyAsync(@"
				UPDATE Claim
				SET UserId = @ReceiverId
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", @params).CAF();
		}

		public async Task UpdateClaimImageUrlAsync(IReadOnlyClaim claim, string? url)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new
			{
				GuildId = claim.GuildId.ToString(),
				UserId = claim.UserId.ToString(),
				claim.CharacterId,
				Url = url
			};
			await connection.ExecuteAsync(@"
				UPDATE Claim
				SET ImageUrl = @Url
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}

		protected override Task<int> BulkModifyAsync<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction)
			=> connection.ExecuteAsync(sql, @params, transaction);

		private async Task<IReadOnlyList<T>> GetManyAsync<T>(string sql, object? param)
		{
			using var connection = await GetConnectionAsync().CAF();
			var result = await connection.QueryAsync<T>(sql, param).CAF();
			return result.ToArray();
		}

		private async Task<T> GetOneAsync<T>(string sql, object param)
		{
			using var connection = await GetConnectionAsync().CAF();
			return await connection.QuerySingleOrDefaultAsync<T>(sql, param).CAF();
		}

		private async Task InsertAsync<T>(string sql, T item)
		{
			using var connection = await GetConnectionAsync().CAF();
			await connection.QueryAsync(sql, item).CAF();
		}
	}
}