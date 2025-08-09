using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class EmoteName_Tests : ParameterPrecondition_Tests<EmoteName>
{
	protected override EmoteName Instance { get; } = new();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertFailureAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length2_Test()
		=> await AssertSuccessAsync(new string('a', 2)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length32_Test()
		=> await AssertSuccessAsync(new string('a', 32)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length33_Test()
		=> await AssertFailureAsync(new string('a', 33)).ConfigureAwait(false);
}