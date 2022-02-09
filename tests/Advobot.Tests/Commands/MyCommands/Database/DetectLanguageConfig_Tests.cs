using Advobot.MyCommands.Database;
using Advobot.MyCommands.Models;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.MyCommands.Database;

[TestClass]
public sealed class DetectLanguageConfig_Tests
	: Database_Tests<MyCommandsDatabase, FakeSQLiteConnectionString>
{
	[TestMethod]
	public async Task DetectLanguageConfigInsertionAndRetrieval_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		{
			var retrieved = await db.GetDetectLanguageConfigAsync().CAF();
			Assert.AreEqual(null, retrieved.APIKey);
			Assert.AreEqual(new DetectLanguageConfig().ConfidenceLimit, retrieved.ConfidenceLimit);
			Assert.AreEqual(null, retrieved.CooldownStartTicks);
			Assert.AreEqual(null, retrieved.CooldownStart);
		}

		{
			var updated = await db.GetDetectLanguageConfigAsync().CAF() with
			{
				APIKey = "joe",
				CooldownStartTicks = 888888888888888
			};
			await db.UpsertDetectLanguageConfigAsync(updated).CAF();
			var retrieved = await db.GetDetectLanguageConfigAsync().CAF();
			Assert.AreEqual(updated.APIKey, retrieved.APIKey);
			Assert.AreEqual(updated.ConfidenceLimit, retrieved.ConfidenceLimit);
			Assert.AreEqual(updated.CooldownStartTicks, retrieved.CooldownStartTicks);
			Assert.AreEqual(updated.CooldownStart, retrieved.CooldownStart);
		}
	}
}