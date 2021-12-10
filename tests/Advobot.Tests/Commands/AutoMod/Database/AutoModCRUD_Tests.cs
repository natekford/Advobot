using Advobot.AutoMod;
using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.AutoMod.Database;

[TestClass]
public sealed class AutoModCRUD_Tests
	: DatabaseTestsBase<AutoModDatabase, FakeSQLiteConnectionString>
{
	private const ulong GUILD_ID = 73;
	private const ulong ROLE_ID = 1337;
	private const ulong USER_ID = ulong.MaxValue;

	[TestMethod]
	public async Task AutoModSettingsCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var settings = new AutoModSettings
		(
			GuildId: GUILD_ID,
			Ticks: TimeSpan.FromSeconds(73).Ticks,
			IgnoreAdmins: true,
			IgnoreHigherHierarchy: true
		);

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

		settings = settings with
		{
			Ticks = TimeSpan.FromSeconds(42).Ticks,
			IgnoreAdmins = false,
		};

		await AssertEqualAsync().CAF();
	}

	[TestMethod]
	public async Task BannedNameCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var phrase = new BannedPhrase
		(
			GuildId: GUILD_ID,
			IsContains: false,
			IsName: true,
			IsRegex: true,
			Phrase: "joe",
			PunishmentType: PunishmentType.Ban
		);

		async Task AssertEqualAsync()
		{
			await db.UpsertBannedPhraseAsync(phrase).CAF();

			var retrieved = (await db.GetBannedNamesAsync(phrase.GuildId).CAF()).Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(phrase.GuildId, retrieved.GuildId);
			Assert.AreEqual(phrase.IsContains, retrieved.IsContains);
			Assert.AreEqual(phrase.IsRegex, retrieved.IsRegex);
			Assert.AreEqual(phrase.Phrase, retrieved.Phrase);
			Assert.AreEqual(phrase.PunishmentType, retrieved.PunishmentType);
		}

		await AssertEqualAsync().CAF();

		phrase = phrase with
		{
			IsContains = false,
			PunishmentType = PunishmentType.Kick,
		};

		await AssertEqualAsync().CAF();

		phrase = phrase with
		{
			Phrase = "not joe",
		};
		await db.UpsertBannedPhraseAsync(phrase).CAF();
		var retrieved = await db.GetBannedNamesAsync(GUILD_ID).CAF();
		Assert.AreEqual(2, retrieved.Count);
	}

	[TestMethod]
	public async Task BannedPhraseCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var phrase = new BannedPhrase
		(
			GuildId: GUILD_ID,
			IsContains: false,
			IsName: false,
			IsRegex: true,
			Phrase: "joe",
			PunishmentType: PunishmentType.Ban
		);

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

		phrase = phrase with
		{
			IsContains = false,
			PunishmentType = PunishmentType.Kick,
		};

		await AssertEqualAsync().CAF();

		phrase = phrase with
		{
			Phrase = "not joe",
		};
		await db.UpsertBannedPhraseAsync(phrase).CAF();
		var retrieved = await db.GetBannedPhrasesAsync(GUILD_ID).CAF();
		Assert.AreEqual(2, retrieved.Count);
	}

	[TestMethod]
	public async Task ChannelSettingsCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var settings = new ChannelSettings
		{
			GuildId = GUILD_ID,
			ChannelId = ROLE_ID,
			IsImageOnly = true,
		};

		async Task AssertEqualAsync()
		{
			await db.UpsertChannelSettings(settings).CAF();

			var retrieved = await db!.GetChannelSettingsAsync(settings.ChannelId).CAF();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(settings.GuildId, retrieved!.GuildId);
			Assert.AreEqual(settings.ChannelId, retrieved.ChannelId);
			Assert.AreEqual(settings.IsImageOnly, retrieved.IsImageOnly);
		}

		await AssertEqualAsync().CAF();

		settings = settings with
		{
			IsImageOnly = false,
		};

		await AssertEqualAsync().CAF();
	}

	[TestMethod]
	public async Task PersistentRoleCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var role = new PersistentRole
		{
			GuildId = GUILD_ID,
			RoleId = ROLE_ID,
			UserId = USER_ID,
		};
		await db.AddPersistentRoleAsync(role).CAF();

		var list = new List<PersistentRole>();
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

		var prevention = new RaidPrevention
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

		prevention = prevention with
		{
			Enabled = false,
			Instances = 24,
			IntervalTicks = 2341234444,
			LengthTicks = 28888883838383,
			PunishmentType = PunishmentType.Ban,
			RoleId = 87,
			Size = 42,
		};

		await AssertEqualAsync().CAF();
	}

	[TestMethod]
	public async Task SelfRoleCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var selfRole = new SelfRole
		{
			GuildId = Context.Guild.Id,
			RoleId = 73,
			GroupId = 4,
		};

		async Task AssertEqualAsync()
		{
			await db.UpsertSelfRolesAsync(new[] { selfRole }).CAF();

			var retrieved = await db!.GetSelfRoleAsync(selfRole.RoleId).CAF();
			if (retrieved is null)
			{
				Assert.IsNotNull(retrieved);
				return;
			}
			Assert.AreEqual(selfRole.GuildId, retrieved.GuildId);
			Assert.AreEqual(selfRole.RoleId, retrieved.RoleId);
			Assert.AreEqual(selfRole.GroupId, retrieved.GroupId);
		}

		await AssertEqualAsync().CAF();

		selfRole = selfRole with
		{
			GroupId = 2
		};

		await AssertEqualAsync().CAF();

		await db.UpsertSelfRolesAsync(new[]
		{
				selfRole with
				{
					RoleId = 4,
				},
				selfRole with
				{
					RoleId = 5,
				},
			}).CAF();

		var ret = await db.GetSelfRolesAsync(Context.Guild.Id).CAF();
		Assert.AreEqual(3, ret.Count);

		await db.DeleteSelfRolesGroupAsync(Context.Guild.Id, selfRole.GroupId).CAF();
		var ret2 = await db.GetSelfRolesAsync(Context.Guild.Id).CAF();
		Assert.AreEqual(0, ret2.Count);
	}

	[TestMethod]
	public async Task SpamPreventionCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var prevention = new SpamPrevention
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

		prevention = prevention with
		{
			Enabled = false,
			Instances = 24,
			IntervalTicks = 2341234444,
			LengthTicks = 28888883838383,
			PunishmentType = PunishmentType.Ban,
			RoleId = 87,
			Size = 42,
		};

		await AssertEqualAsync().CAF();
	}
}