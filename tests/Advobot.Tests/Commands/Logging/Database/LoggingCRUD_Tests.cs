using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Logging.Database;

[TestClass]
public sealed class LoggingCRUD_Tests : Database_Tests<LoggingDatabase>
{
	private const ulong GUILD_ID = ulong.MaxValue;
	private const ulong IMAGE_LOG_ID = 73;
	private const ulong MOD_LOG_ID = 69;
	private const ulong SERVER_LOG_ID = ulong.MaxValue / 2;

	[TestMethod]
	public async Task IgnoredLogChannelsCRUD_Test()
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

	[TestMethod]
	public async Task LogActionCRUD_Test()
	{
		{
			var retrieved = await Db.GetLogActionsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.IsEmpty(retrieved);
		}

		var toInsert = new[]
		{
			LogAction.MessageDeleted,
			LogAction.MessageReceived,
			LogAction.MessageUpdated,
			LogAction.UserJoined,
		};
		{
			await Db.AddLogActionsAsync(GUILD_ID, toInsert).ConfigureAwait(false);

			var retrieved = await Db.GetLogActionsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length, retrieved.Count);
			Assert.AreEqual(toInsert.Length, toInsert.Intersect(retrieved).Count());
		}

		var toRemove = new[]
		{
			LogAction.MessageDeleted,
			LogAction.MessageReceived,
		};
		{
			await Db.DeleteLogActionsAsync(GUILD_ID, toRemove).ConfigureAwait(false);

			var retrieved = await Db.GetLogActionsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(toInsert.Length - toRemove.Length, retrieved.Count);
			Assert.AreEqual(0, toRemove.Intersect(retrieved).Count());
		}
	}

	[TestMethod]
	public async Task LogChannelCRUD_Test()
	{
		{
			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(0UL, retrieved.ImageLogId);
			Assert.AreEqual(0UL, retrieved.ModLogId);
			Assert.AreEqual(0UL, retrieved.ServerLogId);
		}

		//Add image log
		{
			await Db.UpsertLogChannelAsync(Log.Image, GUILD_ID, IMAGE_LOG_ID).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
			Assert.AreEqual(0UL, retrieved.ModLogId);
			Assert.AreEqual(0UL, retrieved.ServerLogId);
		}

		//Add mod log
		{
			await Db.UpsertLogChannelAsync(Log.Mod, GUILD_ID, MOD_LOG_ID).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
			Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
			Assert.AreEqual(0UL, retrieved.ServerLogId);
		}

		//Add server log
		{
			await Db.UpsertLogChannelAsync(Log.Server, GUILD_ID, SERVER_LOG_ID).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
			Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
			Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
		}

		//Remove image log
		{
			await Db.UpsertLogChannelAsync(Log.Image, GUILD_ID, null).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(0UL, retrieved.ImageLogId);
			Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
			Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
		}

		//Remove mod log
		{
			await Db.UpsertLogChannelAsync(Log.Mod, GUILD_ID, null).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(0UL, retrieved.ImageLogId);
			Assert.AreEqual(0UL, retrieved.ModLogId);
			Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
		}

		//Remove server log
		{
			await Db.UpsertLogChannelAsync(Log.Server, GUILD_ID, null).ConfigureAwait(false);

			var retrieved = await Db.GetLogChannelsAsync(GUILD_ID).ConfigureAwait(false);
			Assert.AreEqual(0UL, retrieved.ImageLogId);
			Assert.AreEqual(0UL, retrieved.ModLogId);
			Assert.AreEqual(0UL, retrieved.ServerLogId);
		}
	}
}