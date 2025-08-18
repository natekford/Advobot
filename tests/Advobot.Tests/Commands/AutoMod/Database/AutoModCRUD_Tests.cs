using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Punishments;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.AutoMod.Database;

[TestClass]
public sealed class AutoModCRUD_Tests : Database_Tests<AutoModDatabase>
{
	private const ulong GUILD_ID = 73;
	private const ulong ROLE_ID = 1337;
	private const ulong USER_ID = ulong.MaxValue;

	[TestMethod]
	public async Task AutoModSettingsCRUD_Test()
	{
		var settings = new AutoModSettings
		(
			GuildId: GUILD_ID,
			Ticks: TimeSpan.FromSeconds(73).Ticks,
			IgnoreAdmins: true,
			IgnoreHigherHierarchy: true
		);

		async Task AssertEqualAsync()
		{
			await Db.UpsertAutoModSettingsAsync(settings).ConfigureAwait(false);

			var retrieved = await Db!.GetAutoModSettingsAsync(settings.GuildId).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(settings, retrieved);
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
			await Db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);

			var retrieved = (await Db.GetBannedNamesAsync(phrase.GuildId).ConfigureAwait(false)).Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(phrase, retrieved);
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
		await Db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);
		var retrieved = await Db.GetBannedNamesAsync(GUILD_ID).ConfigureAwait(false);
		Assert.HasCount(2, retrieved);
	}

	[TestMethod]
	public async Task BannedPhraseCRUD_Test()
	{
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
			await Db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);

			var retrieved = (await Db.GetBannedPhrasesAsync(phrase.GuildId).ConfigureAwait(false)).Single();
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(phrase, retrieved);
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
		await Db.UpsertBannedPhraseAsync(phrase).ConfigureAwait(false);
		var retrieved = await Db.GetBannedPhrasesAsync(GUILD_ID).ConfigureAwait(false);
		Assert.HasCount(2, retrieved);
	}

	[TestMethod]
	public async Task ChannelSettingsCRUD_Test()
	{
		var settings = new ChannelSettings
		{
			GuildId = GUILD_ID,
			ChannelId = ROLE_ID,
			IsImageOnly = true,
		};

		async Task AssertEqualAsync()
		{
			await Db.UpsertChannelSettings(settings).ConfigureAwait(false);

			var retrieved = await Db!.GetChannelSettingsAsync(settings.ChannelId).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(settings, retrieved);
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
		var role = new PersistentRole
		{
			GuildId = GUILD_ID,
			RoleId = ROLE_ID,
			UserId = USER_ID,
		};
		await Db.AddPersistentRoleAsync(role).ConfigureAwait(false);

		var list = new List<PersistentRole>();
		{
			var retrieved = await Db.GetPersistentRolesAsync(role.GuildId, role.UserId).ConfigureAwait(false);
			Assert.HasCount(1, retrieved);
			list.AddRange(retrieved);
		}
		{
			var retrieved = await Db.GetPersistentRolesAsync(role.GuildId).ConfigureAwait(false);
			Assert.HasCount(1, retrieved);
			list.AddRange(retrieved);
		}

		foreach (var retrieved in list)
		{
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(role, retrieved);
		}

		await Db.DeletePersistentRoleAsync(list[0]).ConfigureAwait(false);
		var empty = await Db.GetPersistentRolesAsync(role.GuildId).ConfigureAwait(false);
		Assert.IsEmpty(empty);
	}

	[TestMethod]
	public async Task SelfRoleCRUD_Test()
	{
		var selfRole = new SelfRole
		{
			GuildId = Context.Guild.Id,
			RoleId = 73,
			GroupId = 4,
		};

		async Task AssertEqualAsync()
		{
			await Db.UpsertSelfRolesAsync([selfRole]).ConfigureAwait(false);

			var retrieved = await Db!.GetSelfRoleAsync(selfRole.RoleId).ConfigureAwait(false);
			Assert.IsNotNull(retrieved);
			Assert.AreEqual(selfRole, retrieved);
		}
		await AssertEqualAsync().ConfigureAwait(false);

		selfRole = selfRole with
		{
			GroupId = 2
		};
		await AssertEqualAsync().ConfigureAwait(false);

		await Db.UpsertSelfRolesAsync(
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

		var ret = await Db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
		Assert.HasCount(3, ret);

		await Db.DeleteSelfRolesGroupAsync(Context.Guild.Id, selfRole.GroupId).ConfigureAwait(false);
		var ret2 = await Db.GetSelfRolesAsync(Context.Guild.Id).ConfigureAwait(false);
		Assert.IsEmpty(ret2);
	}
}