using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class TextChannelName_Tests : ParameterPrecondition_Tests<TextChannelName>
{
	protected override TextChannelName Instance { get; } = new();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertFailureAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length100_Test()
		=> await AssertSuccessAsync(new string('a', 100)).CAF();

	[TestMethod]
	public async Task Length101_Test()
		=> await AssertFailureAsync(new string('a', 101)).CAF();

	[TestMethod]
	public async Task Length2_Test()
		=> await AssertSuccessAsync(new string('a', 2)).CAF();

	[TestMethod]
	public async Task Space_Test()
		=> await AssertFailureAsync("name with spaces").CAF();
}