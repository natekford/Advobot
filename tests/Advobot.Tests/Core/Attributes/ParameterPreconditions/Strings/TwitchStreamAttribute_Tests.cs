using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Strings
{
	[TestClass]
	public sealed class TwitchStreamAttribute_Tests
		: ParameterPreconditionTestsBase<TwitchStreamAttribute>
	{
		protected override TwitchStreamAttribute Instance { get; } = new();

		[TestMethod]
		public async Task InvalidName_Test()
		{
			var result = await CheckPermissionsAsync("*****").CAF();
			Assert.IsFalse(result.IsSuccess);
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
				var result = await CheckPermissionsAsync(kvp.Key).CAF();
				Assert.AreEqual(kvp.Value, result.IsSuccess);
			}
		}
	}
}