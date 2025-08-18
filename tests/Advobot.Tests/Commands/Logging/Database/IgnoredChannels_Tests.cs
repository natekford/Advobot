using Advobot.Logging.Database;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Logging.Database;

[TestClass]
public sealed class IgnoredChannels_Tests : Database_Tests<LoggingDatabase>
{
	private const ulong GUILD_ID = ulong.MaxValue;

	[TestMethod]
	public async Task IgnoredLogChannelsInsertionAndRetrieval_Test()
	{
		{
			var retrieved = await Db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
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
			await Db.AddIgnoredChannelsAsync(GUILD_ID, toInsert).ConfigureAwait(false);

			var retrieved = await Db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length, retrieved.Count);
			Assert.AreEqual(toInsert.Length, toInsert.Intersect(retrieved).Count());
		}

		var toRemove = new ulong[]
		{
			73,
			69,
		};
		{
			await Db.DeleteIgnoredChannelsAsync(GUILD_ID, toRemove).ConfigureAwait(false);

			var retrieved = await Db.GetIgnoredChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length - toRemove.Length, retrieved.Count);
			Assert.AreEqual(0, toRemove.Intersect(retrieved).Count());
		}
	}
}