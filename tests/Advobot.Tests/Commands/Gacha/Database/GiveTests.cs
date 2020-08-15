using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Gacha.ReadOnlyModels;
using Advobot.Gacha.Trading;
using Advobot.Tests.Commands.Gacha.Utilities;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Gacha.Database
{
	[TestClass]
	public sealed class GiveTests
		: DatabaseTestsBase<GachaDatabase, FakeSQLiteConnectionString>
	{
		public const int CHARACTERS_PER_SOURCE = 7;
		public const ulong GUILD_ID = 73;
		public const int SOURCE_COUNT = 1;
		public static readonly int[] USER_CLAIM_COUNTS = new[] { 5, 2 };

		[TestMethod]
		public async Task GiveMultiple_Test()
		{
			var (db, users, userChars) = await AddClaimsAsync().CAF();

			var trades = userChars[1].Select(x => new Trade(users[0], x));
			var modifiedClaims = await db.TradeAsync(trades).CAF();
			Assert.AreEqual(USER_CLAIM_COUNTS[1], modifiedClaims);

			var newClaims = await db.GetClaimsAsync(users[0]).CAF();
			Assert.AreNotEqual(0, newClaims.Count);
			Assert.AreEqual(USER_CLAIM_COUNTS.Sum(), newClaims.Count);
			foreach (var claim in newClaims)
			{
				Assert.AreEqual(users[0].UserId, claim.UserId);
			}
		}

		[TestMethod]
		public async Task GiveOne_Test()
		{
			var (db, users, userChars) = await AddClaimsAsync().CAF();

			var givenCharacter = userChars[1][0];
			var trade = new Trade(users[0], givenCharacter);
			var modifiedClaims = await db.TradeAsync(new[] { trade }).CAF();
			Assert.AreEqual(1, modifiedClaims);

			var newClaim = await db.GetClaimAsync(users[0], givenCharacter).CAF();
			Assert.IsNotNull(newClaim);
			Assert.AreEqual(users[0].UserId, newClaim.UserId);
		}

		[TestMethod]
		public async Task TradeMultiple_Test()
		{
			var (db, users, userChars) = await AddClaimsAsync().CAF();

			var trades = userChars[1].Select(x => new Trade(users[0], x))
				.Concat(userChars[0].Select(x => new Trade(users[1], x)));
			var modifiedClaims = await db.TradeAsync(trades).CAF();
			Assert.AreEqual(USER_CLAIM_COUNTS.Sum(), modifiedClaims);

			for (var i = 0; i < 2; ++i)
			{
				var oppositeUser = Math.Abs(i - 1);
				var newClaims = await db.GetClaimsAsync(users[i]).CAF();
				Assert.AreNotEqual(0, newClaims.Count);
				Assert.AreEqual(USER_CLAIM_COUNTS[oppositeUser], newClaims.Count);
				foreach (var claim in newClaims)
				{
					Assert.AreEqual(users[i].UserId, claim.UserId);
				}
			}
		}

		[TestMethod]
		public async Task TradeOne_Test()
		{
			var (db, users, userChars) = await AddClaimsAsync().CAF();

			var swappedCharacters = new[]
			{
				userChars[1][0],
				userChars[0][0],
			};
			var trades = new[]
			{
				new Trade(users[0], swappedCharacters[0]),
				new Trade(users[1], swappedCharacters[1]),
			};
			var modifiedClaims = await db.TradeAsync(trades).CAF();
			Assert.AreEqual(2, modifiedClaims);

			for (var i = 0; i < 2; ++i)
			{
				var newClaim = await db.GetClaimAsync(users[i], swappedCharacters[i]).CAF();
				Assert.IsNotNull(newClaim);
				Assert.AreEqual(users[i].UserId, newClaim.UserId);
			}
		}

		private async Task<(IGachaDatabase, IReadOnlyUser[], List<IReadOnlyCharacter>[])> AddClaimsAsync()
		{
			var db = await GetDatabaseAsync().CAF();
			var (_, characters) = await db.AddSourcesAndCharacters(SOURCE_COUNT, CHARACTERS_PER_SOURCE).CAF();

			var users = new[]
			{
				GachaTestUtils.GenerateFakeUser(guildId: GUILD_ID),
				GachaTestUtils.GenerateFakeUser(guildId: GUILD_ID),
			};
			var addedUsers = await db.AddUsersAsync(users).CAF();
			Assert.AreEqual(2, addedUsers);

			var userChars = new[]
			{
				new List<IReadOnlyCharacter>(),
				new List<IReadOnlyCharacter>(),
			};
			var claims = new List<IReadOnlyClaim>();
			var c = 0;
			for (var i = 0; i < users.Length; ++i)
			{
				for (var j = 0; j < USER_CLAIM_COUNTS[i]; ++j, ++c)
				{
					claims.Add(GachaTestUtils.GenerateFakeClaim(users[i], characters[c]));
					userChars[i].Add(characters[c]);
				}
			}
			var addedClaims = await db.AddClaimsAsync(claims).CAF();
			Assert.AreEqual(c, addedClaims);

			return (db, users, userChars);
		}
	}
}