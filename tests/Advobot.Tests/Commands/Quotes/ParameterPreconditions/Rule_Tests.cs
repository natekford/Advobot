using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions;

[TestClass]
public sealed class Rule_Tests : ParameterPrecondition_Tests<Rule>
{
	protected override Rule Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").ConfigureAwait(false);

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length500_Test()
		=> await AssertSuccessAsync(new string('a', 500)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length501_Test()
		=> await AssertFailureAsync(new string('a', 501)).ConfigureAwait(false);
}