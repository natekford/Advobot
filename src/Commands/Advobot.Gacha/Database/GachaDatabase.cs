using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.Relationships;
using Advobot.Utilities;
using AdvorangesUtils;
using Dapper;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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

			await connection.ExecuteAsync(@"CREATE TABLE Source
			(
				SourceId					INTEGER NOT NULL PRIMARY KEY,
				Name						TEXT NOT NULL,
				ThumbnailUrl				TEXT,
				TimeCreated					INTEGER NOT NULL
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Character
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
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Alias
			(
				CharacterId					INTEGER NOT NULL,
				Name						TEXT NOT NULL,
				IsSpoiler					INTEGER NOT NULL,
				PRIMARY KEY(CharacterId, Name)
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Image
			(
				CharacterId					INTEGER NOT NULL,
				Url							TEXT NOT NULL,
				PRIMARY KEY(CharacterId, Url),
				FOREIGN KEY(CharacterId) REFERENCES Character(CharacterId) ON DELETE CASCADE
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE User
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId)
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Claim
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				ImageUrl					TEXT,
				IsPrimaryMarriage			INTEGER NOT NULL,
				TimeCreated					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, CharacterId)
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Wish
			(
				GuildId						TEXT NOT NULL,
				UserId						TEXT NOT NULL,
				CharacterId					INTEGER NOT NULL,
				TimeCreated					INTEGER NOT NULL,
				PRIMARY KEY(GuildId, UserId, CharacterId)
			)").CAF();

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

		public Task<User> GetUserAsync(ulong guildId, ulong userId)
		{
			return Task.FromResult(new User
			{
				GuildId = guildId.ToString(),
				UserId = userId.ToString(),
			});
			/* Uncomment this when User has more properties than just two ids
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId.ToString(), UserId = userId.ToString() };
			var query = await connection.QueryAsync<User>(@"
				SELECT GuildId, UserId
				FROM User
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.SingleOrDefault();*/
		}
		public Task AddUserAsync(User user)
		{
			return Task.CompletedTask;
			/* Uncomment this when User has more properties than just two ids
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			", user).CAF();*/
		}

		public async Task<Source> GetSourceAsync(long sourceId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { SourceId = sourceId };
			var query = await connection.QueryAsync<Source>(@"
				SELECT SourceId, Name, ThumbnailUrl, TimeCreated
				FROM Source
				WHERE SourceId = @SourceId
			", param).CAF();
			return query.SingleOrDefault();
		}
		public async Task AddSourceAsync(Source source)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Source
				( SourceId, Name, ThumbnailUrl, TimeCreated )
				VALUES
				( @SourceId, @Name, @ThumbnailUrl, @TimeCreated )
			", source).CAF();
			source.SourceId = connection.LastInsertRowId;
		}

		public async Task<IReadOnlyList<Character>> GetCharactersAsync(Source source)
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
		public async Task<IReadOnlyList<Character>> GetCharactersAsync(IEnumerable<long> ids)
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
		public async Task<Character> GetCharacterAsync(long characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { CharacterId = characterId };
			var query = await connection.QueryAsync<Character>(@"
				SELECT SourceId, CharacterId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated
				FROM Character
				WHERE CharacterId = @CharacterId
			", param).CAF();
			return query.SingleOrDefault();
		}
		public async Task<Character> GetUnclaimedCharacter(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId };
			var query = await connection.QueryAsync<Character>(@"
				SELECT SourceId, CharacterId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated
				From Character
				WHERE CharacterId IN
				(
					SELECT t1.CharacterId
					From Character t1
					WHERE NOT EXISTS
					(
						SELECT t2.CharacterId
						FROM Claim t2 WHERE t2.GuildId = @GuildId AND t1.CharacterId = t2.CharacterId
					)
					ORDER BY RANDOM() LIMIT 1
				)
			", param).CAF();
			return query.SingleOrDefault();
		}
		public async Task AddCharacterAsync(Character character)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Character
				( SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated )
				VALUES
				( @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter, @TimeCreated )
			", character).CAF();
			character.CharacterId = connection.LastInsertRowId;
		}
		public async Task AddCharactersAsync(IEnumerable<Character> characters)
		{
			//Scope is needed to make the bulk adding not take ages
			using var scope = new TransactionScope();
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
				INSERT INTO Character
				( SourceId, Name, GenderIcon, Gender, RollType, FlavorText, IsFakeCharacter, TimeCreated )
				VALUES
				( @SourceId, @Name, @GenderIcon, @Gender, @RollType, @FlavorText, @IsFakeCharacter, @TimeCreated )
			", characters).CAF();
			scope.Complete();
		}
		public async Task<CharacterMetadata> GetCharacterMetadataAsync(Character character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var id = character.CharacterId;
			var source = await GetSourceAsync(character.SourceId).CAF();
			var claims = await connection.GetRankAsync<Claim>("Claim", id).CAF();
			var likes = new AmountAndRank("Likes", -1, -1);
			var wishes = await connection.GetRankAsync<Wish>("Wish", id).CAF();
			return new CharacterMetadata(source, character, claims, likes, wishes);
		}

		public async Task<IReadOnlyList<Claim>> GetClaimsAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated
				FROM Claim
				WHERE GuildId = @GuildId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<IReadOnlyList<Claim>> GetClaimsAsync(User user)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<Claim> GetClaimAsync(User user, Character character)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { user.GuildId, user.UserId, character.CharacterId };
			var query = await connection.QueryAsync<Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated
				FROM Claim
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
			return query.SingleOrDefault();
		}
		public async Task AddClaimAsync(Claim claim)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Claim
				( GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryMarriage, @TimeCreated )
			", claim).CAF();
		}
		public async Task AddClaimsAsync(IEnumerable<Claim> claims)
		{
			//Scope is needed to make the bulk adding not take ages
			using var scope = new TransactionScope();
			using var connection = await GetConnectionAsync().CAF();

			await connection.ExecuteAsync(@"
				INSERT INTO Claim
				( GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryMarriage, @TimeCreated )
			", claims).CAF();
			scope.Complete();
		}
		public async Task UpdateClaimImageUrlAsync(Claim claim, string? url)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { claim.GuildId, claim.UserId, claim.CharacterId, Url = url };
			await connection.QueryAsync(@"
				UPDATE Claim
				SET ImageUrl = @Url
				WHERE GuildId = @GuildId AND UserId = @UserId AND CharacterId = @CharacterId
			", param).CAF();
		}

		public async Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var param = new { GuildId = guildId };
			var query = await connection.QueryAsync<Wish>(@"
				SELECT GuildId, UserId, CharacterId, TimeCreated
				FROM Wish
				WHERE GuildId = @GuildId
			", param).CAF();
			return query.ToArray();
		}
		public async Task<IReadOnlyList<Wish>> GetWishesAsync(User user)
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
		public async Task AddWishAsync(Wish wish)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Wish
				( GuildId, UserId, CharacterId, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @TimeCreated )
			", wish).CAF();
		}

		public async Task<IReadOnlyList<Image>> GetImagesAsync(Character character)
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
		public async Task AddImageAsync(Image image)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"
				INSERT INTO Image
				( CharacterId, Url )
				VALUES
				( @CharacterId, @Url )
			", image).CAF();
		}
	}

	public static class GachaDatabaseUtils
	{
		public static Task<Character> GetRandomCharacterAsync(
			this GachaDatabase db,
			IGuild guild)
			=> db.GetUnclaimedCharacter(guild.Id);
		public static Task<IReadOnlyList<Wish>> GetWishesAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> throw new NotImplementedException();
			//=> db.GetWishesAsync(guild.Id, character.CharacterId);
		public static Task<User> GetUserAsync(
			this GachaDatabase db,
			IGuildUser user)
			=> db.GetUserAsync(user.GuildId, user.Id);
		/*
		public static Task<Claim> GetMarriageAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> db.GetClaimAsync(guild.Id, character.CharacterId);*/

		public static async Task<AmountAndRank> GetRankAsync<T>(
			this SQLiteConnection connection,
			string tableName,
			long id)
			where T : ICharacterChild
		{
			var query = await connection.QueryAsync<int>($@"
				SELECT CharacterId
				FROM {tableName}
			").CAF();

			//Find out how many exist for each character
			var dict = new Dictionary<long, int>();
			foreach (var cId in query)
			{
				dict.TryGetValue(cId, out var curr);
				dict[cId] = curr + 1;
			}

			//Find ones with a higher rank than the wanted one
			var rank = 1;
			var amount = dict.TryGetValue(id, out var val) ? val : 0;
			foreach (var kvp in dict)
			{
				if (kvp.Value > amount)
				{
					++rank;
				}
			}
			return new AmountAndRank(tableName, amount, rank);
		}
	}
}
