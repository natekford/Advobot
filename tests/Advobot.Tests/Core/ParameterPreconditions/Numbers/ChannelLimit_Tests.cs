using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class ChannelLimit_Tests : ParameterPrecondition_Tests<ChannelLimit>
{
	protected override ChannelLimit Instance { get; } = new();

	[TestMethod]
	public async Task Negative_Test()
		=> await AssertFailureAsync(-1).CAF();

	[TestMethod]
	public async Task Value0_Test()
		=> await AssertSuccessAsync(0).CAF();

	[TestMethod]
	public async Task Value100_Test()
		=> await AssertFailureAsync(100).CAF();

	[TestMethod]
	public async Task Value99_Test()
		=> await AssertSuccessAsync(1).CAF();
}