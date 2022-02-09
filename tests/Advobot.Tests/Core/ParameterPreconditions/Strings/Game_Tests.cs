using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class Game_Tests : ParameterPrecondition_Tests<Game>
{
	protected override Game Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertSuccessAsync("").CAF();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length128_Test()
		=> await AssertSuccessAsync(new string('a', 128)).CAF();

	[TestMethod]
	public async Task Length129_Test()
		=> await AssertFailureAsync(new string('a', 129)).CAF();
}