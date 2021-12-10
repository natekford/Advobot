﻿using Advobot.Attributes.Preconditions;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions;

[TestClass]
public sealed class RequirePartneredGuildAttribute_Tests : PreconditionTestsBase
{
	protected override PreconditionAttribute Instance { get; } = new RequirePartneredGuildAttribute();

	[TestMethod]
	public async Task IsNotPartnered_Test()
	{
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task IsPartnered_Test()
	{
		Context.Guild.Features = new GuildFeaturesCreationArgs
		{
			Value = GuildFeature.Partnered,
		}.Build();

		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}
}