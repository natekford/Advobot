﻿using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class Regex_Tests : ParameterPrecondition_Tests<Regex>
{
	protected override Regex Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").CAF();

	[TestMethod]
	public async Task InvalidRegex_Test()
		=> await AssertFailureAsync(".{10,}*").CAF();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length100_Test()
		=> await AssertSuccessAsync(new string('a', 100)).CAF();

	[TestMethod]
	public async Task Length101_Test()
		=> await AssertFailureAsync(new string('a', 101)).CAF();

	[TestMethod]
	public async Task MatchesEverything_Test()
		=> await AssertFailureAsync(".*").CAF();

	[TestMethod]
	public async Task MatchesNewLine_Test()
		=> await AssertFailureAsync("\n").CAF();
}