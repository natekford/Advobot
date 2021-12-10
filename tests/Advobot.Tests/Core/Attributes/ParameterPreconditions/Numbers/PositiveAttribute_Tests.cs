using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers;

[TestClass]
public sealed class Positive_Tests
	: ParameterPreconditionTestsBase<PositiveAttribute>
{
	protected override PositiveAttribute Instance { get; } = new();

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
		=> await AssertFailureAsync(0).CAF();
}