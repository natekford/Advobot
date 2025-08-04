using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class TwitchStream_Tests : ParameterPrecondition_Tests<TwitchStream>
{
	protected override TwitchStream Instance { get; } = new();

	[TestMethod]
	public async Task Asterisks_Test()
		=> await AssertFailureAsync("*****").CAF();

	[TestMethod]
	public async Task Length25_Test()
		=> await AssertSuccessAsync(new string('a', 25)).CAF();

	[TestMethod]
	public async Task Length26_Test()
		=> await AssertFailureAsync(new string('a', 26)).CAF();

	[TestMethod]
	public async Task Length3_Test()
		=> await AssertFailureAsync(new string('a', 3)).CAF();

	[TestMethod]
	public async Task Length4_Test()
		=> await AssertSuccessAsync(new string('a', 4)).CAF();
}