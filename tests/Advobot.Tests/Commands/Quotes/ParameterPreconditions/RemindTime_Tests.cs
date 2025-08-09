using Advobot.Quotes.ParameterPreconditions;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Quotes.ParameterPreconditions;

[TestClass]
public sealed class RemindTime_Tests : ParameterPrecondition_Tests<RemindTime>
{
	protected override RemindTime Instance { get; } = new();

	[TestMethod]
	public async Task Value0_Test()
		=> await AssertFailureAsync(0).ConfigureAwait(false);

	[TestMethod]
	public async Task Value1_Test()
		=> await AssertSuccessAsync(1).ConfigureAwait(false);

	[TestMethod]
	public async Task Value525600_Test()
		=> await AssertSuccessAsync(525600).ConfigureAwait(false);

	[TestMethod]
	public async Task Value525601_Test()
		=> await AssertFailureAsync(525601).ConfigureAwait(false);
}