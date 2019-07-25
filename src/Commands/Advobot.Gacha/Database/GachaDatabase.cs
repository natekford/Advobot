using Advobot.Gacha.Metadata;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Utilities;
using AdvorangesUtils;
using Dapper;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabase
	{
		private readonly IDatabaseStarter _Starter;

		public GachaDatabase(IServiceProvider provider)
		{
			_Starter = provider.GetRequiredService<IDatabaseStarter>();
		}

		private async Task<IReadOnlyList<string>> GetTablesAsync()
		{
			using var connection = await GetConnectionAsync().CAF();

			return (await connection.QueryAsync<string>(@"
				SELECT name FROM sqlite_master
				WHERE type='table'
				ORDER BY name;
			").CAF()).ToList();
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
				SourceId					INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
				Name						TEXT NOT NULL,
				ThumbnailUrl				TEXT,
				TimeCreated					INTEGER NOT NULL
			)").CAF();

			await connection.ExecuteAsync(@"CREATE TABLE Character
			(
				CharacterId					INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
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

		private async Task<SQLiteConnection> GetConnectionAsync()
		{
			var conn = new SQLiteConnection(_Starter.GetConnectionString());
			await conn.OpenAsync().CAF();
			return conn;
		}

		public async Task<User> GetUserAsync(ulong guildId, ulong userId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var results = (await connection.QueryAsync<User>(@"
				SELECT GuildId, UserId
				FROM User
				WHERE GuildId = @GuildId AND UserId = @UserId
			", new { GuildId = guildId.ToString(), UserId = userId.ToString() }).CAF());
			return results.SingleOrDefault();
		}
		public async Task AddUserAsync(User user)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"INSERT INTO User
				( GuildId, UserId )
				VALUES
				( @GuildId, @UserId )
			", user).CAF();
		}

		public async Task<IReadOnlyList<Claim>> GetClaimsAsync(User user)
		{
			using var connection = await GetConnectionAsync().CAF();

			var result = (await connection.QueryAsync<Claim, Character, Claim>(@"
				SELECT GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated
				FROM Marriage A INNER JOIN Character B ON A.CharacterId = B.CharacterId
				WHERE GuildId = @GuildId AND UserId = @UserId
			",
			(m, c) => { m.Character = c; m.User = user; return m; },
			new { user.GuildId, user.UserId },
			splitOn: "CharacterId").CAF()).ToList();
			return result;
		}
		public async Task AddClaimAsync(Claim claim)
		{
			using var connection = await GetConnectionAsync().CAF();

			await connection.QueryAsync(@"INSERT INTO Marriage
				( GuildId, UserId, CharacterId, ImageUrl, IsPrimaryMarriage, TimeCreated )
				VALUES
				( @GuildId, @UserId, @CharacterId, @ImageUrl, @IsPrimaryMarriage, @TimeCreated )
			", claim).CAF();
		}
		public async Task UpdateClaimImageUrlAsync(Claim claim, string? url)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();
		}

		public async Task<Character> GetRandomCharacterAsync(ulong guildId)
		{
			using var connection = await GetConnectionAsync().CAF();

			var sql = @"";
			var results = await connection.QueryAsync<Character>(sql, new { guildId }).CAF();
			throw new NotImplementedException();

			/*
			var untaken = connection.Characters.Where(c => !connection.Marriages.Any(m =>
				m.GuildId == guildId && m.CharacterId == c.CharacterId)
			);
			var count = untaken.Count();
			var rng = new Random().Next(1, count + 1);
			return untaken.Skip(rng).FirstOrDefaultAsync();*/
		}
		public async Task<IReadOnlyList<Wish>> GetWishesAsync(ulong guildId, int characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();

			/*
			var filtered = context.Wishes.Where(x => 
				x.User.GuildId == guildId && x.Character.CharacterId == characterId);
			return await filtered.ToArrayAsync().CAF();*/
		}
		public async Task<Claim> GetMarriageAsync(ulong guildId, int characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();

			/*
			return context.Marriages.FindAsync(guildId, characterId);*/
		}
		public async Task<Source> GetSourceAsync(int sourceId)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();

			/*
			return context.Sources
				.Include(x => x.Characters)
					.ThenInclude(x => x.Images)
						.ThenInclude(x => x.Character)
				.SingleOrDefaultAsync(x => x.SourceId == sourceId);*/
		}
		public async Task<Character> GetCharacterAsync(int characterId)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();

			/*
			return context.Characters
				.Include(x => x.Images)
					.ThenInclude(x => x.Character)
				.Include(x => x.Source)
				.SingleOrDefaultAsync(x => x.CharacterId == characterId);*/
		}

		public async Task<CharacterMetadata> GetCharacterMetadataAsync(Character character)
		{
			using var connection = await GetConnectionAsync().CAF();

			throw new NotImplementedException();

			/*
			var claims = context.Marriages.GetRankAsync(character.CharacterId, "Claims");
			var likes = new AmountAndRank("Likes", -1, -1);
			var wishes = context.Wishes.GetRankAsync(character.CharacterId, "Wishes");
			return new CharacterMetadata(character, claims, likes, wishes);*/
		}
	}

	public static class GachaDatabaseUtils
	{
		public static Task<Character> GetRandomCharacterAsync(
			this GachaDatabase db,
			IGuild guild)
			=> db.GetRandomCharacterAsync(guild.Id);
		public static Task<IReadOnlyList<Wish>> GetWishesAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> db.GetWishesAsync(guild.Id, character.CharacterId);
		public static Task<User> GetUserAsync(
			this GachaDatabase db,
			IGuildUser user)
			=> db.GetUserAsync(user.GuildId, user.Id);
		public static Task<Claim> GetMarriageAsync(
			this GachaDatabase db,
			IGuild guild,
			Character character)
			=> db.GetMarriageAsync(guild.Id, character.CharacterId);
	}
}
