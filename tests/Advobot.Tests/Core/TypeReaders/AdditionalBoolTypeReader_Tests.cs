﻿using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class AdditionalBoolTypeReader_Tests
	: TypeReader_Tests<AdditionalBoolTypeReader>
{
	protected override AdditionalBoolTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task FalseValues_Test()
	{
		foreach (var value in AdditionalBoolTypeReader.FalseVals)
		{
			var result = await ReadAsync(value).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
			Assert.IsFalse((bool)result.BestMatch);
		}
	}

	[TestMethod]
	public async Task TrueValues_Test()
	{
		foreach (var value in AdditionalBoolTypeReader.TrueVals)
		{
			var result = await ReadAsync(value).CAF();
			Assert.IsTrue(result.IsSuccess);
			Assert.IsInstanceOfType(result.BestMatch, typeof(bool));
			Assert.IsTrue((bool)result.BestMatch);
		}
	}
}