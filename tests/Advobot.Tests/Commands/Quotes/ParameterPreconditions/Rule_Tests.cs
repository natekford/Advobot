using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions;

[TestClass]
public sealed class Rule_Tests : ParameterPrecondition_Tests<Rule>
{
	protected override Rule Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").CAF();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length500_Test()
		=> await AssertSuccessAsync(new string('a', 500)).CAF();

	[TestMethod]
	public async Task Length501_Test()
		=> await AssertFailureAsync(new string('a', 501)).CAF();
}