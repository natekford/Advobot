using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class GuildAfkTimeAttribute_Tests
		: ParameterPreconditionsTestsBase<GuildAfkTimeAttribute>
	{
		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ 59, false },
				{ 60, true },
				{ 300, true },
				{ 900, true },
				{ 1800, true },
				{ 3600, true },
				{ 3601, false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}

		[TestMethod]
		public async Task ThrowsOnNotInt_Test()
		{
			Task Task() => CheckAsync("not int");
			await Assert.ThrowsExceptionAsync<ArgumentException>(Task).CAF();
		}
	}
}