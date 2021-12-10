using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers;

[TestClass]
public sealed class ChannelLimitAttribute_Tests
	: ParameterPreconditionTestsBase<ChannelLimitAttribute>
{
	protected override ChannelLimitAttribute Instance { get; } = new();

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