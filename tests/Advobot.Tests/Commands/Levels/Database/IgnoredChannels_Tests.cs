
using Advobot.Levels.Database;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Levels.Database
{
	[TestClass]
	public sealed class IgnoredChannels_Tests
		: DatabaseTestsBase<LevelDatabase, FakeSQLiteConnectionString>
	{
		private const ulong GUILD_ID = ulong.MaxValue;

		[TestMethod]
		public async Task IgnoredLogChannelsInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			{
				var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0, retrieved.Count);
			}

			var toInsert = new ulong[]
			{
				73,
				69,
				420,
				1337,
			};
			{
				await db.AddIgnoredChannelsAsync(GUILD_ID, toInsert).CAF();

				var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(toInsert.Length, retrieved.Count);
				Assert.AreEqual(toInsert.Length, toInsert.Intersect(retrieved).Count());
			}

			var toRemove = new ulong[]
			{
				73,
				69,
			};
			{
				await db.DeleteIgnoredChannelsAsync(GUILD_ID, toRemove).CAF();

				var retrieved = await db.GetIgnoredChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(toInsert.Length - toRemove.Length, retrieved.Count);
				Assert.AreEqual(0, toRemove.Intersect(retrieved).Count());
			}
		}
	}
}