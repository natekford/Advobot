﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes.ParameterPreconditions.Strings;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class ChannelTopicAttribute_Tests
		: ParameterPreconditionsTestsBase<ChannelTopicAttribute>
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
				{ "", true },
				{ new string('a', 1), true },
				{ new string('a', 1024), true },
				{ new string('a', 1025), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}