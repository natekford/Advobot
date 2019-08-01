using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Attributes.ParameterPreconditions.StringLengthValidation;
using AdvorangesUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.UnitTests.Attributes.ParameterPreconditions.StringLengthValidation
{
	[TestClass]
	public sealed class ValidateTwitchStreamAttribute_Tests
		: ParameterPreconditionsTestsBase<ValidateTwitchStreamAttribute>
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
				{ new string('a', 3), false },
				{ new string('a', 4), true },
				{ new string('a', 25), true },
				{ new string('a', 26), false },
			};
			foreach (var kvp in expected)
			{
				var result = await CheckAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
		[TestMethod]
		public async Task InvalidName_Test()
		{
			var result = await CheckAsync("*****").CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
	}
}
