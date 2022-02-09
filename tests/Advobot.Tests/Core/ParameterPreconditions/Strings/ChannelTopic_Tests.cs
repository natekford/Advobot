using Advobot.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Strings;

[TestClass]
public sealed class ChannelTopic_Tests : ParameterPrecondition_Tests<ChannelTopic>
{
	protected override ChannelTopic Instance { get; } = new();

	[TestMethod]
	public async Task Empty_Test()
		=> await AssertSuccessAsync("").CAF();

	[TestMethod]
	public async Task Length1_Test()
		=> await AssertSuccessAsync(new string('a', 1)).CAF();

	[TestMethod]
	public async Task Length1024_Test()
		=> await AssertSuccessAsync(new string('a', 1024)).CAF();

	[TestMethod]
	public async Task Length1025_Test()
		=> await AssertFailureAsync(new string('a', 1025)).CAF();
}