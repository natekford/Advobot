﻿using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class InviteTypeReader_Tests : TypeReader_Tests<InviteTypeReader>
{
	protected override InviteTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task Valid_Test()
	{
		var invite = await Context.Channel.CreateInviteAsync().CAF();

		var result = await ReadAsync(invite.Code).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IInviteMetadata));
	}
}