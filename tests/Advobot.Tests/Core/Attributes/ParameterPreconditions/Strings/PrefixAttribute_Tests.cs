using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings;

[TestClass]
public sealed class PrefixAttribute_Tests
	: ParameterPreconditionTestsBase<PrefixAttribute>
{
	protected override PrefixAttribute Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertFailureAsync("").CAF();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length10_Test()
		=> await AssertSuccessAsync(new string('a', 10)).CAF();

	[TestMethod]
	public async Task Length11_Test()
		=> await AssertFailureAsync(new string('a', 11)).CAF();
}