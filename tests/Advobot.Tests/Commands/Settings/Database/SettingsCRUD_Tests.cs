using System.Linq;
using System.Threading.Tasks;

using Advobot.Settings.Database;
using Advobot.Settings.Models;
using Advobot.Settings.ReadOnlyModels;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Settings.Database
{
	[TestClass]
	public sealed class SettingsCRUD_Tests
		: DatabaseTestsBase<SettingsDatabase, FakeSQLiteConnectionString>
	{
		[TestMethod]
		public async Task CommandOverridesCRUD_Test()
		{
			const string COMMAND_ID = "joe";

			var db = await GetDatabaseAsync().CAF();

			var editable = new[]
			{
				new CommandOverride(Context.Guild)
				{
					CommandId = COMMAND_ID,
					Enabled = true,
					Priority = 7,
				},
				new CommandOverride(Context.User)
				{
					CommandId = COMMAND_ID,
					Enabled = true,
					Priority = 7,
				},
				new CommandOverride(Context.Guild)
				{
					CommandId = COMMAND_ID + "ba",
					Enabled = true,
					Priority = 6,
				},
			};

			await db.UpsertCommandOverridesAsync(editable).CAF();

			{
				var retrieved = await db.GetCommandOverridesAsync(Context.Guild.Id).CAF();
				var expected = editable
					.OrderBy(x => x.CommandId)
					.ThenByDescending(x => x.Priority)
					.ThenBy(x => x.TargetType)
					.ToArray();
				Assert.AreEqual(expected.Length, retrieved.Count);
				for (var i = 0; i < expected.Length; ++i)
				{
					IReadOnlyCommandOverride e = expected[i], r = retrieved[i];
					Assert.AreEqual(e.CommandId, r.CommandId);
					Assert.AreEqual(e.Enabled, r.Enabled);
					Assert.AreEqual(e.GuildId, r.GuildId);
					Assert.AreEqual(e.Priority, r.Priority);
					Assert.AreEqual(e.TargetId, r.TargetId);
					Assert.AreEqual(e.TargetType, r.TargetType);
				}
			}

			{
				var retrieved = await db.GetCommandOverridesAsync(Context.Guild.Id, COMMAND_ID).CAF();
				var expected = editable
					.Where(x => x.CommandId == COMMAND_ID)
					.OrderBy(x => x.CommandId)
					.ThenByDescending(x => x.Priority)
					.ThenBy(x => x.TargetType)
					.ToArray();
				Assert.AreEqual(expected.Length, retrieved.Count);
				for (var i = 0; i < expected.Length; ++i)
				{
					IReadOnlyCommandOverride e = expected[i], r = retrieved[i];
					Assert.AreEqual(e.CommandId, r.CommandId);
					Assert.AreEqual(e.Enabled, r.Enabled);
					Assert.AreEqual(e.GuildId, r.GuildId);
					Assert.AreEqual(e.Priority, r.Priority);
					Assert.AreEqual(e.TargetId, r.TargetId);
					Assert.AreEqual(e.TargetType, r.TargetType);
				}
			}

			foreach (var e in editable)
			{
				e.Enabled = false;
			}
			await db.UpsertCommandOverridesAsync(editable).CAF();

			{
				var retrieved = await db.GetCommandOverridesAsync(Context.Guild.Id).CAF();
				var expected = editable
					.OrderBy(x => x.CommandId)
					.ThenByDescending(x => x.Priority)
					.ThenBy(x => x.TargetType)
					.ToArray();
				Assert.AreEqual(expected.Length, retrieved.Count);
				for (var i = 0; i < expected.Length; ++i)
				{
					IReadOnlyCommandOverride e = expected[i], r = retrieved[i];
					Assert.AreEqual(e.CommandId, r.CommandId);
					Assert.AreEqual(e.Enabled, r.Enabled);
					Assert.AreEqual(e.GuildId, r.GuildId);
					Assert.AreEqual(e.Priority, r.Priority);
					Assert.AreEqual(e.TargetId, r.TargetId);
					Assert.AreEqual(e.TargetType, r.TargetType);
				}
			}

			var toDelete = editable.Where(x => x.CommandId != COMMAND_ID);
			await db.DeleteCommandOverridesAsync(toDelete).CAF();

			{
				var retrieved = await db.GetCommandOverridesAsync(Context.Guild.Id).CAF();
				var expected = editable
					.Where(x => x.CommandId == COMMAND_ID)
					.OrderBy(x => x.CommandId)
					.ThenByDescending(x => x.Priority)
					.ThenBy(x => x.TargetType)
					.ToArray();
				Assert.AreEqual(expected.Length, retrieved.Count);
				for (var i = 0; i < expected.Length; ++i)
				{
					IReadOnlyCommandOverride e = expected[i], r = retrieved[i];
					Assert.AreEqual(e.CommandId, r.CommandId);
					Assert.AreEqual(e.Enabled, r.Enabled);
					Assert.AreEqual(e.GuildId, r.GuildId);
					Assert.AreEqual(e.Priority, r.Priority);
					Assert.AreEqual(e.TargetId, r.TargetId);
					Assert.AreEqual(e.TargetType, r.TargetType);
				}
			}
		}
	}
}