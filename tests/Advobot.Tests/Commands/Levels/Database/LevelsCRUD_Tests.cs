﻿using Advobot.Levels.Database;
using Advobot.Levels.Models;
using Advobot.Levels.Utilities;
using Advobot.Tests.Fakes.Database;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Levels.Database;

[TestClass]
public sealed class LevelsCRUD_Tests
	: Database_Tests<LevelDatabase, FakeSQLiteConnectionString>
{
	[TestMethod]
	public async Task UserCRUD_Test()
	{
		var db = await GetDatabaseAsync().CAF();

		var args = new SearchArgs(1, 2, 3);
		var user = new User(args);
		await AssertRetrievedUserAsync(db, args, user).CAF();

		var modified = user.AddXp(5);
		await AssertRetrievedUserAsync(db, args, modified).CAF();
	}

	private async Task AssertRetrievedUserAsync(LevelDatabase db, SearchArgs args, User expected)
	{
		await db.UpsertUserAsync(expected).CAF();

		var retrieved = await db.GetUserAsync(args).CAF();
		Assert.IsNotNull(retrieved);
		Assert.AreEqual(expected.ChannelId, retrieved.ChannelId);
		Assert.AreEqual(expected.Experience, retrieved.Experience);
		Assert.AreEqual(expected.GuildId, retrieved.GuildId);
		Assert.AreEqual(expected.MessageCount, retrieved.MessageCount);
		Assert.AreEqual(expected.UserId, retrieved.UserId);
	}
}