using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class ChannelTopic_Tests : ParameterPrecondition_Tests<ChannelTopic>
{
	protected override ChannelTopic Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertSuccessAsync("").ConfigureAwait(false);

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length1024_Test()
		=> await AssertSuccessAsync(new string('a', 1024)).ConfigureAwait(false);

	[TestMethod]
	public async Task Length1025_Test()
		=> await AssertFailureAsync(new string('a', 1025)).ConfigureAwait(false);
}