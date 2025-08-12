using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Punishments;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.AutoMod.Database;

[TestClass]
public sealed class AutoModCRUD_Tests
	: Database_Tests<AutoModDatabase, FakeSQLiteConnectionString>
{
	private const ulong GUILD_ID = 73;
	private const ulong ROLE_ID = 1337;
	private const ulong USER_ID = ulong.MaxValue;

	[TestMethod]
	public async Task AutoModSettingsCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		var settings = new AutoModSettings
		(
			GuildId: GUILD_ID,
			Ticks: TimeSpan.FromSeconds(73).Ticks,
			IgnoreAdmins: true,
			IgnoreHigherHierarchy: true
		);

		async Task AssertEqualAsync()
		{
			await db.UpsertAutoModSettingsAsync(settings).ConfigureAwait(false);

			var retrieved = await db!.GetAutoModSettingsAsync(settings.GuildId).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(settings.CheckDuration, retrieved.CheckDuration);
			Assert.AreEqual(settings.Duration, retrieved.Duration);
			Assert.AreEqual(settings.GuildId, retrieved.GuildId);
			Assert.AreEqual(settings.IgnoreAdmins, retrieved.IgnoreAdmins);
			Assert.AreEqual(settings.IgnoreHigherHierarchy, retrieved.IgnoreHigherHierarchy);
		}

		await AssertEqualAsync().ConfigureAwait(false);

		settings = settings with
		{
			Ticks = TimeSpan.FromSeconds(42).Ticks,
			IgnoreAdmins = false,
		};

		await AssertEqualAsync().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task BannedNameCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

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
			await db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);

			var retrieved = (await db.GetBannedNamesAsync(phrase.GuildId).ConfigureAwait(false)).Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(phrase.GuildId, retrieved.GuildId);
			Assert.AreEqual(phrase.IsContains, retrieved.IsContains);
			Assert.AreEqual(phrase.IsRegex, retrieved.IsRegex);
			Assert.AreEqual(phrase.Phrase, retrieved.Phrase);
			Assert.AreEqual(phrase.PunishmentType, retrieved.PunishmentType);
		}

		await AssertEqualAsync().ConfigureAwait(false);

		phrase = phrase with
		{
			IsContains = false,
			PunishmentType = PunishmentType.Kick,
		};

		await AssertEqualAsync().ConfigureAwait(false);

		phrase = phrase with
		{
			Phrase = "not joe",
		};
		await db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);
		var retrieved = await db.GetBannedNamesAsync(GUILD_ID).ConfigureAwait(false);
		Assert.HasCount(2, retrieved);
	}

	[TestMethod]
	public async Task BannedPhraseCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

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
			await db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);

			var retrieved = (await db.GetBannedPhrasesAsync(phrase.GuildId).ConfigureAwait(false)).Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(phrase.GuildId, retrieved.GuildId);
			Assert.AreEqual(phrase.IsContains, retrieved.IsContains);
			Assert.AreEqual(phrase.IsRegex, retrieved.IsRegex);
			Assert.AreEqual(phrase.Phrase, retrieved.Phrase);
			Assert.AreEqual(phrase.PunishmentType, retrieved.PunishmentType);
		}

		await AssertEqualAsync().ConfigureAwait(false);

		phrase = phrase with
		{
			IsContains = false,
			PunishmentType = PunishmentType.Kick,
		};

		await AssertEqualAsync().ConfigureAwait(false);

		phrase = phrase with
		{
			Phrase = "not joe",
		};
		await db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);
		var retrieved = await db.GetBannedPhrasesAsync(GUILD_ID).ConfigureAwait(false);
		Assert.HasCount(2, retrieved);
	}

	[TestMethod]
	public async Task ChannelSettingsCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		var settings = new ChannelSettings
		{
			GuildId = GUILD_ID,
			ChannelId = ROLE_ID,
			IsImageOnly = true,
		};

		async Task AssertEqualAsync()
		{
			await db.UpsertChannelSettings(settings).ConfigureAwait(false);

			var retrieved = await db!.GetChannelSettingsAsync(settings.ChannelId).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(settings.GuildId, retrieved!.GuildId);
			Assert.AreEqual(settings.ChannelId, retrieved.ChannelId);
			Assert.AreEqual(settings.IsImageOnly, retrieved.IsImageOnly);
		}

		await AssertEqualAsync().ConfigureAwait(false);

		settings = settings with
		{
			IsImageOnly = false,
		};

		await AssertEqualAsync().ConfigureAwait(false);
	}

	[TestMethod]
	public async Task PersistentRoleCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		var role = new PersistentRole
		{
			GuildId = GUILD_ID,
			RoleId = ROLE_ID,
			UserId = USER_ID,
		};
		await db.AddPersistentRoleAsync(role).ConfigureAwait(false);

		var list = new List<PersistentRole>();
		{
			var retrieved = await db.GetPersistentRolesAsync(role.GuildId, role.UserId).ConfigureAwait(false);
			Assert.HasCount(1, retrieved);
			list.AddRange(retrieved);
		}
		{
			var retrieved = await db.GetPersistentRolesAsync(role.GuildId).ConfigureAwait(false);
			Assert.HasCount(1, retrieved);
			list.AddRange(retrieved);
		}

		foreach (var retrieved in list)
		{
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(role.GuildId, retrieved.GuildId);
			Assert.AreEqual(role.RoleId, retrieved.RoleId);
			Assert.AreEqual(role.UserId, retrieved.UserId);
		}

		await db.DeletePersistentRoleAsync(list[0]).ConfigureAwait(false);
		var empty = await db.GetPersistentRolesAsync(role.GuildId).ConfigureAwait(false);
		Assert.IsEmpty(empty);
	}

	[TestMethod]
	public async Task SelfRoleCRUD_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);

		var selfRole = new SelfRole
		{
			GuildId = Context.Guild.Id,
			RoleId = 73,
			GroupId = 4,
		};

		async Task AssertEqualAsync()
		{
			await db.UpsertSelfRolesAsync([selfRole]).ConfigureAwait(false);

			var retrieved = await db!.GetSelfRoleAsync(selfRole.RoleId).ConfigureAwait(false);
			if (retrieved is null)
			{
				Assert.IsNotNull(retrieved);
				return;
			}
			Assert.AreEqual(selfRole.GuildId, retrieved.GuildId);
			Assert.AreEqual(selfRole.RoleId, retrieved.RoleId);
			Assert.AreEqual(selfRole.GroupId, retrieved.GroupId);
		}

		await AssertEqualAsync().ConfigureAwait(false);

		selfRole = selfRole with
		{
			GroupId = 2
		};

		await AssertEqualAsync().ConfigureAwait(false);

		await db.UpsertSelfRolesAsync(
		[
			selfRole with
			{
				RoleId = 4,
			},
			selfRole with
			{
				RoleId = 5,
			},
		]).ConfigureAwait(false);

		var ret = await db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
		Assert.HasCount(3, ret);

		await db.DeleteSelfRolesGroupAsync(Context.Guild.Id, selfRole.GroupId).ConfigureAwait(false);
		var ret2 = await db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
		Assert.IsEmpty(ret2);
	}
}