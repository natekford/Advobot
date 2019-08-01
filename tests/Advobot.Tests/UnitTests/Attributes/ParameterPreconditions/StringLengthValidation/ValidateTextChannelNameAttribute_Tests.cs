using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.StringLengthValidation
{
	[TestClass]
	public sealed class ValidateTextChannelNameAttribute_Tests
		: ParameterPreconditionsTestsBase<ValidateTextChannelNameAttribute>
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
				{ new string('a', 1), false },
				{ new string('a', 2), true },
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
		public async Task Space_Test()
		{
			var result = await CheckAsync("name with spaces").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
	}
}
