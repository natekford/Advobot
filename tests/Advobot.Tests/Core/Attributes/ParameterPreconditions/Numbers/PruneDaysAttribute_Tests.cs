using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class PruneDaysAttribute_Tests
		: ParameterPreconditionTestsBase<PruneDaysAttribute>
	{
		protected override PruneDaysAttribute Instance { get; } = new();

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
}