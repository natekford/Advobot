using System.Linq;
using System.Threading.Tasks;

using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests
		: DatabaseTestsBase<LoggingDatabase, FakeSQLiteConnectionString>
	{
		private const ulong GUILD_ID = ulong.MaxValue;
		private const ulong IMAGE_LOG_ID = 73;
		private const ulong MOD_LOG_ID = 69;
		private const ulong SERVER_LOG_ID = ulong.MaxValue / 2;

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

		[TestMethod]
		public async Task LogActionInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			{
				var retrieved = await db.GetLogActionsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0, retrieved.Count);
			}

			var toInsert = new[]
			{
				LogAction.MessageDeleted,
				LogAction.MessageReceived,
				LogAction.MessageUpdated,
				LogAction.UserJoined,
			};
			{
				await db.AddLogActionsAsync(GUILD_ID, toInsert).CAF();

				var retrieved = await db.GetLogActionsAsync(GUILD_ID).CAF();
				Assert.AreEqual(toInsert.Length, retrieved.Count);
				Assert.AreEqual(toInsert.Length, toInsert.Intersect(retrieved).Count());
			}

			var toRemove = new[]
			{
				LogAction.MessageDeleted,
				LogAction.MessageReceived,
			};
			{
				await db.DeleteLogActionsAsync(GUILD_ID, toRemove).CAF();

				var retrieved = await db.GetLogActionsAsync(GUILD_ID).CAF();
				Assert.AreEqual(toInsert.Length - toRemove.Length, retrieved.Count);
				Assert.AreEqual(0, toRemove.Intersect(retrieved).Count());
			}
		}

		[TestMethod]
		public async Task LogChannelInsertionAndRetrieval_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			{
				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0UL, retrieved.ImageLogId);
				Assert.AreEqual(0UL, retrieved.ModLogId);
				Assert.AreEqual(0UL, retrieved.ServerLogId);
			}

			//Add image log
			{
				await db.UpsertLogChannelAsync(Log.Image, GUILD_ID, IMAGE_LOG_ID).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
				Assert.AreEqual(0UL, retrieved.ModLogId);
				Assert.AreEqual(0UL, retrieved.ServerLogId);
			}

			//Add mod log
			{
				await db.UpsertLogChannelAsync(Log.Mod, GUILD_ID, MOD_LOG_ID).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
				Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
				Assert.AreEqual(0UL, retrieved.ServerLogId);
			}

			//Add server log
			{
				await db.UpsertLogChannelAsync(Log.Server, GUILD_ID, SERVER_LOG_ID).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(IMAGE_LOG_ID, retrieved.ImageLogId);
				Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
				Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
			}

			//Remove image log
			{
				await db.UpsertLogChannelAsync(Log.Image, GUILD_ID, null).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0UL, retrieved.ImageLogId);
				Assert.AreEqual(MOD_LOG_ID, retrieved.ModLogId);
				Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
			}

			//Remove mod log
			{
				await db.UpsertLogChannelAsync(Log.Mod, GUILD_ID, null).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0UL, retrieved.ImageLogId);
				Assert.AreEqual(0UL, retrieved.ModLogId);
				Assert.AreEqual(SERVER_LOG_ID, retrieved.ServerLogId);
			}

			//Remove server log
			{
				await db.UpsertLogChannelAsync(Log.Server, GUILD_ID, null).CAF();

				var retrieved = await db.GetLogChannelsAsync(GUILD_ID).CAF();
				Assert.AreEqual(0UL, retrieved.ImageLogId);
				Assert.AreEqual(0UL, retrieved.ModLogId);
				Assert.AreEqual(0UL, retrieved.ServerLogId);
			}
		}
	}
}