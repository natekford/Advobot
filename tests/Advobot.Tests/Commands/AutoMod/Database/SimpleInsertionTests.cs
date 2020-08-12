using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests
		: Database_TestsBase<AutoModDatabase, FakeSQLiteConnectionString>
	{
		private const ulong GUILD_ID = 73;
		private const ulong ROLE_ID = 1337;
		private const ulong USER_ID = ulong.MaxValue;

		[TestMethod]
		public async Task AutoModSettingsCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new AutoModSettings(GUILD_ID)
			{
				Duration = TimeSpan.FromSeconds(73),
				IgnoreAdmins = true,
				IgnoreHigherHierarchy = true,
			};
			IReadOnlyAutoModSettings settings = editable;

			async Task AssertEqualAsync()
			{
				var retrieved = await db!.GetAutoModSettingsAsync(GUILD_ID).CAF();
				Assert.IsNotNull(retrieved);
				Assert.AreEqual(settings.CheckDuration, retrieved.CheckDuration);
				Assert.AreEqual(settings.Duration, retrieved.Duration);
				Assert.AreEqual(settings.GuildId, retrieved.GuildId);
				Assert.AreEqual(settings.IgnoreAdmins, retrieved.IgnoreAdmins);
				Assert.AreEqual(settings.IgnoreHigherHierarchy, retrieved.IgnoreHigherHierarchy);
			}

			await db.UpsertAutoModSettingsAsync(settings).CAF();
			await AssertEqualAsync().CAF();

			editable.Duration = TimeSpan.FromSeconds(42);
			editable.IgnoreAdmins = false;
			await db.UpsertAutoModSettingsAsync(settings).CAF();
			await AssertEqualAsync().CAF();
		}

		[TestMethod]
		public async Task BannedNameCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();
		}

		[TestMethod]
		public async Task BannedPhraseCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();
		}

		[TestMethod]
		public async Task ChannelSettingsCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();
		}

		[TestMethod]
		public async Task PersistentRoleCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			IReadOnlyPersistentRole role = new PersistentRole
			{
				GuildId = GUILD_ID,
				RoleId = ROLE_ID,
				UserId = USER_ID,
			};
			await db.AddPersistentRoleAsync(role).CAF();

			var list = new List<IReadOnlyPersistentRole>();
			{
				var retrieved = await db.GetPersistentRolesAsync(GUILD_ID, USER_ID).CAF();
				Assert.AreEqual(1, retrieved.Count);
				list.AddRange(retrieved);
			}
			{
				var retrieved = await db.GetPersistentRolesAsync(GUILD_ID).CAF();
				Assert.AreEqual(1, retrieved.Count);
				list.AddRange(retrieved);
			}

			foreach (var retrieved in list)
			{
				Assert.IsNotNull(retrieved);
				Assert.AreEqual(role.GuildId, retrieved.GuildId);
				Assert.AreEqual(role.RoleId, retrieved.RoleId);
				Assert.AreEqual(role.UserId, retrieved.UserId);
			}

			await db.DeletePersistentRoleAsync(list[0]).CAF();
			var empty = await db.GetPersistentRolesAsync(GUILD_ID).CAF();
			Assert.AreEqual(0, empty.Count);
		}
	}
}