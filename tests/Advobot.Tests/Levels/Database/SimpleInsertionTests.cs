using System.Threading.Tasks;

using Advobot.Levels.Database;
using Advobot.Levels.Models;
using Advobot.Levels.ReadOnlyModels;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Levels.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests : DatabaseTestsBase
	{
		[TestMethod]
		public async Task UserInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var args = new SearchArgs(1, 2, 3);
			var user = new User(args);
			await AssertRetrievedUserAsync(db, args, user).CAF();

			var modified = user.AddXp(5);
			await AssertRetrievedUserAsync(db, args, modified).CAF();
		}

		private async Task AssertRetrievedUserAsync(LevelDatabase db, ISearchArgs args, IReadOnlyUser expected)
		{
			await db.UpsertUser(expected).CAF();

			var retrieved = await db.GetUserAsync(args).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(expected.ChannelId, retrieved.ChannelId);
			Assert.AreEqual(expected.Experience, retrieved.Experience);
			Assert.AreEqual(expected.GuildId, retrieved.GuildId);
			Assert.AreEqual(expected.MessageCount, retrieved.MessageCount);
			Assert.AreEqual(expected.UserId, retrieved.UserId);
		}
	}
}