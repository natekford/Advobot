using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Utils;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.GachaTests.UnitTests
{
	[TestClass]
	public sealed class ClaimAllCharactersTest : DatabaseTestsBase
	{
		public const int CHARACTER_COUNT = 500;
		public const int CLAIM_COUNT = (int)(CHARACTER_COUNT * .9);

		[TestMethod]
		public async Task ClaimAllCharacters_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var fakeSource = GenerateFakeSource();
			await db.AddSourceAsync(fakeSource).CAF();

			//Create 500 fake characters
			var fakeCharacters = new List<IReadOnlyCharacter>();
			for (var i = 1; i <= CHARACTER_COUNT; ++i) //Start at id 1 and end at id 500
			{
				fakeCharacters.Add(GenerateFakeCharacter(fakeSource, i));
			}
			var addedFakeCharacters = await db.AddCharactersAsync(fakeCharacters).CAF();
			Assert.AreEqual(CHARACTER_COUNT, addedFakeCharacters);
			var retrievedFakeCharacters = await db.GetCharactersAsync(fakeSource).CAF();
			Assert.AreEqual(CHARACTER_COUNT, retrievedFakeCharacters.Count);

			//Create fake claims for 80% of the fake characters
			var fakeUser = GenerateFakeUser();
			var dict = new Dictionary<long, IReadOnlyClaim>();
			for (var i = 0; i < CLAIM_COUNT; ++i)
			{
				dict[i] = new Claim(fakeUser, fakeCharacters[i]);
			}
			var addedFakeClaims = await db.AddClaimsAsync(dict.Values).CAF();
			Assert.AreEqual(CLAIM_COUNT, addedFakeClaims);
			var retrievedFakeClaims = await db.GetClaimsAsync(fakeUser).CAF();
			Assert.AreEqual(CLAIM_COUNT, retrievedFakeClaims.Count);

			//Go through the remaining 20% characters and claim them like users would through Discord
			while (true)
			{
				var retrieved = await db.GetUnclaimedCharacter(fakeUser.GetGuildId()).CAF();
				if (retrieved == null)
				{
					break;
				}
				else if (dict.TryGetValue(retrieved.CharacterId, out _))
				{
					Assert.Fail($"Retrieved an already claimed character " +
						$"on retrieval #{dict.Count - CLAIM_COUNT + 1} " +
						$"with the character id {retrieved.CharacterId}.");
				}

				var fakeClaim = new Claim(fakeUser, retrieved);
				dict[retrieved.CharacterId] = fakeClaim;
				await db.AddClaimAsync(fakeClaim).CAF();
			}

			Assert.AreEqual(CHARACTER_COUNT, dict.Count);
			var claims = await db.GetClaimsAsync(fakeUser).CAF();
			Assert.AreEqual(CHARACTER_COUNT, claims.Count);
		}
	}
}
