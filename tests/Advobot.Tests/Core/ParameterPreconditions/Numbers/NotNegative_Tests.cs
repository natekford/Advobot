using Advobot.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class NotNegative_Tests : ParameterPrecondition_Tests<NotNegative>
{
	protected override NotNegative Instance { get; } = new();

	[TestMethod]
	public async Task MaxValue_Test()
		=> await AssertSuccessAsync(int.MaxValue).CAF();

	[TestMethod]
	public async Task Negative_Test()
		=> await AssertFailureAsync(-1).CAF();

	[TestMethod]
	public async Task Positive_Test()
		=> await AssertSuccessAsync(1).CAF();

	[TestMethod]
	public async Task Zero_Test()
		=> await AssertSuccessAsync(0).CAF();
}