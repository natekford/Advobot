using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class Prefix_Tests : ParameterPrecondition_Tests<Prefix>
{
	protected override Prefix Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").ConfigureAwait(false);

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length10_Test()
		=> await AssertSuccessAsync(new string('a', 10)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length11_Test()
		=> await AssertFailureAsync(new string('a', 11)).ConfigureAwait(false);
}