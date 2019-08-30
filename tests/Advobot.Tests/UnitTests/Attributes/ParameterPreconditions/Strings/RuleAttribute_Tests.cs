using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class RuleAttribute_Tests
		: ParameterPreconditionsTestsBase<RuleAttribute>
	{
		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<string, bool>
			{
				{ "", false },
				{ new string('a', 1), true },
				{ new string('a', 150), true },
				{ new string('a', 151), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task ThrowsOnNotString_Test()
		{
			Task Task() => CheckAsync(1);
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
	}
}