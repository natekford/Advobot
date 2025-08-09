using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class GuildName_Tests : ParameterPrecondition_Tests<GuildName>
{
	protected override GuildName Instance { get; } = new();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertFailureAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length100_Test()
		=> await AssertSuccessAsync(new string('a', 100)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length101_Test()
		=> await AssertFailureAsync(new string('a', 101)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length2_Test()
		=> await AssertSuccessAsync(new string('a', 2)).ConfigureAwait(false);
}