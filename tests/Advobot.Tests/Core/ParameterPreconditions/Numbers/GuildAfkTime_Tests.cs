using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class GuildAfkTime_Tests : ParameterPrecondition_Tests<GuildAfkTime>
{
	protected override GuildAfkTime Instance { get; } = new();

	[TestMethod]
	public async Task Value1800_Test()
		=> await AssertSuccessAsync(1800).ConfigureAwait(false);

	[TestMethod]
	public async Task Value300_Test()
		=> await AssertSuccessAsync(300).ConfigureAwait(false);

	[TestMethod]
	public async Task Value3600_Test()
		=> await AssertSuccessAsync(3600).ConfigureAwait(false);

	[TestMethod]
	public async Task Value3601_Test()
		=> await AssertFailureAsync(3601).ConfigureAwait(false);

	[TestMethod]
	public async Task Value59_Test()
		=> await AssertFailureAsync(59).ConfigureAwait(false);

	[TestMethod]
	public async Task Value60_Test()
		=> await AssertSuccessAsync(60).ConfigureAwait(false);

	[TestMethod]
	public async Task Value900_Test()
		=> await AssertSuccessAsync(900).ConfigureAwait(false);
}