﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class ChannelTopicAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<ChannelTopicAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotString_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

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