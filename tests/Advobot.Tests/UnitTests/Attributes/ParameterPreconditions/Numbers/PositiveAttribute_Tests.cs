﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class Positive_Tests
		: ParameterPreconditionsTestsBase<PositiveAttribute>
	{
		[TestMethod]
		public async Task Standard_Test()
		{
			var expected = new Dictionary<int, bool>
			{
				{ -1, false },
				{ 1, true },
				{ int.MaxValue, true },
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