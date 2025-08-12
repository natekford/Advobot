using Advobot.MyCommands.Database;
using Advobot.MyCommands.Database.Models;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.MyCommands.Database;

[TestClass]
public sealed class DetectLanguageConfig_Tests
	: Database_Tests<MyCommandsDatabase, FakeSQLiteConnectionString>
{
	[TestMethod]
	public async Task DetectLanguageConfigInsertionAndRetrieval_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		{
			var retrieved = await db.GetDetectLanguageConfigAsync().ConfigureAwait(false);
			Assert.IsNull(retrieved.APIKey);
			Assert.AreEqual(new DetectLanguageConfig().ConfidenceLimit, retrieved.ConfidenceLimit);
			Assert.IsNull(retrieved.CooldownStartTicks);
			Assert.IsNull(retrieved.CooldownStart);
		}

		{
			var updated = await db.GetDetectLanguageConfigAsync().ConfigureAwait(false) with
			{
				APIKey = "joe",
				CooldownStartTicks = 888888888888888
			};
			await db.UpsertDetectLanguageConfigAsync(updated).ConfigureAwait(false);
			var retrieved = await db.GetDetectLanguageConfigAsync().ConfigureAwait(false);
			Assert.AreEqual(updated.APIKey, retrieved.APIKey);
			Assert.AreEqual(updated.ConfidenceLimit, retrieved.ConfidenceLimit);
			Assert.AreEqual(updated.CooldownStartTicks, retrieved.CooldownStartTicks);
			Assert.AreEqual(updated.CooldownStart, retrieved.CooldownStart);
		}
	}
}