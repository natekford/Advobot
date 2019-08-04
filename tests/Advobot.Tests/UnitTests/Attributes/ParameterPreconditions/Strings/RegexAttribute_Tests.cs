using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes.ParameterPreconditions.Strings;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class RegexAttribute_Tests
		: ParameterPreconditionsTestsBase<RegexAttribute>
	{
		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
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
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
		[TestMethod]
		public async Task InvalidRegex_Test()
		{
			var result = await CheckAsync(".{10,}*").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
		[TestMethod]
		public async Task MatchesEverything_Test()
		{
			var result = await CheckAsync(".*").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
		[TestMethod]
		public async Task MatchesNewLine_Test()
		{
			var result = await CheckAsync("\n").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
	}
}
