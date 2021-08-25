using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.Utilities;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Database
{
	[TestClass]
	public sealed class CharacterMetadata_Tests
		: DatabaseTestsBase<GachaDatabase, FakeSQLiteConnectionString>
	{
		public const int CHARACTERS_PER_SOURCE = 25;
		public const int MAX_CLAIMS = 25;
		public const int MIN_CLAIMS = 1;
		public const int SOURCE_COUNT = 20;

		[TestMethod]
		public async Task GetCharacterMetadata_Test()
		{
			var db = await GetDatabaseAsync().CAF();
			var (_, characters) = await db.AddSourcesAndCharacters(SOURCE_COUNT, CHARACTERS_PER_SOURCE).CAF();

			//Set up a random number of claims on each character
			var dict = new Dictionary<Character, int>();
			var users = new List<User>();
			var claims = new List<Claim>();
			foreach (var character in characters)
			{
				var claimCount = Rng.Next(MIN_CLAIMS, MAX_CLAIMS);
				dict[character] = claimCount;
				for (var j = 0; j < claimCount; ++j)
				{
					var user = GachaTestUtils.GenerateFakeUser();
					var claim = GachaTestUtils.GenerateFakeClaim(user, character);
					users.Add(user);
					claims.Add(claim);
				}
			}
			Assert.AreEqual(users.Count, claims.Count);
			var addedUsers = await db.AddUsersAsync(users).CAF();
			Assert.AreEqual(users.Count, addedUsers);
			var addedClaims = await db.AddClaimsAsync(claims).CAF();
			Assert.AreEqual(users.Count, addedClaims);

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