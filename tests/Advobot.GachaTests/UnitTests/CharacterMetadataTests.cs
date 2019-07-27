using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.GachaTests.UnitTests
{
	[TestClass]
	public sealed class CharacterMetadataTests : DatabaseTestsBase
	{
		public const int SOURCE_COUNT = 20;
		public const int CHARACTERS_PER_SOURCE = 25;
		public const int CHARACTER_COUNT = SOURCE_COUNT * CHARACTERS_PER_SOURCE;

		[TestMethod]
		public async Task GetCharacterMetadata_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			{
				var fakeSources = new List<IReadOnlySource>();
				var fakeCharacters = new List<IReadOnlyCharacter>();
				for (var i = 1; i <= SOURCE_COUNT; ++i)
				{
					var fakeSource = GenerateFakeSource(i);
					fakeSources.Add(fakeSource);

					for (var j = 1; j <= CHARACTERS_PER_SOURCE; ++j)
					{
						var id = i * (CHARACTERS_PER_SOURCE - 1) + j;
						fakeCharacters.Add(GenerateFakeCharacter(fakeSource, id));
					}
				}

				var addedFakeSources = await db.AddSourcesAsync(fakeSources).CAF();
				Assert.AreEqual(SOURCE_COUNT, addedFakeSources);
				var addedFakeCharacters = await db.AddCharactersAsync(fakeCharacters).CAF();
				Assert.AreEqual(CHARACTER_COUNT, addedFakeCharacters);
			}

			var dict = new Dictionary<IReadOnlyCharacter, int>();
			{
				var fakeUsers = new List<IReadOnlyUser>();
				var fakeClaims = new List<IReadOnlyClaim>();
				for (var i = 1; i <= CHARACTER_COUNT; ++i)
				{
					var fakeCharacter = await db.GetCharacterAsync(i).CAF();
					var claimCount = Rng.Next(1, 25);
					dict[fakeCharacter] = claimCount;
					for (var j = 0; j < claimCount; ++j)
					{
						var fakeUser = GenerateFakeUser();
						var fakeClaim = GenerateFakeClaim(fakeUser, fakeCharacter);
						fakeUsers.Add(fakeUser);
						fakeClaims.Add(fakeClaim);
					}
				}

				Assert.AreEqual(fakeUsers.Count, fakeClaims.Count);
				var addedFakeUsers = await db.AddUsersAsync(fakeUsers).CAF();
				Assert.AreEqual(fakeUsers.Count, addedFakeUsers);
				var addedFakeClaims = await db.AddClaimsAsync(fakeClaims).CAF();
				Assert.AreEqual(fakeUsers.Count, addedFakeClaims);
			}

			{
				foreach (var kvp in dict)
				{
					var expectedRank = RankUtils.GetRank(dict.Values, kvp.Value);
					var metadata = await db.GetCharacterMetadataAsync(kvp.Key).CAF();

					Assert.AreEqual(expectedRank, metadata.Claims.Rank);
					Assert.AreEqual(kvp.Value, metadata.Claims.Amount);
				}
			}
		}
	}
}
