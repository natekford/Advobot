using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class Game_Tests : ParameterPrecondition_Tests<Game>
{
	protected override Game Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertSuccessAsync("").ConfigureAwait(false);

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length128_Test()
		=> await AssertSuccessAsync(new string('a', 128)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length129_Test()
		=> await AssertFailureAsync(new string('a', 129)).ConfigureAwait(false);
}