using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.Models;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Database
{
	[TestClass]
	public sealed class ClaimAllCharactersTest
		: Database_TestsBase<GachaDatabase, FakeSQLiteConnectionString>
	{
		public const int CHARACTER_COUNT = SOURCE_COUNT * CHARACTERS_PER_SOURCE;
		public const int CHARACTERS_PER_SOURCE = 25;
		public const int CLAIM_COUNT = (int)(CHARACTER_COUNT * CLAIM_PERCENTAGE);
		public const double CLAIM_PERCENTAGE = .9;
		public const ulong GUILD_ID = 73;
		public const int SOURCE_COUNT = 20;

		[TestMethod]
		public async Task ClaimAllCharacters_Test()
		{
			var db = await GetDatabaseAsync().CAF();
			var (_, characters) = await db.AddSourcesAndCharacters(SOURCE_COUNT, CHARACTERS_PER_SOURCE).CAF();

			//Claim the specified percentage of the created characters
			var ids = new HashSet<long>();
			var users = new List<IReadOnlyUser>();
			var claims = new List<IReadOnlyClaim>();
			foreach (var character in characters.Take(CLAIM_COUNT))
			{
				var user = GachaTestUtils.GenerateFakeUser(guildId: GUILD_ID);
				var claim = GachaTestUtils.GenerateFakeClaim(user, character);
				users.Add(user);
				claims.Add(claim);
				ids.Add(character.CharacterId);
			}
			Assert.AreEqual(CLAIM_COUNT, users.Count);
			Assert.AreEqual(CLAIM_COUNT, claims.Count);
			var addedUsers = await db.AddUsersAsync(users).CAF();
			Assert.AreEqual(CLAIM_COUNT, addedUsers);
			var addedClaims = await db.AddClaimsAsync(claims).CAF();
			Assert.AreEqual(CLAIM_COUNT, addedClaims);

			//Go through the remaining characters and claim them like users would through Discord
			var fakeUser = GachaTestUtils.GenerateFakeUser(guildId: GUILD_ID);
			while (true)
			{
				var retrieved = await db.GetUnclaimedCharacter(GUILD_ID).CAF();
				if (retrieved == null)
				{
					break;
				}
				else if (ids.TryGetValue(retrieved.CharacterId, out _))
				{
					Assert.Fail("Retrieved an already claimed character " +
						$"on retrieval #{ids.Count - CLAIM_COUNT + 1} " +
						$"with the character id {retrieved.CharacterId}.");
				}

				var fakeClaim = new Claim(fakeUser, retrieved);
				ids.Add(retrieved.CharacterId);
				await db.AddClaimAsync(fakeClaim).CAF();
			}

			//Make sure every single character has been claimed
			Assert.AreEqual(CHARACTER_COUNT, ids.Count);
			var endingClaims = await db.GetClaimsAsync(GUILD_ID).CAF();
			Assert.AreEqual(CHARACTER_COUNT, endingClaims.Count);
		}
	}
}