using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class NicknameAttribute_Tests
		: ParameterPreconditionTestsBase<NicknameAttribute>
	{
		protected override NicknameAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Empty_Test()
			=> await AssertFailureAsync("").CAF();

		[TestMethod]
		public async Task Length1_Test()
			=> await AssertSuccessAsync(new string('a', 1)).CAF();

		[TestMethod]
		public async Task Length32_Test()
			=> await AssertSuccessAsync(new string('a', 32)).CAF();

		[TestMethod]
		public async Task Length33_Test()
			=> await AssertFailureAsync(new string('a', 33)).CAF();
	}
}