using Advobot.Levels.Database;
using Advobot.Levels.Database.Models;
using Advobot.Levels.Utilities;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Levels.Database;

[TestClass]
public sealed class LevelsCRUD_Tests : Database_Tests<LevelDatabase>
{
	[TestMethod]
	public async Task UserCRUD_Test()
	{
		var args = new SearchArgs(1, 2, 3);
		var user = new User(args);

		async Task AssertEqualAsync()
		{
			await Db.UpsertUserAsync(user).ConfigureAwait(false);

			var retrieved = await Db.GetUserAsync(args).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(user.ChannelId, retrieved.ChannelId);
			Assert.AreEqual(user.Experience, retrieved.Experience);
			Assert.AreEqual(user.GuildId, retrieved.GuildId);
			Assert.AreEqual(user.MessageCount, retrieved.MessageCount);
			Assert.AreEqual(user.UserId, retrieved.UserId);
		}
		await AssertEqualAsync().ConfigureAwait(false);

		user = user.AddXp(5);
		await AssertEqualAsync().ConfigureAwait(false);
	}
}