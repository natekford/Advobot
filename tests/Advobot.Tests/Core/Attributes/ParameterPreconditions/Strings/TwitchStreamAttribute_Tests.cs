using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings;

[TestClass]
public sealed class TwitchStreamAttribute_Tests
	: ParameterPreconditionTestsBase<TwitchStreamAttribute>
{
	protected override TwitchStreamAttribute Instance { get; } = new();

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