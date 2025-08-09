using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class PruneDays_Tests : ParameterPrecondition_Tests<PruneDays>
{
	protected override PruneDays Instance { get; } = new();

	[TestMethod]
	public async Task Value0_Test()
		=> await AssertFailureAsync(0).ConfigureAwait(false);

	[TestMethod]
	public async Task Value1_Test()
		=> await AssertSuccessAsync(1).ConfigureAwait(false);

	[TestMethod]
	public async Task Value30_Test()
		=> await AssertSuccessAsync(30).ConfigureAwait(false);

	[TestMethod]
	public async Task Value31_Test()
		=> await AssertFailureAsync(31).ConfigureAwait(false);

	[TestMethod]
	public async Task Value7_Test()
		=> await AssertSuccessAsync(7).ConfigureAwait(false);
}