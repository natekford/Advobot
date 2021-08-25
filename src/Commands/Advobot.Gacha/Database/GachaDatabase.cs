﻿using System.Data;
using System.Data.SQLite;

using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Trading;
using Advobot.Gacha.Utilities;
using Advobot.Services.Time;
using Advobot.SQLite;

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
		public CloseIds CharacterIds { get; } = new()
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};
		public CloseIds SourceIds { get; } = new()
		{
			IncludeWhenContains = false,
			MaxAllowedCloseness = 2,
		};

		public GachaDatabase(ITime time, IConnectionStringFor<GachaDatabase> conn) : base(conn)
		{
			_Time = time;
		}

		public Task<int> AddCharacterAsync(Character character)
		{
			CharacterIds.Add(character.CharacterId, character.Name);
			return ModifyAsync(INSERT_CHAR, character);
		}

		public Task<int> AddCharactersAsync(IEnumerable<Character> characters)
		{
			foreach (var character in characters)
			{
				CharacterIds.Add(character.CharacterId, character.Name);
			}
			return BulkModifyAsync(INSERT_CHAR, characters);
		}

		public Task<int> AddClaimAsync(Claim claim)
			=> ModifyAsync(INSERT_CLAIM, claim);

		public Task<int> AddClaimsAsync(IEnumerable<Claim> claims)
			=> BulkModifyAsync(INSERT_CLAIM, claims);

		public Task<int> AddImageAsync(Image image)
			=> ModifyAsync(INSERT_IMG, image);

		public Task<int> AddSourceAsync(Source source)
		{
			SourceIds.Add(source.SourceId, source.Name);
			return ModifyAsync(INSERT_SRC, source);
		}

		public Task<int> AddSourcesAsync(IEnumerable<Source> sources)
		{
			foreach (var source in sources)
			{
				SourceIds.Add(source.SourceId, source.Name);
			}
			return BulkModifyAsync(INSERT_SRC, sources);
		}

		public Task<int> AddUserAsync(User user)
			=> ModifyAsync(INSERT_USER, user);

		public Task<int> AddUsersAsync(IEnumerable<User> users)
			=> BulkModifyAsync(INSERT_USER, users);

		public Task<int> AddWishAsync(Wish wish)
			=> ModifyAsync(INSERT_WISH, wish);

		public async Task CacheNamesAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

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
		}

		public async Task<Character> GetCharacterAsync(long id)
		{
			var param = new { CharacterId = id };
			return await GetOneAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<CharacterMetadata> GetCharacterMetadataAsync(Character character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var id = character.CharacterId;
			var source = await GetSourceAsync(character.SourceId).CAF();
			var claims = await connection.GetRankAsync<Claim>("Claim", id, _Time.UtcNow).CAF();
			//TODO: implement likes
			var likes = new AmountAndRank("Likes", -1, -1, -1, -1);
			var wishes = await connection.GetRankAsync<Wish>("Wish", id, _Time.UtcNow).CAF();
			return new(source, character, claims, likes, wishes);
		}

		public async Task<IReadOnlyList<Character>> GetCharactersAsync()
		{
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
			", null).CAF();
		}

		public async Task<IReadOnlyList<Character>> GetCharactersAsync(Source source)
		{
			var param = new { source.SourceId };
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE SourceId = @SourceId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Character>> GetCharactersAsync(IEnumerable<long> ids)
		{
			var param = new { Ids = ids };
			return await GetManyAsync<Character>(@"
				SELECT *
				FROM Character
				WHERE CharacterId IN @Ids
			", param).CAF();
		}

		public async Task<Claim> GetClaimAsync(ulong guildId, Character character)
		{
			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			return await GetOneAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<Claim> GetClaimAsync(User user, Character character)
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

		public async Task<IReadOnlyList<Claim>> GetClaimsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<Claim>(@"
				SELECT *
				FROM Claim
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Claim>> GetClaimsAsync(User user)
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

		public async Task<IReadOnlyList<Image>> GetImagesAsync(Character character)
		{
			var param = new { character.CharacterId };
			return await GetManyAsync<Image>(@"
				SELECT *
				FROM Image
				WHERE CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<Source> GetSourceAsync(long sourceId)
		{
			var param = new { SourceId = sourceId };
			return await GetOneAsync<Source>(@"
				SELECT *
				FROM Source
				WHERE SourceId = @SourceId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Source>> GetSourcesAsync(IEnumerable<long> ids)
		{
			var param = new { Ids = ids };
			return await GetManyAsync<Source>(@"
				SELECT *
				FROM Source
				WHERE SourceId IN @Ids
			", param).CAF();
		}

		public async Task<Character> GetUnclaimedCharacter(ulong guildId)
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

		public async Task<User> GetUserAsync(ulong guildId, ulong userId)
		{
			var param = new { GuildId = guildId.ToString(), UserId = userId.ToString() };
			return await GetOneAsync<User>(@"
				SELECT GuildId, UserId
				FROM User
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId, Character character)
		{
			var param = new { GuildId = guildId.ToString(), character.CharacterId };
			return await GetManyAsync<Wish>(@"
				SELECT *
				FROM Wish
				WHERE GuildId = @GuildId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Wish>> GetWishesAsync(User user)
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

		public async Task<int> TradeAsync(IEnumerable<Trade> trades)
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

		public async Task UpdateClaimImageUrlAsync(Claim claim, string? url)
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
	}
}