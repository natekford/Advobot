﻿using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class BypassUserLimitTypeReader_Tests
	: TypeReader_Tests<BypassUserLimitTypeReader>
{
	protected override BypassUserLimitTypeReader Instance { get; } = new();
	protected override string? NotExisting => null;

	[TestMethod]
	public async Task Invalid_Test()
	{
		var result = await ReadAsync("asdf").CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
		Assert.IsFalse((bool)result.BestMatch);
	}

	[TestMethod]
	public async Task Valid_Test()
	{
		var result = await ReadAsync(BypassUserLimitTypeReader.BYPASS_STRING).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
		Assert.IsTrue((bool)result.BestMatch);
	}
}