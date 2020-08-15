using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Database
{
	[TestClass]
	public sealed class SimpleInsertionTests
		: DatabaseTestsBase<AutoModDatabase, FakeSQLiteConnectionString>
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
				await db.UpsertAutoModSettingsAsync(settings).CAF();

				var retrieved = await db!.GetAutoModSettingsAsync(settings.GuildId).CAF();
				Assert.IsNotNull(retrieved);
				Assert.AreEqual(settings.CheckDuration, retrieved.CheckDuration);
				Assert.AreEqual(settings.Duration, retrieved.Duration);
				Assert.AreEqual(settings.GuildId, retrieved.GuildId);
				Assert.AreEqual(settings.IgnoreAdmins, retrieved.IgnoreAdmins);
				Assert.AreEqual(settings.IgnoreHigherHierarchy, retrieved.IgnoreHigherHierarchy);
			}

			await AssertEqualAsync().CAF();

			editable.Duration = TimeSpan.FromSeconds(42);
			editable.IgnoreAdmins = false;

			await AssertEqualAsync().CAF();
		}

		[TestMethod]
		public async Task BannedNameCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new BannedPhrase
			{
				GuildId = GUILD_ID,
				Phrase = "joe",
				IsRegex = true,
				IsName = true,
				PunishmentType = PunishmentType.Ban,
			};
			IReadOnlyBannedPhrase name = editable;

			async Task AssertEqualAsync()
			{
				await db.UpsertBannedPhraseAsync(name).CAF();

				var retrieved = (await db.GetBannedNamesAsync(name.GuildId).CAF()).Single();
				Assert.IsNotNull(retrieved);
				Assert.AreEqual(name.GuildId, retrieved.GuildId);
				Assert.AreEqual(name.IsContains, retrieved.IsContains);
				Assert.AreEqual(name.IsRegex, retrieved.IsRegex);
				Assert.AreEqual(name.Phrase, retrieved.Phrase);
				Assert.AreEqual(name.PunishmentType, retrieved.PunishmentType);
			}

			await AssertEqualAsync().CAF();

			editable.IsContains = false;
			editable.PunishmentType = PunishmentType.Kick;

			await AssertEqualAsync().CAF();

			editable.Phrase = "not joe";
			await db.UpsertBannedPhraseAsync(name).CAF();
			var retrieved = await db.GetBannedNamesAsync(GUILD_ID).CAF();
			Assert.AreEqual(2, retrieved.Count);
		}

		[TestMethod]
		public async Task BannedPhraseCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new BannedPhrase
			{
				GuildId = GUILD_ID,
				Phrase = "joe",
				IsRegex = true,
				PunishmentType = PunishmentType.Ban,
			};
			IReadOnlyBannedPhrase phrase = editable;

			async Task AssertEqualAsync()
			{
				await db.UpsertBannedPhraseAsync(phrase).CAF();

				var retrieved = (await db.GetBannedPhrasesAsync(phrase.GuildId).CAF()).Single();
				Assert.IsNotNull(retrieved);
				Assert.AreEqual(phrase.GuildId, retrieved.GuildId);
				Assert.AreEqual(phrase.IsContains, retrieved.IsContains);
				Assert.AreEqual(phrase.IsRegex, retrieved.IsRegex);
				Assert.AreEqual(phrase.Phrase, retrieved.Phrase);
				Assert.AreEqual(phrase.PunishmentType, retrieved.PunishmentType);
			}

			await AssertEqualAsync().CAF();

			editable.IsContains = false;
			editable.PunishmentType = PunishmentType.Kick;

			await AssertEqualAsync().CAF();

			editable.Phrase = "not joe";
			await db.UpsertBannedPhraseAsync(phrase).CAF();
			var retrieved = await db.GetBannedPhrasesAsync(GUILD_ID).CAF();
			Assert.AreEqual(2, retrieved.Count);
		}

		[TestMethod]
		public async Task ChannelSettingsCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new ChannelSettings
			{
				GuildId = GUILD_ID,
				ChannelId = ROLE_ID,
				IsImageOnly = true,
			};
			IReadOnlyChannelSettings settings = editable;

			async Task AssertEqualAsync()
			{
				await db.UpsertChannelSettings(settings).CAF();

				var retrieved = await db!.GetChannelSettingsAsync(settings.ChannelId).CAF();
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(settings.GuildId, retrieved.GuildId);
				Assert.AreEqual(settings.ChannelId, retrieved.ChannelId);
				Assert.AreEqual(settings.IsImageOnly, retrieved.IsImageOnly);
			}

			await AssertEqualAsync().CAF();

			editable.IsImageOnly = false;

			await AssertEqualAsync().CAF();
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
				var retrieved = await db.GetPersistentRolesAsync(role.GuildId, role.UserId).CAF();
				Assert.AreEqual(1, retrieved.Count);
				list.AddRange(retrieved);
			}
			{
				var retrieved = await db.GetPersistentRolesAsync(role.GuildId).CAF();
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
			var empty = await db.GetPersistentRolesAsync(role.GuildId).CAF();
			Assert.AreEqual(0, empty.Count);
		}

		[TestMethod]
		public async Task RaidPreventionCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new RaidPrevention
			{
				GuildId = GUILD_ID,
				Enabled = true,
				Instances = 73,
				IntervalTicks = 999999999,
				LengthTicks = 88888888,
				PunishmentType = PunishmentType.Kick,
				RoleId = ROLE_ID,
				Size = 24,
				RaidType = RaidType.Regular,
			};
			IReadOnlyRaidPrevention prevention = editable;

			async Task AssertEqualAsync()
			{
				await db.UpsertRaidPreventionAsync(prevention).CAF();

				var retrieved = await db!.GetRaidPreventionAsync(prevention.GuildId, prevention.RaidType).CAF();
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(prevention.GuildId, retrieved.GuildId);
				Assert.AreEqual(prevention.Enabled, retrieved.Enabled);
				Assert.AreEqual(prevention.Instances, retrieved.Instances);
				Assert.AreEqual(prevention.Interval, retrieved.Interval);
				Assert.AreEqual(prevention.Length, retrieved.Length);
				Assert.AreEqual(prevention.PunishmentType, retrieved.PunishmentType);
				Assert.AreEqual(prevention.RoleId, retrieved.RoleId);
				Assert.AreEqual(prevention.Size, retrieved.Size);
				Assert.AreEqual(prevention.RaidType, retrieved.RaidType);
			}

			await AssertEqualAsync().CAF();

			editable.Enabled = false;
			editable.Instances = 24;
			editable.IntervalTicks = 2341234444;
			editable.LengthTicks = 28888883838383;
			editable.PunishmentType = PunishmentType.Ban;
			editable.RoleId = 87;
			editable.Size = 42;

			await AssertEqualAsync().CAF();
		}

		[TestMethod]
		public async Task SpamPreventionCRUD_Test()
		{
			var db = await GetDatabaseAsync().CAF();

			var editable = new SpamPrevention
			{
				GuildId = GUILD_ID,
				Enabled = true,
				Instances = 73,
				IntervalTicks = 999999999,
				LengthTicks = 88888888,
				PunishmentType = PunishmentType.Kick,
				RoleId = ROLE_ID,
				Size = 24,
				SpamType = SpamType.Image,
			};
			IReadOnlySpamPrevention prevention = editable;

			async Task AssertEqualAsync()
			{
				await db.UpsertSpamPreventionAsync(prevention).CAF();

				var retrieved = await db!.GetSpamPreventionAsync(prevention.GuildId, prevention.SpamType).CAF();
				if (retrieved is null)
				{
					Assert.IsNotNull(retrieved);
					return;
				}
				Assert.AreEqual(prevention.GuildId, retrieved.GuildId);
				Assert.AreEqual(prevention.Enabled, retrieved.Enabled);
				Assert.AreEqual(prevention.Instances, retrieved.Instances);
				Assert.AreEqual(prevention.Interval, retrieved.Interval);
				Assert.AreEqual(prevention.Length, retrieved.Length);
				Assert.AreEqual(prevention.PunishmentType, retrieved.PunishmentType);
				Assert.AreEqual(prevention.RoleId, retrieved.RoleId);
				Assert.AreEqual(prevention.Size, retrieved.Size);
				Assert.AreEqual(prevention.SpamType, retrieved.SpamType);
			}

			await AssertEqualAsync().CAF();

			editable.Enabled = false;
			editable.Instances = 24;
			editable.IntervalTicks = 2341234444;
			editable.LengthTicks = 28888883838383;
			editable.PunishmentType = PunishmentType.Ban;
			editable.RoleId = 87;
			editable.Size = 42;

			await AssertEqualAsync().CAF();
		}
	}
}