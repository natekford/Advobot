﻿using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class BanTypeReader_Tests : TypeReader_Tests<BanTypeReader>
{
	protected override BanTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task InvalidMultipleMatches_Test()
	{
		const string NAME = "bob";

		{
			var user = new FakeGuildUser(Context.Guild)
			{
				Username = NAME,
			};
			await Context.Guild.AddBanAsync(user).CAF();
		}
		{
			var user = new FakeGuildUser(Context.Guild)
			{
				Username = NAME,
			};
			await Context.Guild.AddBanAsync(user).CAF();
		}

		var result = await ReadAsync(NAME).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task ValidId_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		await Context.Guild.AddBanAsync(user).CAF();

		var result = await ReadAsync(user.Id.ToString()).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IBan));
		var ban = (IBan)result.BestMatch;
		Assert.AreEqual(user.Id, ban.User.Id);
	}

	[TestMethod]
	public async Task ValidMention_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		await Context.Guild.AddBanAsync(user).CAF();

		var result = await ReadAsync(user.Mention).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IBan));
		var ban = (IBan)result.BestMatch;
		Assert.AreEqual(user.Id, ban.User.Id);
	}
}