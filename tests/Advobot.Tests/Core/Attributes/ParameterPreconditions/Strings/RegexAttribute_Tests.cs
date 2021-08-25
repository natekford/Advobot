
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class RegexAttribute_Tests
		: ParameterPreconditionTestsBase<RegexAttribute>
	{
		protected override RegexAttribute Instance { get; } = new();

		[TestMethod]
		public async Task InvalidRegex_Test()
		{
			var result = await CheckPermissionsAsync(".{10,}*").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task MatchesEverything_Test()
		{
			var result = await CheckPermissionsAsync(".*").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task MatchesNewLine_Test()
		{
			var result = await CheckPermissionsAsync("\n").CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<string, bool>
			{
				{ "", false },
				{ new string('a', 1), true },
				{ new string('a', 100), true },
				{ new string('a', 101), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}