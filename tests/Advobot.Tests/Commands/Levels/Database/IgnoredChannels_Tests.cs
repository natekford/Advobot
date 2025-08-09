using Advobot.Levels.Database;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Levels.Database;

[TestClass]
public sealed class IgnoredChannels_Tests
	: Database_Tests<LevelDatabase, FakeSQLiteConnectionString>
{
	private const ulong GUILD_ID = ulong.MaxValue;

	[TestMethod]
	public async Task IgnoredLogChannelsInsertionAndRetrieval_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		{
			var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.IsEmpty(retrieved);
		}

		var toInsert = new ulong[]
		{
				73,
				69,
				420,
				1337,
		};
		{
			await db.AddIgnoredChannelsAsync(GUILD_ID, toInsert).ConfigureAwait(false);

			var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length, retrieved.Count);
			Assert.AreEqual(toInsert.Length, toInsert.Intersect(retrieved).Count());
		}

		var toRemove = new ulong[]
		{
				73,
				69,
		};
		{
			await db.DeleteIgnoredChannelsAsync(GUILD_ID, toRemove).ConfigureAwait(false);

			var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length - toRemove.Length, retrieved.Count);
			Assert.AreEqual(0, toRemove.Intersect(retrieved).Count());
		}
	}
}