using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class GuildAfkTime_Tests : ParameterPrecondition_Tests<GuildAfkTime>
{
	protected override GuildAfkTime Instance { get; } = new();

	[TestMethod]
	public async Task Value1800_Test()
		=> await AssertSuccessAsync(1800).CAF();

	[TestMethod]
	public async Task Value300_Test()
		=> await AssertSuccessAsync(300).CAF();

	[TestMethod]
	public async Task Value3600_Test()
		=> await AssertSuccessAsync(3600).CAF();

	[TestMethod]
	public async Task Value3601_Test()
		=> await AssertFailureAsync(3601).CAF();

	[TestMethod]
	public async Task Value59_Test()
		=> await AssertFailureAsync(59).CAF();

	[TestMethod]
	public async Task Value60_Test()
		=> await AssertSuccessAsync(60).CAF();

	[TestMethod]
	public async Task Value900_Test()
		=> await AssertSuccessAsync(900).CAF();
}