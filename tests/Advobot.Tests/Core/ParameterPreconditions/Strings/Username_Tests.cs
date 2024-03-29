﻿using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class Username_Tests : ParameterPrecondition_Tests<Username>
{
	protected override Username Instance { get; } = new();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertFailureAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length2_Test()
		=> await AssertSuccessAsync(new string('a', 2)).CAF();

	[TestMethod]
	public async Task Length32_Test()
		=> await AssertSuccessAsync(new string('a', 32)).CAF();

	[TestMethod]
	public async Task Length33_Test()
		=> await AssertFailureAsync(new string('a', 33)).CAF();
}