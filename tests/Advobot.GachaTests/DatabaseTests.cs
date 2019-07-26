using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.GachaTests.Utils;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.GachaTests
{
	[TestClass]
	public class DatabaseTests
	{
		private readonly IServiceProvider _Provider;
		private readonly Random _Rng = new Random();

		public DatabaseTests()
		{
			_Provider = new ServiceCollection()
				.AddSingleton<GachaDatabase>()
				.AddSingleton<IDatabaseStarter, SQLiteTestDatabaseFactory>()
				.BuildServiceProvider();
		}

		private async Task<GachaDatabase> GetDatabaseAsync()
		{
			var db = _Provider.GetRequiredService<GachaDatabase>();
#pragma warning disable IDE0059 // Value assigned to symbol is never used
			var tables = await db.CreateDatabaseAsync().CAF();
#pragma warning restore IDE0059 // Value assigned to symbol is never used
			return db;
		}

		[TestMethod]
		public async Task UserInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeUser = GenerateFakeUser();
			await db.AddUserAsync(fakeUser).CAF();

			var guildId = ulong.Parse(fakeUser.GuildId);
			var userId = ulong.Parse(fakeUser.UserId);

			var retrieved = await db.GetUserAsync(guildId, userId).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeUser.UserId, retrieved.UserId);
			Assert.AreEqual(fakeUser.GuildId, retrieved.GuildId);
		}
		[TestMethod]
		public async Task SourceInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			await db.AddSourceAsync(fakeSource).CAF();

			var retrieved = await db.GetSourceAsync(fakeSource.SourceId).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeSource.SourceId, retrieved.SourceId);
			Assert.AreEqual(fakeSource.Name, retrieved.Name);
			Assert.AreEqual(fakeSource.ThumbnailUrl, retrieved.ThumbnailUrl);
			Assert.AreEqual(fakeSource.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task CharacterInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			var fakeCharacter = GenerateFakeCharacter(fakeSource);
			await db.AddCharacterAsync(fakeCharacter).CAF();

			var retrieved = await db.GetCharacterAsync(fakeCharacter.CharacterId).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeCharacter.SourceId, retrieved.SourceId);
			Assert.AreEqual(fakeCharacter.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(fakeCharacter.Name, retrieved.Name);
			Assert.AreEqual(fakeCharacter.GenderIcon, retrieved.GenderIcon);
			Assert.AreEqual(fakeCharacter.Gender, retrieved.Gender);
			Assert.AreEqual(fakeCharacter.RollType, retrieved.RollType);
			Assert.AreEqual(fakeCharacter.FlavorText, retrieved.FlavorText);
			Assert.AreEqual(fakeCharacter.IsFakeCharacter, retrieved.IsFakeCharacter);
			Assert.AreEqual(fakeCharacter.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task ClaimInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			var fakeCharacter = GenerateFakeCharacter(fakeSource);
			var fakeUser = GenerateFakeUser();
			var fakeClaim = GenerateFakeClaim(fakeUser, fakeCharacter);
			await db.AddClaimAsync(fakeClaim).CAF();

			var retrieved = await db.GetClaimAsync(fakeUser, fakeCharacter).CAF();
			Assert.AreNotEqual(null, retrieved);
			Assert.AreEqual(fakeClaim.GuildId, retrieved.GuildId);
			Assert.AreEqual(fakeClaim.UserId, retrieved.UserId);
			Assert.AreEqual(fakeClaim.CharacterId, retrieved.CharacterId);
			Assert.AreEqual(fakeClaim.ImageUrl, retrieved.ImageUrl);
			Assert.AreEqual(fakeClaim.IsPrimaryMarriage, retrieved.IsPrimaryMarriage);
			Assert.AreEqual(fakeClaim.TimeCreated, retrieved.TimeCreated);
		}
		[TestMethod]
		public async Task ClaimAllCharacters_Test()
		{
			const int CHARACTER_COUNT = 500;

			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			await db.AddSourceAsync(fakeSource).CAF();

			//Create 500 fake characters
			var fakeCharacters = new List<Character>();
			for (var i = 1; i <= CHARACTER_COUNT; ++i) //Start at id 1 and end at id 500
			{
				fakeCharacters.Add(GenerateFakeCharacter(fakeSource, i));
			}
			await db.AddCharactersAsync(fakeCharacters).CAF();

			//Create fake claims for 80% of the fake characters
			var fakeUser = GenerateFakeUser();
			var dict = new Dictionary<long, Claim>();
			for (var i = 0; i < CHARACTER_COUNT * .8; ++i)
			{
				dict[i] = new Claim(fakeUser, fakeCharacters[i]);
			}
			await db.AddClaimsAsync(dict.Values).CAF();

			//Go through the remaining 20% characters and claim them like users would through Discord
			var guildId = ulong.Parse(fakeUser.GuildId);
			while (true)
			{
				var retrieved = await db.GetUnclaimedCharacter(guildId).CAF();
				if (retrieved == null)
				{
					break;
				}
				else if (dict.TryGetValue(retrieved.CharacterId, out _))
				{
					Assert.Fail("Retrieved an already claimed character.");
				}

				var fakeClaim = new Claim(fakeUser, retrieved);
				dict[retrieved.CharacterId] = fakeClaim;
				await db.AddClaimAsync(fakeClaim).CAF();
			}

			Assert.AreEqual(CHARACTER_COUNT, dict.Count);
			var claims = await db.GetClaimsAsync(fakeUser).CAF();
			Assert.AreEqual(CHARACTER_COUNT, claims.Count);
		}

		private Source GenerateFakeSource(long sourceId = 1)
		{
			return new Source
			{
				SourceId = sourceId,
				Name = Guid.NewGuid().ToString(),
			};
		}
		private Character GenerateFakeCharacter(Source fakeSource, long characterId = 1)
		{
			return new Character(fakeSource)
			{
				CharacterId = characterId,
				Name = Guid.NewGuid().ToString(),
				GenderIcon = "\uD83D\uDE39",
				Gender = Gender.Other,
				RollType = RollType.All,
				IsFakeCharacter = true,
			};
		}
		private User GenerateFakeUser()
		{
			var userId = _Rng.NextUlong();
			var guildId = _Rng.NextUlong();

			return new User
			{
				UserId = userId.ToString(),
				GuildId = guildId.ToString(),
			};
		}
		private Claim GenerateFakeClaim(User user, Character character)
		{
			return new Claim(user, character)
			{
				IsPrimaryMarriage = _Rng.NextBool(),
			};
		}
	}
}
