using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class PruneDays_Tests : ParameterPrecondition_Tests<PruneDays>
{
	protected override PruneDays Instance { get; } = new();

	[TestMethod]
	public async Task Value0_Test()
		=> await AssertFailureAsync(0).CAF();

	[TestMethod]
	public async Task Value1_Test()
		=> await AssertSuccessAsync(1).CAF();

	[TestMethod]
	public async Task Value30_Test()
		=> await AssertSuccessAsync(30).CAF();

	[TestMethod]
	public async Task Value31_Test()
		=> await AssertFailureAsync(31).CAF();

	[TestMethod]
	public async Task Value7_Test()
		=> await AssertSuccessAsync(7).CAF();
}